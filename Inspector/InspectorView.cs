#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement("Inspector")]
#endif
    internal partial class InspectorView : VisualElement
    {
        private GraphEditor m_GraphEditor;
        public GraphEditor GraphEditor
        {
            get => m_GraphEditor;
            set
            {
                if (m_GraphEditor != null)
                {
                    m_GraphEditor.GraphView.ViewState.SelectionUpdated -= OnSelectedUpdated;
                }
                m_GraphEditor = value;
                if (m_GraphEditor != null)
                {
                    m_GraphEditor.GraphView.ViewState.SelectionUpdated += OnSelectedUpdated;
                }
            }
        }

        private readonly VisualElement m_ContentContainer;
        public override VisualElement contentContainer => m_ContentContainer ?? base.contentContainer;

        protected NodeModel InspectedNode => m_InspectedNodeUI?.Model;
        private NodeUI m_InspectedNodeUI;
        private NodeInspectorUI m_NodeInspectorUI;
        protected NodeUI InspectedNodeUI
        {
            get => m_InspectedNodeUI;
            private set
            {
                if (m_InspectedNodeUI == value)
                    return;

                m_InspectedNodeUI = value;
                CreateNodeInspectorUI(InspectedNode);
            }
        }

        public InspectorView()
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Inspector/Assets/InspectorStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Inspector/Assets/InspectorLayout.uxml").CloneTree(this);

            AddToClassList("Inspector");
            m_ContentContainer = this.Q("Content");

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        }

        public virtual void CreateDefaultInspector() { }

        private void OnAttachToPanelEvent(AttachToPanelEvent evt)
        {
            if (GetFirstAncestorOfType<FloatingPanel>() == null)
            {
                return;
            }

            FloatingPanel floatingPanel = GetFirstAncestorOfType<FloatingPanel>();
            floatingPanel.Title = "Inspector";
        }


        public virtual void Refresh()
        {
            if (m_NodeInspectorUI != null && InspectedNode != null)
            {
                m_NodeInspectorUI.InspectedNode = InspectedNode;
            }
            m_NodeInspectorUI?.Refresh();
        }

        private void OnSelectedUpdated(IEnumerable<GraphElement> selection)
        {
            int selectedCount = selection.Count();
            if (selectedCount == 1)
            {
                if (selection.First() is NodeUI nodeUI)
                {
                    InspectedNodeUI = nodeUI;
                    return;
                }
            }
            InspectedNodeUI = null;
        }

        private void CreateNodeInspectorUI(NodeModel nodeModel)
        {
            m_NodeInspectorUI?.RemoveFromHierarchy();
            m_NodeInspectorUI = null;

            if (nodeModel == null)
            {
                Clear();
                CreateDefaultInspector();
                return;
            }
            Type nodeInspectorUIType = NodeRegistry.GetInspectorUIType(nodeModel.GetType());
            if (nodeInspectorUIType == null)
            {
                return;
            }

            // Create UI
            Clear();
            m_NodeInspectorUI = Activator.CreateInstance(nodeInspectorUIType, nodeModel) as NodeInspectorUI;
            Add(m_NodeInspectorUI);

        }
    }
}
#endif