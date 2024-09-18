#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class NodeUI : GraphElement
    {
        public override VisualElement contentContainer { get; }
        private NodeModel m_NodeModel;
        public NodeModel Model
        {
            get => m_NodeModel;
            set
            {
                m_NodeModel = value;
                var ports = GetAllPortUIs();
                foreach (var port in ports)
                {
                    port.PortModel = Model.FindPortModelByName(port.name);
                }
            }
        }

        protected VisualElement NodeTitle { get; }
        protected Label NodeTitleLabel { get; }
        protected VisualElement NodeValueContainer { get; }

        internal VisualElement InputPortsContainer { get; }
        internal VisualElement OutputPortsContainer { get; }
        
        internal VisualElement SelectionBorder { get; }
        internal VisualElement DebugIconElement { get; }

        public string Title
        {
            get => NodeTitleLabel?.text;
            set
            {
                if (NodeTitleLabel != null)
                {
                    NodeTitleLabel.text = value;
                }
            }
        }
        
        public NodeUI(NodeModel nodeModel)
        {
            m_NodeModel = nodeModel;

            AddToClassList("GraphNode");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/GraphNodeStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/Graph/Assets/GraphNodeLayout.uxml").CloneTree(this);

            InputPortsContainer = this.Q("InputPortsContainer");
            OutputPortsContainer = this.Q("OutputPortsContainer");
            SelectionBorder = this.Q("SelectionBorder");
            
            contentContainer = this.Q("Content");

            NodeTitle = this.Q("NodeTitle");
            NodeTitleLabel = this.Q<Label>("NodeTitleLabel");
            NodeValueContainer = this.Q("NodeValueContainer");
            DebugIconElement = this.Q("DebugIcon");
            DebugIconElement.SetPreferredTooltipPlacement(PopoverPlacement.Top);

            InitPortUIs();
        }

        public virtual void Refresh(bool isDragging)
        {
            // Only update the position if not in a sequence, which determines the node position.
            if (!IsInSequence)
            {
                var translate = new Translate(Model.Position.x, Model.Position.y);
                Translate = translate;
            }
            
            // Send event to notify any edges of updated position. The edges don't need data, just the event.
            GeometryChangedEvent geometryEvent = GeometryChangedEvent.GetPooled(default, default);
            geometryEvent.target = this;
            SendEvent(geometryEvent);
            geometryEvent.Dispose();
        }

        private void InitPortUIs()
        {
            if (Model == null) { return; }
            foreach (PortModel portModel in Model.AllPortModels)
            {
                if (portModel.IsInputPort)
                {
                    InputPortsContainer.Add(CreatePortUI(portModel));
                } 
                else
                {
                    OutputPortsContainer.Add(CreatePortUI(portModel));
                }
            }
        }

        protected virtual VisualElement CreatePortUI(PortModel portModel) => new Port(portModel) { Style = PortStyle.Edge };

        public Port GetFirstInputPort() => InputPortsContainer.Q<Port>();
        public Port GetFirstOutputPort() => OutputPortsContainer.Q<Port>();
        public List<Port> GetInputPortUIs() => InputPortsContainer.Query<Port>().ToList();
        public List<Port> GetOutputPortUIs() => OutputPortsContainer.Query<Port>().ToList();
        public List<Port> GetAllPortUIs() => GetInputPortUIs().Concat(GetOutputPortUIs()).ToList();
         
        public IEnumerable<NodeUI> GetChildNodeUIs() => 
            GetOutputPortUIs().SelectMany(port => port.Edges.Select(edge => edge.End.GetFirstAncestorOfType<NodeUI>()));
        public IEnumerable<NodeUI> GetParentNodeUIs() => 
            GetInputPortUIs().SelectMany(port => port.Edges.Select(edge => edge.Start.GetFirstAncestorOfType<NodeUI>()));

        public virtual bool IsGroup => false;
        public override bool IsMoveable => true;
        public bool IsSequenceable => Model.IsSequenceable;
        public bool IsInSequence => GetFirstAncestorOfType<SequenceGroup>() != null;
    }
}
#endif