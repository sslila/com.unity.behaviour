using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class Popup<RootElementType> where RootElementType : VisualElement, new()
    {
        private VisualElement Overlay;
        public RootElementType Root;

        public void Close()
        {
            Overlay.RemoveFromHierarchy();
        }

        public static Popup<RootElementType> Show(VisualElement parent, Vector2 worldPosition,
            bool closeOnOutsideClick = true)
        {
            return Show(parent, worldPosition, Vector2.zero, closeOnOutsideClick);
        }

        public static Popup<RootElementType> Show(VisualElement parent, Vector2 worldPosition, Vector2 size,
            bool closeOnOutsideClick = true)
        {
            VisualElement top = parent;
            while (top.parent != null && top.parent.parent != null)
            {
                top = top.parent;
            }

            VisualElement overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.width = new Length(100.0f, LengthUnit.Percent);
            overlay.style.height = new Length(100.0f, LengthUnit.Percent);
            overlay.style.flexShrink = 0.0f;

            RootElementType element = new RootElementType();
            element.style.position = Position.Absolute;
            element.style.left = worldPosition.x;
            element.style.top = worldPosition.y;
            if (size != Vector2.zero)
            {
                element.style.width = size.x;
                element.style.height = size.y;
            }

            overlay.Add(element);
            top.Add(overlay);
            element.Focus();

            if (closeOnOutsideClick)
            {
                overlay.RegisterCallback<MouseDownEvent>((e) =>
                {
                    overlay.RemoveFromHierarchy();
                    parent?.Focus();
                });
            }


            Popup<RootElementType> popup = new Popup<RootElementType>();
            popup.Overlay = overlay;
            popup.Root = element;

            element.RegisterCallback<GeometryChangedEvent>(OnPopupGeometryChanged);

            return popup;
        }

        private static void OnPopupGeometryChanged(GeometryChangedEvent evt)
        {
            VisualElement element = evt.target as VisualElement;
            if (element.worldBound.xMax > element.parent.worldBound.xMax)
            {
                element.style.left = element.style.left.value.value -
                                     (element.worldBound.xMax - element.parent.worldBound.xMax) - 10;
            }

            if (element.worldBound.yMax > element.parent.worldBound.yMax)
            {
                element.style.top = element.style.top.value.value -
                                    (element.worldBound.yMax - element.parent.worldBound.yMax) - 10;
            }
        }
    }
}
