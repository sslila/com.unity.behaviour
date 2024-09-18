using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    [NodeModelInfo(typeof(RunSubgraph))]
    internal class SubgraphNodeModel : BehaviorGraphNodeModel
    {
        internal const string k_SubgraphFieldName = "Subgraph";
        internal const string k_BlackboardFieldName = "Blackboard";
        
        public override bool IsSequenceable => true;

        [SerializeField] private SerializableGUID m_SubgraphAssetId;
        
        public bool IsDynamic
        {
            get
            {
                UpdateIsDynamic();
                return m_IsDynamic;
            }
            private set => m_IsDynamic = value;
        }

        [SerializeField]
        private bool m_IsDynamic;
        
        public BehaviorGraph RuntimeSubgraph => GetLinkedSubgraph();
        
        public bool ShowStaticSubgraphRepresentation
        {
            get => m_ShowStaticSubgraphRepresentation;
            set => m_ShowStaticSubgraphRepresentation = value;
        }

        [SerializeField]
        private bool m_ShowStaticSubgraphRepresentation;

        private BehaviorGraph GetLinkedSubgraph()
        {
            TypedVariableModel<BehaviorGraph> linkedVariable = SubgraphField.LinkedVariable as TypedVariableModel<BehaviorGraph>;
            if (linkedVariable != null)
            {
                return linkedVariable.m_Value;
            }

            return null;
        }

        public BehaviorAuthoringGraph SubgraphAuthoringAsset => GetAsset();
        
        public BehaviorBlackboardAuthoringAsset RequiredBlackboard => GetBlackboardAsset();

        internal FieldModel SubgraphField => Fields.FirstOrDefault(field => field.FieldName == k_SubgraphFieldName);
        internal FieldModel BlackboardAssetField => Fields.FirstOrDefault(field => field.FieldName == k_BlackboardFieldName);

        [SerializeReference]
        internal List<FieldModel> m_StoryFields = new ();

        public SubgraphNodeModel(NodeInfo nodeInfo) : base(nodeInfo) { }
        
        protected SubgraphNodeModel(SubgraphNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
            GetOrCreateField(k_SubgraphFieldName, typeof(BehaviorGraph));
            if (nodeModelOriginal.RuntimeSubgraph != null)
            {
                SubgraphField.LinkedVariable = nodeModelOriginal.SubgraphField.LinkedVariable;
            }
            
            GetOrCreateField(k_BlackboardFieldName, typeof(BehaviorBlackboardAuthoringAsset));
            if (nodeModelOriginal.RequiredBlackboard != null)
            {
                BlackboardAssetField.LinkedVariable = nodeModelOriginal.BlackboardAssetField.LinkedVariable;
            }
        }

        public override void OnDefineNode()
        {
            base.OnDefineNode();
            GetOrCreateField(k_SubgraphFieldName, typeof(BehaviorGraph));
            GetOrCreateField(k_BlackboardFieldName, typeof(BehaviorBlackboardAuthoringAsset));
            UpdateIsDynamic();
        }
        
        private BehaviorAuthoringGraph GetAsset()
        {
            if (SubgraphField.LinkedVariable == null)
            {
                return null;
            }

#if UNITY_EDITOR
            BehaviorAuthoringGraph asset = BehaviorGraphAssetRegistry.TryGetAssetFromGraphPath(RuntimeSubgraph);
#else
            BehaviorAuthoringGraph asset = null;
#endif

            return asset;
        }
        
        private BehaviorBlackboardAuthoringAsset GetBlackboardAsset()
        {
            if (BlackboardAssetField?.LinkedVariable != null)
            {
                return BlackboardAssetField.LinkedVariable.ObjectValue as BehaviorBlackboardAuthoringAsset;
            }

            return null;
        }

        private BehaviorAuthoringGraph GetAssetFromID(SerializableGUID assetID)
        {
            return BehaviorGraphAssetRegistry.GlobalRegistry.Assets.FirstOrDefault(asset => asset.AssetID == assetID);
        }

        protected override void EnsureFieldValuesAreUpToDate()
        {
            if (SubgraphField == null)
            {
                GetOrCreateField(k_SubgraphFieldName, typeof(BehaviorGraph));
            }

            if (BlackboardAssetField == null)
            {
                GetOrCreateField(k_BlackboardFieldName, typeof(BehaviorBlackboardAuthoringAsset));
            }
            
            if (SubgraphField?.LinkedVariable == null)
            {
                // No subgraph is assigned, so remove variable fields and set the node back to static.
                ClearFields();
                IsDynamic = false;
                return;
            }

            if (!RuntimeSubgraph || !SubgraphAuthoringAsset || SubgraphAuthoringAsset.Story == null)
            {
                return;
            }

            EnsureVariableFieldsAreUpToDate();

            List<VariableInfo> subgraphStoryParameters = SubgraphAuthoringAsset.Story.Variables;

            // Check if number of subgraph story param types is correct
            if (subgraphStoryParameters.Count != m_StoryFields.Count)
            {
                RecreateStoryFields(subgraphStoryParameters);
                return;
            }

            // Check if subgraph story param types align with field types
            for (int i = 0; i < subgraphStoryParameters.Count; ++i)
            {
                VariableInfo info = subgraphStoryParameters[i];
                Type fieldValueType = m_StoryFields[i]?.Type;
                if (!fieldValueType.IsAssignableFrom(info.Type))
                {
                    RecreateStoryFields(subgraphStoryParameters);
                    return;
                }
            }
        }

        private void EnsureVariableFieldsAreUpToDate()
        {
            if (IsDynamic)
            {
                if (RequiredBlackboard != null)
                {
                    foreach (VariableModel variable in RequiredBlackboard.Variables)
                    {
                        RemoveFieldIfShared(variable);
                    }
                }
            }
            else if (SubgraphAuthoringAsset != null)
            {
                foreach (VariableModel variable in SubgraphAuthoringAsset.Blackboard.Variables)
                {
                    RemoveFieldIfShared(variable);
                }

                foreach (var blackboard in SubgraphAuthoringAsset.m_Blackboards)
                {
                    foreach (var variable in blackboard.Variables)
                    {
                        RemoveFieldIfShared(variable);
                    }
                }
            }
        }

        private void RemoveFieldIfShared(VariableModel variable)
        {
            if (!variable.IsShared)
            {
                return;
            }

            FieldModel field = GetOrCreateField(variable.Name, variable.Type);
            if (field != null)
            {
                m_FieldValues.Remove(field);
            }
        }

        private void ClearFields()
        {
            m_FieldValues.Clear();
            m_StoryFields.Clear();
            GetOrCreateField(k_SubgraphFieldName, typeof(BehaviorGraph));
            GetOrCreateField(k_BlackboardFieldName, typeof(BehaviorBlackboardAuthoringAsset));
        }

        private void RecreateStoryFields(List<VariableInfo> storyParameters)
        {
            var oldStoryFields = m_StoryFields.ToList();
            m_StoryFields.Clear();
            for (int m = 0; m < storyParameters.Count; m++)
            {
                VariableInfo info = storyParameters[m];
                var field = GetOrCreateField(info.Name, info.Type);
                m_StoryFields.Add(field);
                oldStoryFields.Remove(field);
            }

            foreach (var oldStoryField in oldStoryFields)
            {
                m_FieldValues.Remove(oldStoryField);
            }
        }

        public override void OnValidate()
        {
            base.OnValidate();

            ValidateCachedRuntimeGraph();
            
            if (SubgraphAuthoringAsset.ContainsReferenceTo(Asset as BehaviorAuthoringGraph))
            {
                Debug.LogWarning($"Subgraph {RuntimeSubgraph.name} contains a cyclic reference to {Asset.name}. The subgraph {RuntimeSubgraph.name} will be removed.");
                SubgraphField.LinkedVariable.ObjectValue = null;
                ClearFields();
            }
            
            UpdateIsDynamic();
        }

        public void ValidateCachedRuntimeGraph()
        {
            if (SubgraphAuthoringAsset == null && m_SubgraphAssetId != new SerializableGUID())
            {
#if UNITY_EDITOR
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(m_SubgraphAssetId.ToString());

                if (!string.IsNullOrEmpty(path))
                {
                    BehaviorAuthoringGraph result = UnityEditor.AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(path);
                    BehaviorGraph runtimeGraph = BehaviorAuthoringGraph.GetOrCreateGraph(result);

                    if (SubgraphField.LinkedVariable != null)
                    {
                        SubgraphField.LinkedVariable.ObjectValue = runtimeGraph;
                    }
                }
#endif
            }
        }
        public void CacheRuntimeGraphId()
        {
            BehaviorAuthoringGraph cachedGraph = SubgraphAuthoringAsset;
            
            if (cachedGraph == null)
            {
                m_SubgraphAssetId = new SerializableGUID();
            }
            else
            {
#if UNITY_EDITOR
                m_SubgraphAssetId = new SerializableGUID(UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(cachedGraph)));
#endif
            }
        }

        private void UpdateIsDynamic()
        {
            if (SubgraphField.LinkedVariable == null)
            {
                // If nothing is linked to the field, the node is static by default.
                IsDynamic = false;
            }
            
            bool assetIsBlackboardVariable = false;
            if (Asset != null)
            {
                foreach (VariableModel variable in Asset.Blackboard.Variables)
                {
                    if (variable == SubgraphField.LinkedVariable)
                    {
                        assetIsBlackboardVariable = true;
                    }
                }
            }

            IsDynamic = assetIsBlackboardVariable;
        }
    }
}