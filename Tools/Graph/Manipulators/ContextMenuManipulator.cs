#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class ContextMenuManipulator : PointerManipulator
    {
        GraphView Target => target as GraphView;
        Vector2 MousePos { get; set; }
        GraphElement ClickedElement { get; set; }

        public ContextMenuManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        protected virtual void OnPointerUpEvent(PointerUpEvent evt)
        {
            if (!CanStartManipulation(evt))
            {
                return;
            }

            ClickedElement = null;
            MousePos = evt.position;
            NodeUI clickedNode = Target.NodeAt(MousePos);
            Edge clickedEdge = Target.EdgeAt(MousePos);
            
            ContextMenu menu = new ContextMenu(Target);
            if (clickedNode == null && clickedEdge == null)
            {
                menu.AddItem("Add...", OnAdd);
            }
            else
            {
                ClickedElement = clickedNode != null ? clickedNode : clickedEdge;
                menu.AddItem("Delete", OnDelete);
            }

            menu.Show();
        }

        private void OnAdd()
        {
            Target.ShowNodeSearch(MousePos);
        }

        private void OnDelete()
        {
            if (ClickedElement == null || Target.ViewState.Selected.Contains(ClickedElement)) 
            {
                DeleteSelected();
            }
            else
            {
                if (ClickedElement is Edge edge)
                {
                    NodeUI start = edge.Start.GetFirstAncestorOfType<NodeUI>();
                    NodeUI end = edge.End.GetFirstAncestorOfType<NodeUI>();
                    PortModel startPort = start.Model.FindPortModelByName(edge.Start.name);
                    PortModel endPort = end.Model.FindPortModelByName(edge.End.name);
                    
                    Target.Dispatcher.DispatchImmediate(new DeleteEdgeCommand(startPort, endPort, false));
                }
                else if (ClickedElement is NodeUI node && node.Model != null)
                {
                    Target.Dispatcher.DispatchImmediate(new DeleteNodeCommand(node.Model, false));
                }
            }

            ClickedElement = null;
        }

        void DeleteSelected()
        {
            // The code below is duplicated in DeleteManipulator.OnKeyDown()
            List<GraphElement> notDeleted = new List<GraphElement>();
            List<Tuple<PortModel, PortModel>> edgesToDelete = new();
            List<NodeModel> nodesToDelete = new();
            foreach (GraphElement element in Target.ViewState.Selected)
            {
                if (element is Edge edge && edge.IsDeletable)
                {
                    edgesToDelete.Add(new Tuple<PortModel, PortModel>(edge.Start.PortModel, edge.End.PortModel));
                } 
                else if (element is NodeUI nodeUI && nodeUI.IsDeletable)
                {
                    nodesToDelete.Add(nodeUI.Model);
                } 
                else
                {
                    notDeleted.Add(element);
                }
            }
            Target.ViewState.SetSelected(notDeleted);
            Target.Dispatcher.Dispatch(new DeleteNodesAndEdgesCommand(edgesToDelete, nodesToDelete, markUndo:true));
        }
    }
}
#endif