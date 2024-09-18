#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    [NodeUIAttribute(typeof(SequenceNodeModel))]
    internal class SequenceGroup : Group
    {
        VisualElement m_Sequence;
        public override VisualElement contentContainer => m_Sequence;

        public SequenceGroup(NodeModel nodeModel) : base(nodeModel)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/SequenceGroupStyles.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/Graph/Assets/SequenceGroupLayout.uxml").CloneTree(base.contentContainer);

            m_Sequence = this.Q("Sequence");

            RegisterCallback<PointerEnterEvent>(OnPointerEnterEvent, TrickleDown.TrickleDown);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent, TrickleDown.TrickleDown);
        }

        private void OnPointerEnterEvent(PointerEnterEvent evt)
        {
            var capturingElement = m_Sequence.panel.GetCapturingElement(evt.pointerId);
            if (capturingElement != null || evt.isPropagationStopped)
            {
                return;
            }
            
            bool mouseEnteredChild = typeof(NodeUI).IsAssignableFrom(evt.target.GetType()) && evt.target != this;
            if (mouseEnteredChild)
            {
                AddToClassList("Sequence_ChildHovered");
            }
        }

        private void OnPointerLeaveEvent(PointerLeaveEvent evt)
        {
            var capturingElement = m_Sequence.panel.GetCapturingElement(evt.pointerId);
            if (capturingElement != null || evt.isPropagationStopped)
            {
                return;
            }
            
            bool mouseLeftChild = typeof(NodeUI).IsAssignableFrom(evt.target.GetType()) && evt.target != this;
            if (mouseLeftChild)
            {
                RemoveFromClassList("Sequence_ChildHovered");
            }
        }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);

            if (isDragging)
            {
                return;
            }
            
            // TODO: Darren
            // This happening all the time when updating anything with UI in general.. is still causing spikes in other places..
            // How to do this only when necessary?
            //
            // This should only be valid on first open,
            // moving nodes around a sequence & dropping a new node or list of nodes into another sequence.
            List<NodeModel> nestedNodes = (Model as SequenceNodeModel).Nodes;
            // Why is this visual element sort so costly?
            Sort((a, b) =>
            {
                if (a is NodeUI nodeA && b is NodeUI nodeB)
                {
                    return nestedNodes.IndexOf(nodeA.Model).CompareTo(nestedNodes.IndexOf(nodeB.Model));
                }
                return 0;
            });
        }
    }
}
#endif