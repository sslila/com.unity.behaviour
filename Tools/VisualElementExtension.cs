using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal static class VisualElementExtensions
    {
        public static void SetVisible(this VisualElement ve, bool visible)
        {
            ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static bool IsVisible(this VisualElement ve) => ve.style.display.value == DisplayStyle.Flex;

        public static void Show(this VisualElement ve) => ve.SetVisible(true);

        public static void Hide(this VisualElement ve) => ve.SetVisible(false);
    }
}