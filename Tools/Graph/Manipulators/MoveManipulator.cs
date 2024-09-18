#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class MoveManipulator : PointerManipulator
    {
        private GraphView m_GraphView => target as GraphView;
        private GraphViewState m_GraphViewState => m_GraphView?.ViewState;
        private bool m_IsActive = false;
        private bool m_IsDragging = false;
        private Vector2 m_PointerPosStart;
        private Vector2 m_PointerDeltaPrev;
        private bool m_IsIndicatorVisible;
        private bool m_IsSelectionSequenceable;
        private readonly VisualElement m_InsertIndicator;

        // Reused containers for commands.
        private readonly List<NodeModel> m_NodesToMove = new();
        private readonly List<NodeUI> m_NodeUiToRefresh = new();
        private readonly List<Vector2> m_Positions = new();    
        private readonly List<SequenceNodeModel> m_ParentSequences = new();
        
        public MoveManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Shift });

            m_InsertIndicator = new VisualElement();
            m_InsertIndicator.style.position = Position.Absolute;
            m_InsertIndicator.style.height = 2.0f;
            m_InsertIndicator.style.backgroundColor = (Color)new Color32(8, 146, 255, 255);
            m_InsertIndicator.style.opacity = 1.0f;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
        }
        
        private void OnPointerDownEvent(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
            {
                return;
            }

            m_PointerPosStart = evt.position;
            m_PointerDeltaPrev = Vector2.zero;
            m_IsSelectionSequenceable = true;
            foreach (GraphElement graphElement in m_GraphViewState.Selected)
            {
                // If any of the selected nodes is not sequenceable, the whole set is determined not sequenceable.
                if (graphElement is not NodeUI { IsSequenceable: true })
                {
                    m_IsSelectionSequenceable = false;
                }
                
                if (graphElement.ContainsPoint(graphElement.WorldToLocal(evt.position)))
                {
                    m_IsActive = true;
                    target.CapturePointer(evt.pointerId);
                    return;
                }
            }
        }

        private void OnPointerMoveEvent(PointerMoveEvent evt)
        {
            if (!m_IsActive)
            {
                return;
            }
            
            if (!target.HasPointerCapture(evt.pointerId))
            {
                target.CapturePointer(evt.pointerId);
            }
            
            bool isFirstFrameOfDrag = !m_IsDragging;
            m_IsDragging = true;

            // Move nodes and remove from sequences if applicable.
            m_NodesToMove.Clear();
            m_Positions.Clear();
            m_ParentSequences.Clear();
            m_NodeUiToRefresh.Clear();
            
            Vector2 deltaPointerPos = (Vector2)evt.position - m_PointerPosStart;
            Vector2 scaledDeltaPointerPos = (deltaPointerPos - m_PointerDeltaPrev) / m_GraphView.Background.zoom;
            foreach (GraphElement graphElement in m_GraphViewState.Selected)
            {
                if (graphElement.IsMoveable)
                {
                    if (graphElement is not NodeUI nodeUI)
                    {
                        continue;
                    }

                    bool shouldUnnest = false;
                    Group parentUI = null;
                    Vector2 nodePosition = nodeUI.Model.Position + scaledDeltaPointerPos;
                    nodeUI.AddToClassList("MoveManipulatorActive");
                    if (nodeUI.IsSequenceable)
                    {
                        // Check if the node is in a sequence.
                        parentUI = nodeUI.GetFirstAncestorOfType<Group>();
                        if (parentUI != null)
                        {
                            // Check if the node is being moved outside of the group.
                            shouldUnnest = !parentUI.ContainsPoint(parentUI.WorldToLocal(evt.position));
                            if (shouldUnnest)
                            {
                                nodePosition = parentUI.GetNodeWorldPosition(nodeUI) + deltaPointerPos / m_GraphView.Background.zoom;
                                nodeUI.style.position = Position.Absolute;
                            }
                        }
                    }

                    // Bring moved nodes to front.
                    if (isFirstFrameOfDrag && parentUI == null)
                    {
                        nodeUI.BringToFront(); 
                    }

                    m_NodeUiToRefresh.Add(nodeUI);
                    m_Positions.Add(nodePosition);
                    m_NodesToMove.Add(nodeUI.Model);
                    
                    if (shouldUnnest)
                    {
                        m_ParentSequences.Add(parentUI.Model as SequenceNodeModel);
                        m_NodeUiToRefresh.AddRange(parentUI.GetChildren<NodeUI>(false));
                    }
                    else
                    {
                        m_ParentSequences.Add(null);
                    }
                }
            }

#if UNITY_EDITOR
            if (isFirstFrameOfDrag)
            {
                UnityEditor.Undo.SetCurrentGroupName("Move Nodes");
                UnityEditor.Undo.RegisterCompleteObjectUndo(m_GraphView.Asset, nameof(MoveNodesCommand));
            }
#endif
            
            // We don't mark for undo in this case because we need special RegisterCompleteObjectUndo functionality above.
            var moveCommand = new MoveNodesCommand(m_NodesToMove, m_Positions, m_ParentSequences, markUndo:false);
            m_GraphView.Dispatcher.DispatchImmediate(moveCommand, false);
            m_PointerDeltaPrev = deltaPointerPos;
            
            // Update insertion indicator.
            if (m_IsSelectionSequenceable && TryGetSequenceableDropTarget(evt.position, out NodeUI dropTarget))
            {
                VisualElement sequenceContainer = dropTarget.Q("Sequence");
                float offsetX = 0.0f;
                if (sequenceContainer != null)
                {
                    offsetX = (dropTarget.localBound.width - sequenceContainer.localBound.width) * 0.5f + 4.0f;
                    m_InsertIndicator.style.width = sequenceContainer.localBound.width - 6.0f;
                }
                else
                {
                    m_InsertIndicator.style.width = dropTarget.localBound.width - 4.0f;
                }
                if (!m_IsIndicatorVisible)
                {
                    m_GraphView.Add(m_InsertIndicator);
                    m_IsIndicatorVisible = true;
                    
                }
                int dropIndex = GetDropIndex(evt.position, dropTarget);
                m_InsertIndicator.transform.position = new Vector2(dropTarget.localBound.x + offsetX + 2.0f, GetDropIndexYPosition(dropIndex, dropTarget));
            }
            else if (m_IsIndicatorVisible)
            {
                m_InsertIndicator.RemoveFromHierarchy();
                m_IsIndicatorVisible = false;
            }
            
            // Refresh visuals to reflect model changes.
            m_GraphViewState.RefreshNodeUI(true, m_NodeUiToRefresh); 
        }
        
        private void OnPointerUpEvent(PointerUpEvent evt)
        {
            if (!CanStopManipulation(evt))
            {
                return;
            }
#if UNITY_EDITOR
            int undoGroup = -1;
            if (m_IsDragging)
            {
                UnityEditor.Undo.SetCurrentGroupName("Move Nodes");
                undoGroup = UnityEditor.Undo.GetCurrentGroup();
            }
            
#endif

            m_IsActive = false;
            target.ReleasePointer(evt.pointerId);

            if (m_IsIndicatorVisible)
            {
                m_InsertIndicator.RemoveFromHierarchy();
                m_IsIndicatorVisible = false;
            }

            if (m_IsDragging)
            {
                m_IsDragging = false;
                foreach (GraphElement graphElement in m_GraphViewState.Selected)
                {
                    if (graphElement.IsMoveable)
                    {
                        graphElement.RemoveFromClassList("MoveManipulatorActive");
                    }
                }

                // Handle Drop.
                if (m_IsSelectionSequenceable && TryGetSequenceableDropTarget(evt.position, out NodeUI dropTarget))
                {
                    HandleDrop(evt.position, dropTarget);
                }
            }
            
            m_GraphViewState.RefreshFromAsset(false); 

#if UNITY_EDITOR
            if (m_IsDragging)
            {
                UnityEditor.Undo.CollapseUndoOperations(undoGroup); 
            }
#endif
        }

        private static float GetDropIndexYPosition(int dropIndex, NodeUI dropTarget)
        {
            if (!dropTarget.IsGroup)
            {
                return dropIndex == 0 ? dropTarget.Position.y : dropTarget.Position.y + dropTarget.localBound.height;
            }

            if (dropTarget.childCount == 0)
            {
                return dropTarget.Position.y;
            }
            if (dropIndex < dropTarget.childCount)
            {
                VisualElement child = dropTarget[dropIndex];
                return dropTarget.WorldToLocal(new Vector2(0.0f, child.worldBound.yMin)).y + dropTarget.localBound.y;
            } 
            else
            {
                VisualElement child = dropTarget[dropTarget.childCount - 1];
                return dropTarget.WorldToLocal(new Vector2(0.0f, child.worldBound.yMax)).y + dropTarget.localBound.y;
            }
        }

        private static int GetDropIndex(Vector2 worldMousePosition, NodeUI dropTarget)
        {
            Vector2 localPos = dropTarget.WorldToLocal(worldMousePosition);
            if (dropTarget.IsGroup)
            {
                int index = 0;
                foreach (VisualElement child in dropTarget.Children())
                {
                    if (worldMousePosition.y <= child.worldBound.center.y)
                    {
                        return index;
                    } 
                    if (worldMousePosition.y <= child.worldBound.yMax)
                    {
                        return index + 1;
                    }
                    ++index;
                }
                return dropTarget.childCount;
            }

            
            return localPos.y < dropTarget.localBound.height / 2 ? 0 : 1;
        }
        
        private bool TryGetSequenceableDropTarget(Vector2 mousePos, out NodeUI dropTarget)
        {
            dropTarget = null;

            foreach (NodeUI graphElement in m_GraphViewState.Nodes)
            {
                if (graphElement.ContainsPoint(graphElement.WorldToLocal(mousePos)) && !m_GraphViewState.Selected.Contains(graphElement) && graphElement.GetFirstAncestorOfType<NodeUI>() == null)
                {
                    dropTarget = graphElement;
                    NodeUI targetParent = dropTarget.GetFirstAncestorOfType<NodeUI>();
                    if (targetParent != null && targetParent.IsSequenceable)
                    {
                        dropTarget = targetParent;
                    }
                    if (dropTarget.IsSequenceable)
                    {
                        return true;
                    }
                    dropTarget = null;
                }
            }
            return false;
        }
        
        private void HandleDrop(Vector2 mousePos, NodeUI dropTarget)
        {
            Dictionary<NodeModel, NodeUI> nodeModelToNodeUI = new Dictionary<NodeModel, NodeUI>();
            List<NodeModel> nodesToAdd = new();
            List<SequenceNodeModel> sequencesToDelete = new(); 
            int index = GetDropIndex(mousePos, dropTarget);
            if (dropTarget.IsGroup)
            {
                int count = 0;
                int newIndex = index;
                foreach (VisualElement child in dropTarget.Children())
                {
                    if (++count > index)
                    {
                        break;
                    }
                    if (m_GraphViewState.Selected.Contains(child))
                    {
                        newIndex = Mathf.Max(newIndex - 1, 0);
                    }
                }
                index = newIndex;
            }

            // Collect selected nodes to add to sequence.
            foreach (NodeUI selectedNodeUI in m_GraphViewState.Selected.OfType<NodeUI>())
            {
                if (selectedNodeUI is SequenceGroup sequenceGroup)
                {
                    // Selected node is a group. Add nested nodes to new sequence.
                    foreach (var nodeInSequence in sequenceGroup.Children().OfType<NodeUI>())
                    {   
                        nodesToAdd.Add(nodeInSequence.Model);
                        nodeModelToNodeUI.Add(nodeInSequence.Model, nodeInSequence);
                    }
                    
                    // Add the selected parent sequence to deletion list.
                    sequencesToDelete.Add(sequenceGroup.Model as SequenceNodeModel);
                }
                else
                {
                    nodesToAdd.Add(selectedNodeUI.Model);
                    nodeModelToNodeUI.Add(selectedNodeUI.Model, selectedNodeUI);
                }
                
                selectedNodeUI.MarkDirtyAndRepaint();
            }

            nodesToAdd.Sort((NodeModel node1, NodeModel node2) =>
            {
                var worldBound1 = nodeModelToNodeUI[node1].worldBound;
                var worldBound2 = nodeModelToNodeUI[node2].worldBound;
                if (Mathf.Approximately(worldBound1.position.y, worldBound2.position.y))
                {
                    return Comparer<float>.Default.Compare(worldBound1.position.x, worldBound2.position.x);
                }
                return Comparer<float>.Default.Compare(worldBound1.position.y, worldBound2.position.y);
            });

            if (dropTarget is SequenceGroup)
            {
                // Add all selected nodes to the target sequence.
                SequenceNodeModel targetSequence = dropTarget.Model as SequenceNodeModel;
                m_GraphView.Dispatcher.DispatchImmediate(new AddNodesToSequenceCommand(targetSequence, index, nodesToAdd, sequencesToDelete));
            }
            else
            {
                // Create new sequence from drop target and selected nodes.
                bool insertAtTop = index == 0;
                m_GraphView.Dispatcher.DispatchImmediate(new CreateNewSequenceOnDropCommand(insertAtTop, dropTarget.Model, nodesToAdd, sequencesToDelete));
            }
            m_GraphViewState.DeselectAll();
        }
    }
}
#endif