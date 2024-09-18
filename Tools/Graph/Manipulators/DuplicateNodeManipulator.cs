#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class DuplicateNodeManipulator : Manipulator
    {
        private GraphView Target => target as GraphView;
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
            if (evt.keyCode == KeyCode.D && evt.modifiers is EventModifiers.Control or EventModifiers.Command)
            {
                var nodeModelsOriginal = new List<NodeModel>();
                foreach (var node in Target.ViewState.Selected.OfType<NodeUI>())
                {
                    if (node.Model is { IsDuplicatable: true })
                    {
                        nodeModelsOriginal.Add(node.Model);
                    }
                }
                
                if(nodeModelsOriginal.Any())
                {
                    var position = Target.WorldPosToLocal(evt.originalMousePosition);
                    Target.Dispatcher.DispatchImmediate(new DuplicateNodeCommand(nodeModelsOriginal, position));
                }
            }
        }
    }
}
#endif