#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class SelectionManipulator : PointerManipulator
    {
        private GraphView Target => target as GraphView;
        private long m_LastClickTime = 0;
        private const long k_DoubleClickDelay = 500; // ms

        private bool m_DoubleClicked;
        private GraphElement justSelectedElement;
        private NodeUI m_LastSelectedNodeUI;
        private Vector2 m_MouseDownPos;

        public SelectionManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control });
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Command });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        private void OnPointerDownEvent(PointerDownEvent evt)
        {
            m_DoubleClicked = false;
            if (!CanStartManipulation(evt))
            {
                return;
            }

            m_MouseDownPos = evt.originalMousePosition;
            var nodeAt = Target.NodeAt(evt.position);
            long millisecondsNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (millisecondsNow <= (m_LastClickTime + k_DoubleClickDelay) && nodeAt == m_LastSelectedNodeUI)
            {
                // Handle second click
                OnDoublePointerDownEvent(evt);
                m_LastClickTime = 0;
            }
            else
            {
                // Handle single click
                OnSinglePointerDownEvent(evt);

                // Set the timer waiting for second click
                m_LastClickTime = millisecondsNow;
            }
            m_LastSelectedNodeUI = nodeAt;
        }

        private void OnPointerUpEvent(PointerUpEvent evt)
        {
            if (evt.originalMousePosition != m_MouseDownPos || m_DoubleClicked)
            {
                return;
            }

            var nodeAt = Target.NodeAt(evt.position);
            var edgeAt = Target.EdgeAt(evt.position);

            if (nodeAt == null && edgeAt == null)
            {
                return;
            }
            
            // we try selection on node first, and try edge only if node selection doesn't succeed
            if (!TryHandleSelection(nodeAt))
            {
                TryHandleSelection(edgeAt);
            }
            
            bool TryHandleSelection(GraphElement element)
            {
                // no element => nothing to do
                if (element == null)
                {
                    return false;
                }

                // if element is already selected and clicked again with modifiers present, we deselect
                if (HasModifier(evt))
                {
                    if (IsSelected(element) && element != justSelectedElement)
                    {
                        Deselect(element);
                        return true;
                    }
                }
                else
                {
                    Target.ViewState.DeselectAll();
                    Select(element);
                    return true;
                }

                return false;
            }
            justSelectedElement = null;
        }

        private void OnSinglePointerDownEvent(PointerDownEvent evt)
        {
            var nodeAt = Target.NodeAt(evt.position);
            var edgeAt = Target.EdgeAt(evt.position);

            if (nodeAt == null && edgeAt == null)
            {
                justSelectedElement = null;
                Target.ViewState.DeselectAll(); 
                return;
            }

            // we try selection on node first, and try edge only if node selection doesn't succeed
            if (!TryHandleSelection(nodeAt))
            {
                TryHandleSelection(edgeAt);
            }

            bool TryHandleSelection(GraphElement element)
            {
                // no element => nothing to do
                if (element == null)
                {
                    return false;
                }

                // element is not yet selected, select based on pressed modifiers
                if (!IsSelected(element))
                {
                    justSelectedElement = element;
                    if (HasModifier(evt))
                    {
                        Select(element);
                    }
                    else
                    {
                        SingleSelect(element);
                    }
                }
                return true;
            }
        }
        
        private static bool HasModifier(IPointerEvent evt) => evt.shiftKey || evt.ctrlKey || evt.commandKey;

        private void OnDoublePointerDownEvent(PointerDownEvent evt)
        {
            m_DoubleClicked = true;
            Target.ViewState.DeselectAll();
            foreach (NodeUI element in Target.ViewState.Nodes)
            {
                if (!element.ContainsPoint(element.WorldToLocal(evt.position)))
                {
                    continue;
                }

                foreach (NodeUI child in GetAllChildrenInSubgraph(element).Where(child => child.parent is not SequenceGroup))
                {
                    Select(child);
                }
                return;
            }
        }

        private bool IsSelected(GraphElement element) => Target.ViewState.Selected.Contains(element);
        
        private void Select(GraphElement elementToSelect)
        {
            Target.ViewState.AddSelected(elementToSelect);
            elementToSelect.AddToClassList("Selected");
            elementToSelect.OnSelect();
        }
        
        private void SingleSelect(GraphElement element)
        {
            Target.ViewState.DeselectAll();
            Select(element);
        }
        
        private void Deselect(GraphElement elementToDeselect)
        {
            Target.ViewState.RemoveSelected(elementToDeselect);
            elementToDeselect.OnDeselect();
            elementToDeselect.RemoveFromClassList("Selected");
        }

        private static IEnumerable<NodeUI> GetAllChildrenInSubgraph(NodeUI node)
        {
            var nodes = new HashSet<NodeUI>();
            var nodeQueue = new Queue<NodeUI>();
            nodeQueue.Enqueue(node);
            nodes.Add(node);

            while (nodeQueue.TryDequeue(out NodeUI nextNode))
            {
                foreach (NodeUI child in nextNode.GetChildNodeUIs())
                {
                    if (!node.Contains(child))
                    {
                        nodes.Add(child);
                        nodeQueue.Enqueue(child);
                    }
                }
            }
            return nodes;
        }
    }
}
#endif