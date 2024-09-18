#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class PasteNodeManipulator : Manipulator
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
            if (evt.keyCode == KeyCode.V && evt.modifiers is EventModifiers.Control or EventModifiers.Command)
            {
                string jsonString = GUIUtility.systemCopyBuffer;
                NodeCopyData copyData = new NodeCopyData();
                try
                {
                    JsonUtility.FromJsonOverwrite(jsonString, copyData);
                }
                catch
                {
                    return;
                }
                
                Vector2 position = Target.WorldPosToLocal(evt.originalMousePosition);
                Target.Dispatcher.DispatchImmediate(new PasteNodeCommand(copyData.Nodes, position));
            }
        }
    }
}
#endif