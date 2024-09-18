using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class GraphUIUtility
    {
        private const float k_PopupWindowPadding = 25f;
        
        internal static void PlacePopupAt<T>(AnchorPopup<T> popup, Vector2 position, float width, float height) where T : AnchorPopup<T>
        {
            Panel appUiPanel = popup.anchor.GetFirstAncestorOfType<Panel>();
            Rect anchorRect = popup.anchor.worldBound;
            anchorRect.position -= appUiPanel.worldBound.position;
            position = new Vector2(Mathf.Clamp(position.x, 0, anchorRect.width - width - k_PopupWindowPadding),
                Mathf.Clamp(position.y, 0, anchorRect.height + k_PopupWindowPadding - height));
            Vector2 offset = position - anchorRect.position;
            popup.SetPlacement(PopoverPlacement.BottomLeft).
                SetCrossOffset(Mathf.RoundToInt(offset.x)).
                SetOffset(Mathf.RoundToInt(offset.y - anchorRect.height));
        }   
    }
}