#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class AddNodeManipulator : Manipulator
    {
        GraphView Target => target as GraphView;
        long lastOpenTime = 0;

        protected override void RegisterCallbacksOnTarget()
        {
            Target.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            Target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
            {
                OnAddNode(evt);
                evt.StopImmediatePropagation();
            }
        }

        private void OnAddNode(KeyDownEvent evt)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - lastOpenTime < 500)
                return;

            lastOpenTime = now;

            if (evt.target != Target.Background && evt.target != Target)
                return;

            // Get the button position to offset the popup by.
            VisualElement searchButton = evt.target as VisualElement;
#if ENABLE_INPUT_SYSTEM && USE_NEW_INPUT_SYSTEM
            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
            Vector2 mousePos = Input.mousePosition;
#endif
            mousePos.y = Screen.height - mousePos.y;
            Vector2 pos = Vector2.zero;
            if (searchButton.panel.contextType == ContextType.Player)
            {
                pos = RuntimePanelUtils.ScreenToPanel(searchButton.panel, mousePos);
            }
            else
            {
                pos = evt.originalMousePosition;
            }

            SequenceNodeModel sequence = null;
            if (Target.ViewState.Selected.Count() == 1 && Target.ViewState.Selected.First() is SequenceGroup selectedSequenceGroup)
            {
                sequence = selectedSequenceGroup.Model as SequenceNodeModel;
            }
                
            Target.ShowNodeSearch(pos, insertToSequence: sequence);
        }
    }
}
#endif