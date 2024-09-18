#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class DeleteManipulator : Manipulator
    {
        GraphView Target => target as GraphView;
        protected override void RegisterCallbacksOnTarget()
        {
            Target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            Target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete || (evt.keyCode is KeyCode.Backspace or KeyCode.Delete && evt.modifiers.HasFlag(EventModifiers.Command)))
            {
                // The code below is duplicated in ContextMenuManipulator.DeleteSelected()
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

                if (edgesToDelete.Count == 0 && nodesToDelete.Count == 0)
                {
                    return;
                }
                Target.Dispatcher.Dispatch(new DeleteNodesAndEdgesCommand(edgesToDelete, nodesToDelete, markUndo:true));
            }
        }
    }
}
#endif