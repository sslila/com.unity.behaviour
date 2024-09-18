#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class BehaviorGraphView : GraphView
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<BehaviorGraphView, UxmlTraits> {}
#endif
        BehaviorAuthoringGraph m_Asset => Asset as BehaviorAuthoringGraph;
        BehaviorGraphModule m_ActiveGraph;
        internal BehaviorGraphModule ActiveGraph
        {
            get => m_ActiveGraph;
            set
            {
                // if another graph was already assigned, unsubscribe from status updates
                if (m_ActiveGraph != null)
                {
                    m_ActiveGraph.OnGraphStatusChange -= OnGraphStatusUpdate;
                }

                m_ActiveGraph = value;
                if (m_ActiveGraph != null)
                {
                    m_ActiveGraph.OnGraphStatusChange += OnGraphStatusUpdate;
                    OnGraphStatusUpdate(m_ActiveGraph);
                }
            }
        }

        // todo move to a dedicated tutorial content handler
        GraphElement m_CreateNodeTutorialText;
        
        private readonly HashSet<Type> m_ExcludedTypes = new()
        {
            //typeof(PlaceholderAction)
        };

        public BehaviorGraphView() 
        {
            AddToClassList("BehaviorGraphView");
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<FocusInEvent>(_ => RefreshFromAsset());
        }

        protected override void OnInitAsset(GraphAsset value)
        {
            if (IsPerformingUndo)
            {
                return;
            }
            
            schedule.Execute(DelayedInitUI);
        }

        protected override void CreateManipulators()
        {
            this.AddManipulator(new SelectionManipulator());
            this.AddManipulator(new MultiSelectionManipulator());
            this.AddManipulator(new MoveManipulator());
            this.AddManipulator(new AddNodeManipulator());
            this.AddManipulator(new DeleteManipulator());
            this.AddManipulator(new DuplicateNodeManipulator());
            this.AddManipulator(new CopyNodeManipulator());
            
            this.AddManipulator(new ContextMenuManipulator());
            this.AddManipulator(new PasteNodeManipulator());
        }

        internal void ResetNodesUI()
        {
            foreach (NodeUI node in ViewState.Nodes)
            {
                if (node is BehaviorNodeUI nodeUI)
                {
                    nodeUI.UpdateStatus(Node.Status.Uninitialized);
                }
                else if (node is SequenceGroup sequenceUI)
                {
                    SetSequenceStatus(sequenceUI, Node.Status.Uninitialized);
                }
            }
        }

        private void OnGraphStatusUpdate(BehaviorGraphModule graph)
        {
            foreach (Node node in graph.Nodes())
            {
                Node.Status status = node.CurrentStatus;
                SerializableGUID id = node.ID;

                NodeUI nodeUI = ViewState.Nodes.FirstOrDefault(nodeUI => nodeUI.Model.ID == id);
                switch (nodeUI)
                {
                    case BehaviorNodeUI aidNodeUI:
                        aidNodeUI.UpdateStatus(status);
                        break;
                    case SequenceGroup sequenceUI:
                        SetSequenceStatus(sequenceUI, status);
                        break;
                }
            }
        }

        private void SetSequenceStatus(SequenceGroup sequenceUI, Node.Status status)
        {
            Port sequenceInputPort = sequenceUI.GetFirstInputPort();
            if (sequenceInputPort != null)
            {
                bool isRunning = !(status is Node.Status.Uninitialized);
                foreach (Edge edge in sequenceInputPort.Edges)
                {
                    edge.IsDebugHighlighted = isRunning;
                    edge.MarkDirtyAndRepaint();
                    edge.EnableInClassList("NodeStatus_Running", isRunning);
                    edge.EnableInClassList("NodeStatus_Uninitialized", !isRunning);
                }
            }
        }

        // To be run only once the visual elements have resolved their positions and size.
        private void DelayedInitUI()
        {
            if (m_Asset == null || !m_Asset.Roots.Any())
            {
                return;
            }

            if (TryGetFirstRoot(out NodeUI rootUI))
            {
                // Check if the root node has a resolved position and dimension. If not, reschedule.
                if (float.IsNaN(rootUI.layout.center.x))
                {
                    schedule.Execute(DelayedInitUI); 
                    return;
                }
                
                // Frame all content on the GridBackground canvas.
                Background.FrameAll();
                Background.zoom = Mathf.Clamp(Background.zoom, Background.minZoom, Background.maxZoom);
                
                // If only the root exists, anchor the tutorial text relative to it.
                // todo move this to a dedicated tutorial content handler
                if (ViewState.Nodes.Count() == 1)
                {
                    m_CreateNodeTutorialText ??= InitTutorialText(rootUI.layout.center);
                    contentContainer.Add(m_CreateNodeTutorialText);
                    this.ViewState.ViewStateUpdated += OnViewStateUpdated;
                }
            }
        }

        private bool TryGetFirstRoot(out NodeUI nodeUI)
        {
            nodeUI = ViewState.Nodes.FirstOrDefault(elem => elem is StartNodeUI);
            if (nodeUI == null)
            {
                nodeUI = ViewState.Nodes.FirstOrDefault(elem => elem is StartOnEventUI);
            }
            return nodeUI != null;
        }

        private GraphElement InitTutorialText(Vector2 anchorElementCenter)
        {
            var element = new GraphElement();
            element.style.fontSize = 24;
            element.Add(new Label("Press Space or Right Click to add your first action."));
            var elementPosition = anchorElementCenter + new Vector2(-275.0f, 75.0f); // 275 ~= half of the text width
            element.Translate = new Translate(elementPosition.x, elementPosition.y);
            return element;
        }

        private void OnViewStateUpdated()
        {
            // If the tutorial text is present and an additional node has been added, remove the text. 
            if (m_CreateNodeTutorialText != null && Asset.Nodes.Count > 1)
            {
                m_CreateNodeTutorialText.RemoveFromHierarchy();
                m_CreateNodeTutorialText = null;
                this.ViewState.ViewStateUpdated -= OnViewStateUpdated;
            }
        }

        internal void RefreshNode(SerializableGUID guid)
        {
            if (ViewState.m_NodeModelToNodeUI.TryGetValue(guid, out NodeUI nodeUI))
            {
                if (nodeUI is BehaviorNodeUI behaviorNodeUI)
                {
                    behaviorNodeUI.UpdateLinkFields();
                }
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.R && TryGetFirstRoot(out NodeUI rootUI))
            {
                Background.FrameElement(rootUI);
                //this.PanToNode(rootUI, localBound.size * Vector2.up * 0.5f,200);
                return;
            }

            if (evt.keyCode is KeyCode.A)
            {
                // If Ctrl/Command+A, select all nodes.
                if (evt.modifiers.HasFlag(EventModifiers.Command) || evt.modifiers.HasFlag(EventModifiers.Control))
                {
                    ViewState.SetSelected(ViewState.Nodes.Where(node => !node.IsInSequence));
                    return;
                }
                // Otherwise, align nodes.
                if (evt.modifiers.HasFlag(EventModifiers.Shift))
                {
                    GraphUILayoutUtility.AlignSelectedNodesAndAllChildren(this);
                    return;
                }
                GraphUILayoutUtility.AlignSelectedNodesImmediateChildren(this);
                return;
            }
        }

        protected override void CreateAddNodeOptions(SearchMenuBuilderGeneric<NodeCreateParams> builder, NodeCreateParams parameters)
        {
            base.CreateAddNodeOptions(builder, parameters);

            builder.SortSearchItems = true;
            builder.Add("Action/Conditional", nodeCreateParams => { }, ResourceLoadAPI.Load<Texture2D>($"Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/Images/Icons/conditional-dot.png"));
            builder.Add("Flow", nodeCreateParams => { }, ResourceLoadAPI.Load<Texture2D>($"Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/Images/Icons/flow-dot.png"));
            builder.Add("Flow/Conditional", nodeCreateParams => { }, ResourceLoadAPI.Load<Texture2D>($"Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/Images/Icons/conditional-dot.png"));
            builder.Add("Events", nodeCreateParams => { }, ResourceLoadAPI.Load<Texture2D>($"Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/Images/Icons/event-dot.png"));

#if UNITY_EDITOR
            builder.Add("Action/Create new Action...", _ => { ShowActionNodeWizard(parameters.Position); }, null, null, true, 1);
            builder.Add("Flow/Create new Modifier...", _ => { ShowModifierNodeWizard(parameters.Position); }, null, null, true, 1);
            builder.Add("Flow/Create new Sequencing...", _ => { ShowSequencingNodeWizard(parameters.Position); }, null, null, true, 1);
#endif

            List<NodeInfo> nodeInfos = NodeRegistry.NodeInfos.Where(n => n.Type != null).ToList();
            foreach (var nodeInfo in nodeInfos)
            {
                if (!string.IsNullOrEmpty(nodeInfo.Category))
                {
                    continue;
                }

                if (nodeInfo.Type.BaseType == typeof(Modifier) || nodeInfo.Type.BaseType == typeof(Composite))
                {
                    nodeInfo.Category = "Flow";
                }
                else
                {
                    nodeInfo.Category = nodeInfo.Type.BaseType?.Name;
                }
            }

            var sortedInfo = nodeInfos.Select(n => ($"{n.Category}/{n.Name}", n)).OrderBy(p => p.Item1, Comparer<string>.Create((s1, s2) => s1.CompareTo(s2))).ToList();
            foreach ((string path, NodeInfo nodeInfo) in sortedInfo)
            {
                // PlaceholderAction should not be able to be created by the user. So we exclude it from the search.
                if (nodeInfo.HideInSearch || m_ExcludedTypes.Contains(nodeInfo.Type))
                {
                    continue;
                }
                builder.Add(path, nodeCreateParams => { OnAddItem(nodeCreateParams.Position, nodeInfo, nodeCreateParams.ConnectedPort, nodeCreateParams.InsertToSequence); }, null, nodeInfo.Description);
            }
        }

        private void OnAddItem(Vector2 position, NodeInfo nodeInfo, PortModel connectedPort = null, SequenceNodeModel sequenceToAddTo = null)
        {
            if (nodeInfo.ModelType == null)
            {
                Debug.LogError($"Node model cannot be null. Ensure a model type is assigned in the NodeDescriptionAttribute.");
                return;
            }

            Dispatcher.DispatchImmediate(new CreateNodeCommand(nodeInfo.ModelType, position, connectedPort, sequenceToAddTo,
                args: new object[] { nodeInfo }));
        }
        
        internal void ConnectPorts(PortModel outputPortModel, PortModel inputPortModel)
        {
            // Check for merge.
            bool canMerge = typeof(JoinNodeModel).IsAssignableFrom(inputPortModel.NodeModel.GetType());
            List<PortModel> inputConnections = inputPortModel.Connections.ToList();
            if (inputConnections.Count >= 1 && !canMerge)
            {
                ShowNodeInsertionPopup(
                    "Merge Options",
                    "To connect to multiple nodes you must pick a valid sequencing node that's allowed to have multiple children. How would you like the branching behaviour to work?",
                    NodeRegistry.NodeInfos.Where(n => n.Type != null && typeof(Join).IsAssignableFrom(n.Type)).ToList(),
                    new Tuple<PortModel, PortModel>(inputPortModel, inputConnections.First()),
                    inputConnections.Append(outputPortModel),
                    new []{ inputPortModel });
                return;
            }

            // Check for branch.
            bool canBranch = typeof(SequenceNodeModel).IsAssignableFrom(outputPortModel.NodeModel.GetType()) ||
                             typeof(CompositeNodeModel).IsAssignableFrom(outputPortModel.NodeModel.GetType());
            List<PortModel> outputConnections = outputPortModel.Connections.ToList();
            if (outputConnections.Count >= 1 && !canBranch)
            {
                ShowNodeInsertionPopup(
                    "Branch Options",
                    "To connect to multiple nodes you must pick a valid sequencing node that's allowed to have multiple children. How would you like the branching behaviour to work?",
                    NodeRegistry.NodeInfos.Where(n => n.Type != null && typeof(Composite).IsAssignableFrom(n.Type)).ToList(),
                    new Tuple<PortModel, PortModel>(outputPortModel, outputConnections.First()),
                    new []{ outputPortModel },
                    outputConnections.Append(inputPortModel));
                return;
            }

            // Otherwise, connect the ports.
            Asset.ConnectEdge(outputPortModel, inputPortModel);
        }
        
        internal void ShowNodeInsertionPopup(string title, string description, List<NodeInfo> options, Tuple<PortModel, PortModel> connectionToBreak, IEnumerable<PortModel> inputConnections, IEnumerable<PortModel> outputConnections)
        {
            InsertNodeDialog dialog = InsertNodeDialog.GetAndShowDialog(this);
            dialog.Title = title;
            dialog.Description = description;
            dialog.NodeOptions = options;
            dialog.Dispatcher = Dispatcher;
            dialog.ConnectionToBreak = connectionToBreak;
            dialog.ConnectedInputPorts.AddRange(outputConnections);
            dialog.ConnectedOutputPorts.AddRange(inputConnections);
        }

        internal void ShowActionNodeWizard(Vector2 mousePosition, PlaceholderNodeModel placeholderNodeModel = null)
        {
#if UNITY_EDITOR
            Dictionary<string, Type> variableSuggestions = Util.GetVariableSuggestions(Asset, placeholderNodeModel);
            ActionNodeWizard nodeWizard = ActionNodeWizardWindow.GetAndShowWindow(this, variableSuggestions);
            if (placeholderNodeModel != null)
            {
                nodeWizard.SetupSuggestedNodeProperties(placeholderNodeModel.Name, "Action");
                nodeWizard.SetStoryField(placeholderNodeModel.Story);
                nodeWizard.OnNodeTypeCreated = nodeData => OnNodeTypeCreatedFromPlaceholderNode(placeholderNodeModel.Name, nodeData, mousePosition);
            }
            else
            {
                nodeWizard.OnNodeTypeCreated = nodeData => OnNodeTypeCreatedFromWizard(nodeData, mousePosition);
            }            
#endif
        }

        internal void ShowModifierNodeWizard(Vector2 mousePosition, PlaceholderNodeModel placeholderNodeModel = null)
        {
#if UNITY_EDITOR
            Dictionary<string, Type> variableSuggestions = Util.GetVariableSuggestions(Asset, placeholderNodeModel);
            BaseNodeWizard nodeWizard = ModifierNodeWizardWindow.GetAndShowWindow(this, variableSuggestions);
            if (placeholderNodeModel != null)
            {
                nodeWizard.SetupSuggestedNodeProperties(placeholderNodeModel.Name, "Flow");
                nodeWizard.SetStoryField(placeholderNodeModel.Story);
                nodeWizard.OnNodeTypeCreated = nodeData => OnNodeTypeCreatedFromPlaceholderNode(placeholderNodeModel.Name, nodeData, mousePosition);
            }
            else
            {
                nodeWizard.OnNodeTypeCreated = nodeData => OnNodeTypeCreatedFromWizard(nodeData, mousePosition);
            }
#endif
        }

        internal void ShowSequencingNodeWizard(Vector2 mousePosition, PlaceholderNodeModel placeholderNodeModel = null)
        {
#if UNITY_EDITOR
            Dictionary<string, Type> variableSuggestions = Util.GetVariableSuggestions(Asset, placeholderNodeModel);
            SequencingNodeWizard nodeWizard = SequencingNodeWizardWindow.GetAndShowWindow(this, variableSuggestions);
            if (placeholderNodeModel != null)
            {
                nodeWizard.SetupSuggestedNodeProperties(placeholderNodeModel.Name, "Flow");
                nodeWizard.SetStoryField(placeholderNodeModel.Story);
                nodeWizard.SetupCustomPorts(placeholderNodeModel.NamedChildren);
                nodeWizard.OnNodeTypeCreated = nodeData => OnNodeTypeCreatedFromPlaceholderNode(placeholderNodeModel.Name, nodeData, mousePosition);
            }
            else
            {
                nodeWizard.OnNodeTypeCreated = nodeData => OnNodeTypeCreatedFromWizard(nodeData, mousePosition);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnNodeTypeCreatedFromWizard(NodeGeneratorUtility.NodeData createdNodeData, Vector2 mousePosition)
        {
            m_Asset.CommandBuffer.SerializeDeferredCommand(new CreateNodeFromSerializedTypeCommand(createdNodeData.ClassName, mousePosition, true));
        }
#endif
        
#if UNITY_EDITOR
        private void OnNodeTypeCreatedFromPlaceholderNode(string placeholderNodeName,
            NodeGeneratorUtility.NodeData createdNodeData, Vector2 mousePosition)
        {
            m_Asset.CommandBuffer.SerializeDeferredCommand(new SwapNodeFromSerializedTypeCommand(placeholderNodeName,  
                createdNodeData.ClassName, mousePosition, true));
        }
#endif
    }
}
#endif