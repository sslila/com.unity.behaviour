using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal static class VisualElementExtensions
    {
        internal static void SetVisible(this VisualElement ve, bool visible)
        {
            ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        internal static bool IsVisible(this VisualElement ve) => ve.style.display.value == DisplayStyle.Flex;

        internal static void Show(this VisualElement ve) => ve.SetVisible(true);

        internal static void Hide(this VisualElement ve) => ve.SetVisible(false);
    }
}