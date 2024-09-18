#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.Behavior.GenerativeAI;

namespace Unity.Behavior
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class BehaviorToolbar : GraphToolbar
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<BehaviorToolbar, UxmlTraits> {}
#endif
        internal ActionButton DebugButton;

        public BehaviorToolbar()
        {
            AddToClassList("BehaviorToolbar");
            CreateDebugButton();
        }

        private void CreateDebugButton()
        {
            DebugButton = new ActionButton();
            DebugButton.label = "Debug";
            DebugButton.name = "DebugButton";
            DebugButton.icon = "debug";
            this.Q<ActionGroup>("AssetActionsGroup").Add(DebugButton);
        }
    }
}
#endif