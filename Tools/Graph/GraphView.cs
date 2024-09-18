#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Canvas = Unity.AppUI.UI.Canvas;

namespace Unity.Behavior.GraphFramework
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class GraphView : VisualElement
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<GraphView, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public UxmlTraits()
            {
                focusable.defaultValue = true;
            }
        }
#endif

        private const string k_VisualTreeAssetPath = "Packages/com.unity.behavior/Tools/Graph/Assets/GraphViewLayout.uxml";
        private const string k_StyleSheetAssetPath = "Packages/com.unity.behavior/Tools/Graph/Assets/GraphViewStyles.uss";

        public GraphAsset Asset { get; private set; }
        public GraphViewState ViewState { get; private set; }
        public Dispatcher Dispatcher { get; internal set; }
        public bool IsPerformingUndo { get; internal set; } // todo do we still need this?

        public readonly VisualElement Viewport;
        public Canvas Background { get; set; }
        public override VisualElement contentContainer => m_ElementsParent;
        private readonly VisualElement m_ElementsParent; // Used for everything except edges.
        private readonly VisualElement m_EdgesParent; // todo do we need to keep edges separately?

        private const float k_CanvasMinZoom = 0.1f;
        private const float k_CanvasMaxZoom = 1.25f;

        public GraphView()
        {
            AddToClassList("GraphView");
            this.focusable = true;

            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>(k_StyleSheetAssetPath));
            ResourceLoadAPI.Load<VisualTreeAsset>(k_VisualTreeAssetPath).CloneTree(this);

            Viewport = this.Q("Viewport");
            Background = this.Q<Canvas>();
            m_ElementsParent = this.Q("Elements");
            m_EdgesParent = this.Q("Edges");

            ViewState = new GraphViewState(Asset);
            ViewState.ViewStateUpdated += OnViewStateUpdated;
            ViewState.RefreshFromAsset(false);

            // Set the canvas min and max zoom levels.
            Background.StretchToParentSize();
            Background.minZoom = k_CanvasMinZoom;
            Background.maxZoom = k_CanvasMaxZoom;
            Background.dampingEffectDuration = 0;

            schedule.Execute(CreateManipulators);
            schedule.Execute(RefreshFromAsset);
        }

        internal void Load(GraphAsset asset)
        {
            Reset();
            Asset = asset;

            ViewState.Asset = asset;
            ViewState.RefreshFromAsset(false);

            OnInitAsset(asset);
        }

        // This refreshes the state of EdgesParent and ElementsParent.
        private void OnViewStateUpdated()
        {
            // Remove nodes that are no longer in the view state.
            foreach (VisualElement child in m_EdgesParent.Children())
            {
                if (child is NodeUI childNodeUI)
                {
                    bool matchFound = false;
                    foreach (NodeUI nodeUI in ViewState.Nodes)
                    {
                        if (nodeUI == childNodeUI)
                        {
                            matchFound = true;
                            break;
                        }
                    }
                    if (!matchFound)
                    {
                        m_ElementsParent.Remove(childNodeUI);
                    }
                }
            }

            // Add new nodes.
            // Note: Nodes in sequences are already parented and shouldn't be re-added to ElementsParent.
            foreach (NodeUI nodeUI in ViewState.Nodes)
            {
                if (nodeUI.parent is not SequenceGroup && !m_ElementsParent.Contains(nodeUI))
                {
                    m_ElementsParent.Add(nodeUI);
                }
            }

            // Remove edges that are no longer in the view state.
            foreach (VisualElement visualElement in m_EdgesParent.Children())
            {
                var edge = (Edge)visualElement;
                bool matchFound = false;
                foreach (Edge viewStateEdge in ViewState.Edges)
                {
                    if (viewStateEdge == edge)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (!matchFound)
                {
                    m_EdgesParent.Remove(edge);
                }
            }

            // Add new edges.
            foreach (Edge edge in ViewState.Edges)
            {
                if (!m_EdgesParent.Contains(edge))
                {
                    m_EdgesParent.Add(edge);
                }
            }
        }

        private void Reset()
        {
            Asset = null;
            m_EdgesParent.Clear();
            m_ElementsParent.Clear();
        }

        protected virtual void OnInitAsset(GraphAsset asset) { }

        protected internal virtual void RefreshFromAsset()
        {
            ViewState.RefreshFromAsset(false);
        }

        protected virtual void CreateManipulators()
        {
            this.AddManipulator(new SelectionManipulator());
            this.AddManipulator(new MultiSelectionManipulator());
            this.AddManipulator(new MoveManipulator());
            this.AddManipulator(new AddNodeManipulator());
            this.AddManipulator(new DeleteManipulator());
            this.AddManipulator(new DuplicateNodeManipulator());
            this.AddManipulator(new CopyNodeManipulator());
            this.AddManipulator(new PasteNodeManipulator());
        }

        protected class NodeCreateParams
        {
            public Vector2 Position;
            public PortModel ConnectedPort;
            public SequenceNodeModel InsertToSequence;
        }

        protected virtual void CreateAddNodeOptions(SearchMenuBuilderGeneric<NodeCreateParams> builder, NodeCreateParams parameters)
        {
            builder.Add("Sticky Note", (nodeCreateParams) =>
            {
                Dispatcher.DispatchImmediate(new CreateNodeCommand(typeof(StickyNoteModel), nodeCreateParams.Position, nodeCreateParams.ConnectedPort, nodeCreateParams.InsertToSequence));
            });
        }

        public void ShowNodeSearch(Vector2 worldPosition, PortModel connectedPort = null, SequenceNodeModel insertToSequence = null)
        {
            NodeCreateParams nodeParams = new NodeCreateParams { Position = this.WorldPosToLocal(worldPosition), ConnectedPort = connectedPort, InsertToSequence = insertToSequence };
            SearchMenuBuilderGeneric<NodeCreateParams> builder = new SearchMenuBuilderGeneric<NodeCreateParams>();
            CreateAddNodeOptions(builder, nodeParams);

            builder.Title = "Add Node";
            builder.OnSelection = (selection) =>
            {
                if (selection.Data is SearchMenuBuilderGeneric<NodeCreateParams>.OnOptionSelectedWithParams callback)
                {
                    callback(nodeParams);
                }
            };
            builder.Width = 200.0f;
            builder.Height = 268.0f;
            builder.Parent = Background;
            builder.ShowIcons = true;
            builder.ShowAtPosition(worldPosition.x, worldPosition.y);
        }
    }

    // A class to hold data for the state of the view.
    internal class GraphViewState
    {
        internal GraphAsset Asset;
        public Action ViewStateUpdated;
        public Action<IEnumerable<GraphElement>> SelectionUpdated;

        // UI elements
        public IEnumerable<NodeUI> Nodes => m_Nodes;
        private readonly List<NodeUI> m_Nodes = new ();
        public IEnumerable<Edge> Edges => m_Edges;
        private readonly List<Edge> m_Edges = new ();
        public HashSet<GraphElement> Selected => m_Selected;
        private readonly HashSet<GraphElement> m_Selected = new ();

        // Model->UI lookups
        internal readonly Dictionary<SerializableGUID, NodeUI> m_NodeModelToNodeUI = new ();
        private readonly Dictionary<PortModel, List<Edge>> m_PortToEdges = new ();

        // Cached query containers for updating the view state.
        private readonly Dictionary<(SerializableGUID, SerializableGUID), Tuple<PortModel, PortModel>> m_EdgesInAsset = new();
        private readonly Dictionary<(SerializableGUID, SerializableGUID), Edge> m_EdgeUIs = new();
        private readonly HashSet<SerializableGUID> m_AssetNodeIDs = new();
        private readonly List<NodeUI> m_NodeUIsToDelete = new();
        private readonly HashSet<SerializableGUID> m_UINodeIDs = new();
        private readonly List<Edge> m_EdgesToDelete = new();

        public GraphViewState(GraphAsset asset)
        {
            Asset = asset;
        }

        internal void RefreshFromAsset(bool isDragging)
        {
            RefreshNodeUI(isDragging, m_Nodes);
        }

        /// <summary>
        /// Refresh specific node UI visual elements.
        /// </summary>
        /// <param name="isDragging">Used to know if we are dragging a node to avoid redundant performance heavy code</param>
        /// <param name="nodesToRefresh">The node UI list to refresh visuals</param>
        internal void RefreshNodeUI(bool isDragging, List<NodeUI> nodesToRefresh)
        {
            if (Asset == null)
            {
                return;
            }

            // Associate model data with UI. If asset has re/deserialized, the model data will have new instances.
            foreach (NodeModel node in Asset.Nodes)
            {
                foreach (NodeUI nodeUI in Nodes)
                {
                    if (nodeUI.Model.ID == node.ID)
                    {
                        nodeUI.Model = node;
                        break;
                    }
                }
            }

            // Setup edge lookups by ID pairs.
            m_EdgesInAsset.Clear();
            foreach (NodeModel node in Asset.Nodes)
            {
                foreach (PortModel inputPort in node.InputPortModels)
                {
                    foreach (PortModel outputPort in inputPort.Connections)
                    {
                        var key = (outputPort.NodeModel.ID, inputPort.NodeModel.ID);
                        if (!m_EdgesInAsset.ContainsKey(key))
                        {
                            m_EdgesInAsset.Add(key, new Tuple<PortModel, PortModel>(outputPort, inputPort));
                        }
                    }
                }
            }

            m_EdgeUIs.Clear();
            foreach (Edge edge in m_Edges)
            {
                m_EdgeUIs.Add((edge.Start.PortModel.NodeModel.ID, edge.End.PortModel.NodeModel.ID), edge);
            }

            bool EdgeAndUIMatch(Tuple<PortModel, PortModel> edge, Edge edgeUI)
            {
                var edgeAndUIMatch = edgeUI.Start.PortModel.Name == edge.Item1.Name // same starting port name
                                        && edgeUI.End.PortModel.Name == edge.Item2.Name; // same ending port name
                return edgeAndUIMatch;    
            }

            foreach (Edge edgeUI in m_Edges)
            {
                SerializableGUID edgeStartID = edgeUI.Start.PortModel.NodeModel.ID;
                SerializableGUID edgeEndID = edgeUI.End.PortModel.NodeModel.ID;
                if (m_EdgesInAsset.TryGetValue((edgeStartID, edgeEndID), out Tuple<PortModel, PortModel> edge) && EdgeAndUIMatch(edge, edgeUI))
                {
                    edgeUI.Start.PortModel = edge.Item1;
                    edgeUI.End.PortModel = edge.Item2;
                }
            }

            // Remove node UIs without corresponding node model in asset.
            m_AssetNodeIDs.Clear();
            foreach (NodeModel node in Asset.Nodes)
            {
                m_AssetNodeIDs.Add(node.ID);
            }
            m_NodeUIsToDelete.Clear();
            foreach (NodeUI nodeUI in Nodes)
            {
                if (!m_AssetNodeIDs.Contains(nodeUI.Model.ID))
                {
                    m_NodeUIsToDelete.Add(nodeUI);
                }
            }
            foreach (NodeUI nodeUI in m_NodeUIsToDelete)
            {
                DeleteNodeUI(nodeUI);
            }

            // Update node UIs.
            foreach (NodeUI nodeUI in nodesToRefresh)
            {
                // Un-nest nodes removed from sequences but whose UI is still nested.
                if (nodeUI.GetFirstAncestorOfType<SequenceGroup>() != null)
                {
                    bool nodeInSequence = false;
                    foreach (NodeModel parent in nodeUI.Model.Parents)
                    {
                        if (parent is SequenceNodeModel)
                        {
                            nodeInSequence = true;
                            break;
                        }
                    }

                    if (!nodeInSequence)
                    {
                        nodeUI.RemoveFromHierarchy();
                        nodeUI.Translate = new Translate(nodeUI.Model.Position.x, nodeUI.Model.Position.y);
                        nodeUI.RemoveFromClassList("SequencedNode");
                    }
                }

                // Refresh the node UI to pick up changes in the node model data.
                nodeUI.Refresh(isDragging);
            }

            // Create node UIs for nodes in the asset that do not have a preexisting UI.
            m_UINodeIDs.Clear();
            foreach (NodeUI nodeUI in Nodes)
            {
                m_UINodeIDs.Add(nodeUI.Model.ID);
            }
            foreach (NodeModel node in Asset.Nodes)
            {
                if (!m_UINodeIDs.Contains(node.ID))
                {
                    CreateNodeUI(node);
                }
            }

            // Delete edge UIs without matching edge in asset.
            m_EdgesToDelete.Clear();
            foreach (Edge edgeUI in Edges)
            {
                SerializableGUID edgeStartID = edgeUI.Start.PortModel.NodeModel.ID;
                SerializableGUID edgeEndID = edgeUI.End.PortModel.NodeModel.ID;
                if (m_EdgesInAsset.TryGetValue((edgeStartID, edgeEndID), out Tuple<PortModel, PortModel> edge) &&
                    EdgeAndUIMatch(edge, edgeUI))
                {
                    continue;
                }
                m_EdgesToDelete.Add(edgeUI);
            }
            m_EdgesToDelete.ForEach(DeleteEdgeUI);

            // Create edge UIs for edges in asset that don't already exist.
            foreach (Tuple<PortModel, PortModel> edge in m_EdgesInAsset.Values)
            {
                var hasEdgeUI = m_EdgeUIs.TryGetValue((edge.Item1.NodeModel.ID, edge.Item2.NodeModel.ID), out Edge edgeUI);
                var edgeAndUIMatch = hasEdgeUI && EdgeAndUIMatch(edge, edgeUI);
                if (!edgeAndUIMatch)
                {
                    CreateEdgeUI(edge.Item1, edge.Item2);
                }
            }

            // Update sequence nodes!
            foreach (NodeModel nodeModel in Asset.Nodes)
            {
                if (nodeModel is not SequenceNodeModel sequence)
                {
                    continue;
                }

                if (!m_NodeModelToNodeUI.TryGetValue(sequence.ID, out var value))
                {
                    continue;
                }
                
                SequenceGroup sequenceUI = value as SequenceGroup;
                int index = 0;
                foreach (NodeModel nodeInSequence in sequence.Nodes)
                {
                    var foundNodeUI = m_NodeModelToNodeUI.TryGetValue(nodeInSequence.ID, out NodeUI nodeUI);
                    if (foundNodeUI && nodeUI.parent != sequenceUI)
                    {
                        MoveNodeUIToSequence(nodeUI, sequenceUI, index);
                    }
                    index++;
                }
            }

            ViewStateUpdated?.Invoke();
        }

        private void CreateNodeUI(NodeModel nodeModel)
        {
            Type nodeUIType = NodeRegistry.GetUIType(nodeModel.GetType());
            if (nodeUIType == null)
            {
                Debug.LogError($"Could not find node UI type for {nodeModel.GetType()}.");
                return;
            }

            // Create UI
            NodeUI nodeUI = Activator.CreateInstance(nodeUIType, nodeModel) as NodeUI;
            nodeUI!.Translate = new Translate(nodeModel.Position.x, nodeModel.Position.y);
            nodeUI.usageHints |= UsageHints.DynamicTransform;

            // Update containers
            m_NodeModelToNodeUI.Add(nodeModel.ID, nodeUI);
            if (nodeModel is SequenceNodeModel)
            {
                m_Nodes.Add(nodeUI);
            }
            else
            {
                m_Nodes.Insert(0, nodeUI);
            }
        }

        private void DeleteNodeUI(NodeUI nodeUI)
        {
            // If the node is a group, delete all child nodeUIs.
            if (nodeUI.IsGroup)
            {
                var children = new List<VisualElement>(nodeUI.Children());
                foreach (VisualElement child in children)
                {
                    if (child is NodeUI nodeChild)
                    {
                        nodeChild.RemoveFromHierarchy();
                        nodeChild.style.position = Position.Absolute;
                        nodeChild.RemoveFromClassList("SequencedNode");
                    }
                }
            }

            // Remove the UI from remaining graph data containers.
            m_Nodes.Remove(nodeUI);
            m_NodeModelToNodeUI.Remove(nodeUI.Model.ID);
            nodeUI.GetAllPortUIs().ForEach(DeletePortUI); // Delete ports and connected edges.
            nodeUI.RemoveFromHierarchy();
        }

        private void MoveNodeUIToSequence(NodeUI nodeUI, SequenceGroup sequenceUI, int index)
        {
            // Delete edges from initial node UI creation.
            foreach (PortModel portModel in nodeUI.Model.AllPortModels)
            {
                if (m_PortToEdges.TryGetValue(portModel, out List<Edge> edges))
                {
                    m_EdgesToDelete.Clear();
                    foreach (Edge edge in edges)
                    {
                        m_EdgesToDelete.Add(edge);
                    }
                    m_EdgesToDelete.ForEach(DeleteEdgeUI);
                }
            }

            nodeUI.RemoveFromHierarchy(); // remove the node from the prior parent, as it will be added to sequence
            nodeUI.style.position = Position.Relative;
            nodeUI.transform.position = Vector2.zero;
            nodeUI.style.left = StyleKeyword.Auto;
            nodeUI.style.top = StyleKeyword.Auto;
            nodeUI.AddToClassList("SequencedNode");

            // When is the passed index incorrect?
            if (index == -1 || index > sequenceUI.childCount)
            {
                index = sequenceUI.childCount;
            }
            // Add to sequence at desired index.
            if (index >= sequenceUI.childCount)
            {
                sequenceUI.Add(nodeUI);
            }
            else
            {
                sequenceUI.Insert(index, nodeUI);
            }
        }

        private void DeletePortUI(Port portUI)
        {
            if (m_PortToEdges.TryGetValue(portUI.PortModel, out List<Edge> edges))
            {
                m_EdgesToDelete.Clear();
                foreach (Edge edge in edges)
                {
                    m_EdgesToDelete.Add(edge);
                }
                m_EdgesToDelete.ForEach(DeleteEdgeUI);
            }
            m_PortToEdges.Remove(portUI.PortModel);
            portUI.RemoveFromHierarchy();
        }

        private void CreateEdgeUI(PortModel startPort, PortModel endPort)
        {
            Assert.IsTrue(startPort.IsInputPort != endPort.IsInputPort, "Cannot connect ports of the same type.");
            PortModel outputPort = startPort.IsOutputPort ? startPort : endPort;
            PortModel inputPort = startPort.IsInputPort ? startPort : endPort;

            // Find associated node and port UI instances.
            NodeUI startNodeUI = null;
            foreach (NodeUI nodeUI in m_Nodes)
            {
                if (nodeUI.Model.ID == outputPort.NodeModel.ID)
                {
                    startNodeUI = nodeUI;
                    break;
                }
            }
            if (startNodeUI == null)
            {
                Debug.LogError($"Did not find node UI for {outputPort.NodeModel} containing {outputPort}.");
                return;
            }

            NodeUI endNodeUI = null;
            foreach (NodeUI nodeUI in m_Nodes)
            {
                if (nodeUI.Model.ID == inputPort.NodeModel.ID)
                {
                    endNodeUI = nodeUI;
                    break;
                }
            }
            if (endNodeUI == null)
            {
                Debug.LogError($"Did not find node UI for {inputPort.NodeModel} containing {inputPort}.");
                return;
            }

            Port outputPortUI = null;
            foreach (Port port in startNodeUI.GetOutputPortUIs())
            {
                if (port.name == outputPort.Name)
                {
                    outputPortUI = port;
                    break;
                }
            }
            if (outputPortUI == null)
            {
                Debug.LogError($"Port UI missing for port {outputPort} named {outputPort.Name} from {outputPort.NodeModel}.");
                return;
            }

            Port inputPortUI = null;
            foreach (Port port in endNodeUI.GetInputPortUIs())
            {
                if (port.name == inputPort.Name)
                {
                    inputPortUI = port;
                    break;
                }
            }
            if (inputPortUI == null)
            {
                Debug.LogError($"Port UI missing for port {inputPort} named {inputPort.Name} from {inputPort.NodeModel}.");
                return;
            }

            // Create edge.
            Edge edge = new Edge { Start = outputPortUI, End = inputPortUI };

            // Edges coming from special nodes with multiple ports shouldn't be deleteable because we're creating explicit port nodes for them.
            edge.IsDeletable = outputPort.NodeModel.OutputPortModels.Count() <= 1;
            m_Edges.Add(edge);

            // Add edge to port UI containers.
            outputPortUI.Edges.Add(edge);
            inputPortUI.Edges.Add(edge);

            // Associate with port to edge lookup for both ports.
            if (!m_PortToEdges.TryGetValue(outputPort, out List<Edge> startEdges))
            {
                startEdges = new List<Edge>();
                m_PortToEdges.Add(outputPort, startEdges);
            }
            startEdges.Add(edge);
            if (!m_PortToEdges.TryGetValue(inputPort, out List<Edge> endEdges))
            {
                endEdges = new List<Edge>();
                m_PortToEdges.Add(inputPort, endEdges);
            }
            endEdges.Add(edge);
        }

        private void DeleteEdgeUI(Edge edgeUI)
        {
            if (m_PortToEdges.TryGetValue(edgeUI.Start.PortModel, out List<Edge> startEdges))
            {
                startEdges.Remove(edgeUI);
            }
            if (m_PortToEdges.TryGetValue(edgeUI.End.PortModel, out List<Edge> endEdges))
            {
                endEdges.Remove(edgeUI);
            }
            edgeUI.End?.Edges.Remove(edgeUI);
            edgeUI.Start?.Edges.Remove(edgeUI);
            edgeUI.RemoveFromHierarchy();
            m_Edges.Remove(edgeUI);
        }

        public void AddSelected(GraphElement element)
        {
            if (!m_Selected.Contains(element))
            {
                m_Selected.Add(element);
                element.OnSelect();
                element.AddToClassList("Selected");
            }

            SelectionUpdated?.Invoke(Selected);
        }

        public void RemoveSelected(GraphElement element)
        {
            var wasRemoved = m_Selected.Remove(element);
            if (wasRemoved)
            {
                element.OnDeselect();
                element.RemoveFromClassList("Selected");

                SelectionUpdated?.Invoke(Selected);
            }
        }

        public void SetSelected(IEnumerable<GraphElement> elements)
        {
            DeselectAll();
            m_Selected.UnionWith(elements);
            foreach (GraphElement selected in m_Selected)
            {
                selected.OnSelect();
                selected.AddToClassList("Selected");
            }
            SelectionUpdated?.Invoke(Selected);
        }

        public void DeselectAll()
        {
            foreach (GraphElement selected in m_Selected)
            {
                selected.OnDeselect();
                selected.RemoveFromClassList("Selected");
            }
            m_Selected.Clear();
            SelectionUpdated?.Invoke(Selected);
        }

        public void SetSelected(IEnumerable<NodeModel> nodeModelsToSelect)
        {
            DeselectAll();
            List<GraphElement> elementsToSelect = new List<GraphElement>();
            foreach (NodeModel selected in nodeModelsToSelect)
            {
                if (m_NodeModelToNodeUI.TryGetValue(selected.ID, out var node))
                {
                    elementsToSelect.Add(node);
                }
            }
            SetSelected(elementsToSelect);
        }
    }
}
#endif