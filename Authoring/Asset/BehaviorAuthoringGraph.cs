using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace Unity.Behavior
{
    /// <summary>
    /// The primary asset type used by Unity Behavior. The asset contains the authoring representation of the behavior
    /// graph. 
    /// </summary>
    [Serializable]
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "Behavior Graph", menuName = "Behavior/Behavior Graph")]
#endif
    internal class BehaviorAuthoringGraph : GraphAsset, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeReference]
        private BehaviorGraphDebugInfo m_DebugInfo;
        public BehaviorGraphDebugInfo DebugInfo
        {
            get
            {
                if (m_DebugInfo == null)
                {
                    m_DebugInfo = GetOrCreateGraphDebugInfo(this);
                }
                return m_DebugInfo;
            }
        }
#endif
        [SerializeField]
        public SerializableGUID AssetID = SerializableGUID.Generate();
        private long m_LastSerializedTimestamp;

        [SerializeField]
        public StoryInfo Story = new StoryInfo();

        [NonSerialized]
        private List<NodeModel> m_RootNodes = new List<NodeModel>();

        public List<NodeModel> Roots
        {
            get
            {
                m_RootNodes.Clear();
                for (var i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] is BehaviorGraphNodeModel { IsRoot: true })
                    {
                        m_RootNodes.Add(Nodes[i]);
                    }
                }

                return m_RootNodes;
            }
        }

        [SerializeField]
        private List<NodeModelInfo> m_NodeModelsInfo;
        public IReadOnlyList<NodeModelInfo> NodeModelsInfo => m_NodeModelsInfo;

        private Dictionary<SerializableGUID, NodeModelInfo> m_RuntimeNodeTypeIDToNodeModelInfo;
        public IReadOnlyDictionary<SerializableGUID, NodeModelInfo> RuntimeNodeTypeIDToNodeModelInfo => m_RuntimeNodeTypeIDToNodeModelInfo;
        
        [SerializeField]
        internal List<BehaviorBlackboardAuthoringAsset> m_Blackboards = new List<BehaviorBlackboardAuthoringAsset>();
        
        [Serializable]
        public struct NodeModelInfo
        {
            public string Name;
            public string Story;
            public SerializableGUID RuntimeTypeID;
            public List<VariableInfo> Variables;
            public List<string> NamedChildren;
        }
               
        [SerializeField]
        private SerializableCommandBuffer m_CommandBuffer = new SerializableCommandBuffer();        
        public SerializableCommandBuffer CommandBuffer => m_CommandBuffer;

        private void Awake()
        {
#if UNITY_EDITOR
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(GetInstanceID()));
            AssetID = new SerializableGUID(guid);
#endif
            BehaviorGraphAssetRegistry.Add(this);
        }

        /// <inheritdoc cref="OnValidate" />
        public override void OnValidate()
        {
            EnsureAtLeastOneRoot();
            EnsureStoryVariablesExist();
            EnsureAssetReferenceOnConditions();
            EnsureBlackboardsAreUpToDate();

            base.OnValidate();
        }

        private void EnsureBlackboardsAreUpToDate()
        {
            for (int i = m_Blackboards.Count - 1; i >= 0; i--)
            {
                BehaviorBlackboardAuthoringAsset blackboard = m_Blackboards[i];
                if (blackboard == null)
                {
                    m_Blackboards.RemoveAt(i);
                }
                else
                {
                    blackboard.OnValidate();
                }
            }
        }

        private void EnsureAtLeastOneRoot()
        {
            // Add a start node if no root exists.
            if (Roots.Count == 0)
            {
                var newStart = new StartNodeModel(NodeRegistry.GetInfo(typeof(Start)));
                newStart.OnDefineNode();
                Nodes.Add(newStart);
            }
        }

        internal void EnsureCorrectModelTypes()
        {
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                NodeModel nodeModel = Nodes[i];
                if (nodeModel is not BehaviorGraphNodeModel node || nodeModel is PlaceholderNodeModel)
                {
                    continue;
                }
                NodeInfo info = NodeRegistry.GetInfoFromTypeID(node.NodeTypeID);
                if (info == null)
                {
                    ReplaceNodeWithPlaceholder(node);
                    Debug.LogWarning($"Serialized node: '{node.NodeType}' is invalid. " +
                        $"This generally happen when you rename or delete a node that was used by the graph. " +
                        $"The faulty node has been been replaced with a placeholder node.", this);
                }
                else if (node.NodeType.text != info.Type.AssemblyQualifiedName)
                {
                    node.NodeType = info.Type;
                }
            }
        }

        private void ReplaceNodeWithPlaceholder(BehaviorGraphNodeModel nodeModel)
        {
            if (RuntimeNodeTypeIDToNodeModelInfo.TryGetValue(nodeModel.NodeTypeID, out NodeModelInfo modelInfo) == false)
            {
                modelInfo = new NodeModelInfo();
                modelInfo.Name = "Missing Node";
                modelInfo.Story = "Missing Node";
            }
            modelInfo.RuntimeTypeID = nodeModel.NodeTypeID;
            PlaceholderNodeModel placeholderNodeModel = new PlaceholderNodeModel(modelInfo);
            if (typeof(CompositeNodeModel).IsAssignableFrom(nodeModel.GetType()))
            {
                placeholderNodeModel.PlaceholderType = PlaceholderNodeModel.PlaceholderNodeType.Composite;
            }
            else if (typeof(ModifierNodeModel).IsAssignableFrom(nodeModel.GetType()))
            {
                placeholderNodeModel.PlaceholderType = PlaceholderNodeModel.PlaceholderNodeType.Modifier;
            }
            else
            {
                placeholderNodeModel.PlaceholderType = PlaceholderNodeModel.PlaceholderNodeType.Action;
            }
            placeholderNodeModel.m_FieldValues = nodeModel.m_FieldValues;
            ReplaceNode(nodeModel, placeholderNodeModel);
        }

        private void ReplaceNode(BehaviorGraphNodeModel oldNodeModel, BehaviorGraphNodeModel newNodeModel)
        {
            newNodeModel.ID = oldNodeModel.ID;
            newNodeModel.Position = oldNodeModel.Position;
            newNodeModel.Asset = oldNodeModel.Asset;

            int nodeIndex = Nodes.FindIndex(node => node == oldNodeModel);
            if (nodeIndex != -1)
            {
                Nodes[nodeIndex] = newNodeModel;
            }

            foreach (NodeModel parent in oldNodeModel.Parents)
            {
                if (parent is SequenceNodeModel sequence)
                {
                    int childIndex = sequence.Nodes.FindIndex(node => node == oldNodeModel);
                    if (childIndex != -1)
                    {
                        sequence.Nodes[childIndex] = newNodeModel;
                    }
                }
                newNodeModel.Parents.Add(parent);
            }

            newNodeModel.PortModels.Clear();
            foreach (PortModel portModel in oldNodeModel.AllPortModels)
            {
                portModel.NodeModel = newNodeModel;
                newNodeModel.PortModels.Add(portModel);

                if (portModel.IsFloating)
                {
                    foreach (var connection in portModel.Connections)
                    {
                        if (connection.NodeModel is FloatingPortNodeModel floatingPortNodeModel)
                        {
                            
                        }
                    }
                }
            }
        }

        private void EnsureStoryVariablesExist()
        {
            Story ??= new StoryInfo();

            List<VariableInfo> storyVariables = Story.Variables;
            List<VariableModel> assetVariables = Blackboard.Variables;
            foreach (VariableInfo storyVariable in storyVariables)
            {
                if (!assetVariables.Any(assetVar =>
                        string.Equals(assetVar.Name, storyVariable.Name, StringComparison.CurrentCultureIgnoreCase)
                        && assetVar.Type == (Type)storyVariable.Type))
                {
                    string varName = char.ToUpper(storyVariable.Name.First()) + storyVariable.Name.Substring(1);
                    Type varType = BlackboardUtils.GetVariableModelTypeForType(storyVariable.Type);
                    VariableModel variable = Activator.CreateInstance(varType) as VariableModel;
                    variable.Name = varName;
                    Blackboard.Variables.Add(variable);
                }
            }
        }

        private void EnsureAssetReferenceOnConditions()
        {
            foreach (NodeModel node in Nodes)
            {
                if (node is not IConditionalNodeModel conditionalNode)
                {
                    continue;
                }
                foreach (ConditionModel condition in conditionalNode.ConditionModels)
                {
                    condition.Asset = this;
                }
            }
        }

#if UNITY_EDITOR
        [OnOpenAsset(1)]
        public static bool step1(int instanceID, int line)
        {
            BehaviorAuthoringGraph asset = EditorUtility.InstanceIDToObject(instanceID) as BehaviorAuthoringGraph;
            if (asset == null)
            {
                BehaviorGraph runtimeGraph = EditorUtility.InstanceIDToObject(instanceID) as BehaviorGraph;
                if (runtimeGraph == null)
                {
                    return false;
                }
                if (!AssetDatabase.IsMainAsset(runtimeGraph) && AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(runtimeGraph)) is BehaviorAuthoringGraph parentAsset)
                {
                    asset = parentAsset;
                }
                else
                {
                    return false;
                }
            }
            BehaviorWindowDelegate.Open(asset);
            return true; // we did not handle the open
        }
#endif

        public static BehaviorGraph GetOrCreateGraph(BehaviorAuthoringGraph assetObject)
        {
#if !UNITY_EDITOR
            return CreateInstance<BehaviorGraph>();
#else
            string assetPath = AssetDatabase.GetAssetPath(assetObject);
            if (!EditorUtility.IsPersistent(assetObject) || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            BehaviorGraph graph = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .FirstOrDefault(asset => asset is BehaviorGraph) as BehaviorGraph;
            if (graph != null)
            {
                return graph;
            }

            graph = CreateInstance<BehaviorGraph>();
            graph.name = assetObject.name;
            AssetDatabase.AddObjectToAsset(graph, assetObject);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(assetObject);
            return graph;
#endif
        }

#if UNITY_EDITOR
        public static BehaviorGraphDebugInfo GetOrCreateGraphDebugInfo(BehaviorAuthoringGraph assetObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(assetObject);
            if (!EditorUtility.IsPersistent(assetObject) || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            BehaviorGraph graph = GetOrCreateGraph(assetObject);
            string graphPath = AssetDatabase.GetAssetPath(graph);

            BehaviorGraphDebugInfo debugInfo = AssetDatabase.LoadAssetAtPath<BehaviorGraphDebugInfo>(graphPath);
            if (!debugInfo)
            {
                debugInfo = CreateInstance<BehaviorGraphDebugInfo>();
                debugInfo.name = assetObject.name + " Debug Info";
                AssetDatabase.AddObjectToAsset(debugInfo, graph);
                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(assetObject);
            }
            assetObject.m_DebugInfo = debugInfo;
            return debugInfo;
        }
#endif
        /// <inheritdoc cref="OnBeforeSerialize"/>
        public void OnBeforeSerialize()
        {
            if (VersionTimestamp == m_LastSerializedTimestamp)
            {
                return;
            }

            CreateNodeModelsInfoCache();
            m_LastSerializedTimestamp = VersionTimestamp;
        }

        /// <inheritdoc cref="OnAfterDeserialize"/>
        public void OnAfterDeserialize()
        {
            CreateNodeModelInfosDictionaryFromList();
        }

        private void CreateNodeModelsInfoCache()
        {
            m_RuntimeNodeTypeIDToNodeModelInfo ??= new Dictionary<SerializableGUID, NodeModelInfo>();
            m_NodeModelsInfo ??= new List<NodeModelInfo>();
            m_RuntimeNodeTypeIDToNodeModelInfo.Clear();
            m_NodeModelsInfo.Clear();
            foreach (NodeModel node in Nodes)
            {
                if (node is not BehaviorGraphNodeModel behaviorNode)
                {
                    continue;
                }
                NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorNode.NodeTypeID);
                if (nodeInfo == null || m_RuntimeNodeTypeIDToNodeModelInfo.ContainsKey(nodeInfo.TypeID))
                {
                    continue;
                }
                NodeModelInfo nodeModelInfo = new NodeModelInfo
                {
                    Name = nodeInfo.Name,
                    Story = nodeInfo.Story,
                    RuntimeTypeID = nodeInfo.TypeID,
                    Variables = nodeInfo.Variables.Select(variable => new VariableInfo
                    {
                        Name = variable.Name,
                        Type = (typeof(BlackboardVariable).IsAssignableFrom(variable.Type.Type) && variable.Type.Type.IsGenericType) ? variable.Type.Type.GenericTypeArguments[0] : variable.Type
                    }).ToList(),
                    NamedChildren = nodeInfo.NamedChildren
                };
                m_RuntimeNodeTypeIDToNodeModelInfo[nodeInfo.TypeID] = nodeModelInfo;
                m_NodeModelsInfo.Add(nodeModelInfo);
            }
        }

        private void CreateNodeModelInfosDictionaryFromList()
        {
            if (m_RuntimeNodeTypeIDToNodeModelInfo == null)
            {
                m_RuntimeNodeTypeIDToNodeModelInfo = new Dictionary<SerializableGUID, NodeModelInfo>();
            }
            if (m_NodeModelsInfo == null)
            {
                return;
            }
            foreach (NodeModelInfo nodeModelInfo in m_NodeModelsInfo)
            {
                m_RuntimeNodeTypeIDToNodeModelInfo[nodeModelInfo.RuntimeTypeID] = nodeModelInfo;
            }
        }
        
        internal override void EnsureAssetHasBlackboard()
        {
            string blackboardName = name + " Blackboard Editor";
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(this);
            BlackboardAsset existingBlackboard = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(asset => asset is BlackboardAsset) as BlackboardAsset;
            BehaviorBlackboardAuthoringAsset blackboardAuthoring = existingBlackboard as BehaviorBlackboardAuthoringAsset;
            
            if (blackboardAuthoring == null)
            {
                blackboardAuthoring = CreateInstance<BehaviorBlackboardAuthoringAsset>();
                
                Blackboard = blackboardAuthoring;
                Blackboard.name = blackboardName;

                if (existingBlackboard != null)
                {
                    Blackboard.AssetID = existingBlackboard.AssetID;
                    Blackboard.Variables = existingBlackboard.Variables;
                    AssetDatabase.RemoveObjectFromAsset(existingBlackboard);
                }
            }
            else if(blackboardAuthoring != null)
            {
                if (Blackboard != null && blackboardAuthoring == Blackboard)
                {
                    // Update the graph Blackboard name if needed.
                    if (blackboardAuthoring.name != blackboardName)
                    {
                        blackboardAuthoring.name = blackboardName;
                    }   
                }
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
                
            AssetDatabase.AddObjectToAsset(Blackboard, this);
            AssetDatabase.SaveAssets();
            
            blackboardAuthoring.BuildRuntimeBlackboard();

            EditorUtility.SetDirty(this);
#endif
        }

        public override string ToString() => name;

        public void OnDestroy()
        {
            BehaviorGraphAssetRegistry.Remove(this);
        }

        public BehaviorGraph BuildRuntimeGraph()
        {
            var runtimeGraph = GetOrCreateGraph(this);
            if (runtimeGraph == null)
            {
                return null;
            }
            
            runtimeGraph.Graphs.Clear();
            var graphAssetProcessor = new GraphAssetProcessor(this, runtimeGraph);
            graphAssetProcessor.ProcessGraph();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif                
            return runtimeGraph;
        }
    }
}