#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class MultiSelectionManipulator : PointerManipulator
    {
        private GraphView m_GraphView => target as GraphView;
        private bool m_IsActive;
        private Vector2 m_PointerDownPosition;
        private Vector2 m_WorldPointerDownPosition;
        private readonly VisualElement m_SelectionBoxElement;

        public MultiSelectionManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_SelectionBoxElement = new VisualElement();
            m_SelectionBoxElement.style.borderRightWidth = m_SelectionBoxElement.style.borderBottomWidth = m_SelectionBoxElement.style.borderLeftWidth = m_SelectionBoxElement.style.borderTopWidth = 1.0f;
            //m_SelectionBoxElement.style.borderRightColor = m_SelectionBoxElement.style.borderLeftColor = m_SelectionBoxElement.style.borderBottomColor = m_SelectionBoxElement.style.borderTopColor = Color.blue;
            m_SelectionBoxElement.style.opacity = 0.9f;
            m_SelectionBoxElement.pickingMode = PickingMode.Ignore;
            m_SelectionBoxElement.style.position = Position.Absolute;
            m_SelectionBoxElement.name = "Selection Box";
            m_SelectionBoxElement.AddToClassList("GraphView_SelectionBox");
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
            target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }        

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }
        
        private void OnPointerDownEvent(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
            {
                return;
            }

            if (!m_GraphView.ViewState.Selected.Any())
            {
                m_IsActive = true;
                m_PointerDownPosition = evt.localPosition;
                m_WorldPointerDownPosition = evt.position ;
                m_SelectionBoxElement.style.left = m_PointerDownPosition.x;
                m_SelectionBoxElement.style.top = m_PointerDownPosition.y;
            }
        }

        private void OnPointerMoveEvent(PointerMoveEvent evt)
        {
            if (!m_IsActive)
            {
                return;
            }
            if (!m_GraphView.HasPointerCapture(evt.pointerId))
            {
                m_GraphView.CapturePointer(evt.pointerId);
                m_GraphView.hierarchy.Add(m_SelectionBoxElement);
            }
            Vector2 size = (Vector2)evt.localPosition - m_PointerDownPosition;
            Vector2 absoluteSize = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
            m_SelectionBoxElement.style.left = Mathf.Min(evt.localPosition.x, m_PointerDownPosition.x);
            m_SelectionBoxElement.style.top = Mathf.Min(evt.localPosition.y, m_PointerDownPosition.y);
            m_SelectionBoxElement.style.width = absoluteSize.x;
            m_SelectionBoxElement.style.height = absoluteSize.y;


            Vector2 viewportPosStart = m_GraphView.WorldPosToLocal(m_WorldPointerDownPosition);
            Vector2 viewportPos = m_GraphView.WorldPosToLocal(evt.position);
            Vector2 viewportScaledSize = viewportPos - viewportPosStart;
            Vector2 absoluteViewportScaledSize = new Vector2(Mathf.Abs(viewportScaledSize.x), Mathf.Abs(viewportScaledSize.y));
            Rect rect = new Rect(new Vector2(Mathf.Min(viewportPosStart.x, viewportPos.x), Mathf.Min(viewportPosStart.y, viewportPos.y)), absoluteViewportScaledSize);
           
            List<NodeUI> nodes = GetNodesInRect(rect, out List<NodeUI> notInsideRect);
            foreach (NodeUI node in nodes)
            {
                if (node.IsInSequence)
                {
                    continue;
                }
                m_GraphView.ViewState.AddSelected(node);
            }

            for (int i = 0; i < notInsideRect.Count; i++)
            {
                m_GraphView.ViewState.RemoveSelected(notInsideRect[i]);
            }
            
            Vector2 selectionSizeWorld = (Vector2)evt.position - m_WorldPointerDownPosition;
            Rect worldRect = new Rect(Mathf.Min(m_WorldPointerDownPosition.x, evt.position .x), Mathf.Min(m_WorldPointerDownPosition.y, evt.position .y), Mathf.Abs(selectionSizeWorld.x), Mathf.Abs(selectionSizeWorld.y));
            List<Edge> edgesInRect = GetEdgesInRect(worldRect);
            edgesInRect.ForEach(edge => {
                m_GraphView.ViewState.AddSelected(edge);
            });
            foreach (Edge edge in m_GraphView.ViewState.Edges)
            {
                if (!edgesInRect.Contains(edge))
                {
                    m_GraphView.ViewState.RemoveSelected(edge);
                }
            }

        }

        private void OnPointerUpEvent(PointerUpEvent evt)
        {
            if (!m_IsActive)
            {
                return;
            }
            target.ReleasePointer(evt.pointerId);
            m_IsActive = false;
            m_SelectionBoxElement.RemoveFromHierarchy();
        }
        
        private List<Edge> GetEdgesInRect(Rect rect)
        {
            List<Edge> edges = new List<Edge>();
            foreach (Edge edge in m_GraphView.ViewState.Edges)
            {
                if (edge.IsEdgeInRect(rect))
                {
                    edges.Add(edge);
                }
            }
            return edges;
        }

        private List<NodeUI> GetNodesInRect(Rect rect, out List<NodeUI> notInsideRect)
        {
            List<NodeUI> nodes = new List<NodeUI>();
            notInsideRect = new List<NodeUI>();
            foreach (NodeUI node in m_GraphView.ViewState.Nodes)
            {
                if (node.localBound.Overlaps(rect))
                {
                    nodes.Add(node);
                }
                else
                {
                    notInsideRect.Add(node);
                }
            }
            return nodes;
        }
    }
}
#endif