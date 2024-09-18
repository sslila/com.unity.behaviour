#if UNITY_EDITOR
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class FloatingPanelManipulator : PointerManipulator
    {
        private bool m_IsDragging;

        private Vector2 TargetStartPosition { get; set; }
        private Vector3 PointerStartPosition { get; set; }
        private Vector2 PanelStartSize { get; set; }
        private Vector2 PanelStartPosition { get; set; }

        private FloatingPanel Panel => target as FloatingPanel;

        private VisualElement View { get; }

        private Selection m_Selected;

        private bool m_IsResizing;

        private const float k_ResizerSize = 10f;

        public FloatingPanelManipulator(VisualElement view)
        {
            View = view;
            View.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
            View.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
            View.RegisterCallback<GeometryChangedEvent>(OnParentGeometryChanged);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
        }

        private void OnPointerMoveEvent(PointerMoveEvent evt)
        {
            if (m_IsDragging)
            {
                if (!target.HasPointerCapture(evt.pointerId))
                {
                    target.CapturePointer(evt.pointerId);
                }
                Move(evt);
            }

            else if (m_IsResizing)
            {
                Resize(evt.position);
            }
        }

        private void Move(PointerMoveEvent evt)
        {
            Vector3 pointerDelta = evt.position - PointerStartPosition;
            
            Panel.transform.position = new Vector2((TargetStartPosition.x + pointerDelta.x), (TargetStartPosition.y + pointerDelta.y));
            Panel.ClampPositionWithinParent();

#if UNITY_EDITOR
            SaveFloatingPanelEditorPrefs();
#endif
        }

        private void OnPointerUpEvent(PointerUpEvent evt)
        {
            if (m_IsDragging && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }

            if (m_IsResizing)
            {
                m_Selected = Selection.None;
                target.ReleasePointer(evt.pointerId);
            }
#if UNITY_EDITOR
            SaveFloatingPanelEditorPrefs();
#endif
            m_IsResizing = false;
            m_IsDragging = false;
        }

        private void OnPointerDownEvent(PointerDownEvent evt)
        {
            PanelStartSize = Panel.layout.size;
            PanelStartPosition = Panel.transform.position;
            if (m_IsResizing)
            {
                evt.StopImmediatePropagation();
                return;
            }
            VisualElement targetElement = evt.target as VisualElement;

            m_Selected = GetPointerSelectionOnPanel(evt);
            if (m_Selected != Selection.None)
            {
                m_IsResizing = true;
                PointerStartPosition = evt.position;
                target.CapturePointer(evt.pointerId);
                return;
            }
            if (targetElement == null)
            {
                return;
            }
            
            // Enable dragging only when the floating panel content element or the AppBar is being dragged.
            if (targetElement.name == "PanelContent" || targetElement is AppBar)
            {
                TargetStartPosition = Panel.transform.position;
                m_IsDragging = true;
                PointerStartPosition = evt.position;
            }
        }

        private void Resize(Vector3 pointerPosition)
        {
            Vector2 deltaFromStart = pointerPosition - PointerStartPosition;
            if (!Panel.parent.worldBound.Contains(pointerPosition))
            {
                return;
            }
            
            Vector2 positionOffset = Vector2.zero;
            Vector2 sizeOffset = Vector2.zero;

            switch (m_Selected)
            {
                case Selection.Upper:
                {
                    positionOffset.y += deltaFromStart.y;
                    sizeOffset.y -= deltaFromStart.y;
                    break;
                }
                case Selection.Right:
                {
                    sizeOffset.x += deltaFromStart.x;
                    break;
                }
                case Selection.Lower:
                {
                    sizeOffset.y += deltaFromStart.y;
                    break;
                }
                case Selection.Left:
                {
                    sizeOffset.x -= deltaFromStart.x;
                    positionOffset.x += deltaFromStart.x;
                    break;
                }
                case Selection.LowerRight:
                {
                    sizeOffset += deltaFromStart;
                    break;
                }
                case Selection.LowerLeft:
                {
                    sizeOffset.y += deltaFromStart.y;
                    sizeOffset.x -= deltaFromStart.x;
                    positionOffset.x += deltaFromStart.x;
                    break;
                }
                case Selection.UpperRight:
                {
                    sizeOffset.y -= deltaFromStart.y;
                    sizeOffset.x += deltaFromStart.x;
                    positionOffset.y += deltaFromStart.y;
                    break;
                }
                case Selection.UpperLeft:
                {
                    sizeOffset -= deltaFromStart;
                    positionOffset = deltaFromStart;
                    break;
                }
            }
            Vector2 unclampedNewSize = PanelStartSize + sizeOffset;
            
            // Clamp new calculated size between panel minimum width and height values.
            float largerWidth = Mathf.Max(unclampedNewSize.x, Panel.resolvedStyle.minWidth.value + k_ResizerSize * 2);
            float largerHeight = Mathf.Max(unclampedNewSize.y, Panel.resolvedStyle.minHeight.value + k_ResizerSize * 2 + Panel.Q<AppBar>().resolvedStyle.height);
            Vector2 clampedNewSize = new Vector2(largerWidth, largerHeight);

            Panel.style.width = clampedNewSize.x;
            Panel.style.height = clampedNewSize.y;

            Vector2 finalSizeDifference = clampedNewSize - PanelStartSize;
            positionOffset.x = Mathf.Min(Mathf.Abs(positionOffset.x), Mathf.Abs(finalSizeDifference.x)) * Mathf.Sign(positionOffset.x);
            positionOffset.y = Mathf.Min(Mathf.Abs(positionOffset.y), Mathf.Abs(finalSizeDifference.y)) * Mathf.Sign(positionOffset.y);
            Panel.transform.position = PanelStartPosition + positionOffset;
        }

        private Selection GetPointerSelectionOnPanel(PointerDownEvent evt)
        {
            VisualElement targetElement = evt.target as VisualElement;
            
            if (targetElement != null && targetElement.GetFirstAncestorOfType<FloatingPanel>() == Panel)
            {
                return targetElement.name switch
                {
                    "Upper" => Selection.Upper,
                    "Right" => Selection.Right,
                    "Lower" => Selection.Lower,
                    "Left" => Selection.Left,
                    "UpperLeft" => Selection.UpperLeft,
                    "UpperRight" => Selection.UpperRight,
                    "LowerLeft" => Selection.LowerLeft,
                    "LowerRight" => Selection.LowerRight,
                    _ => Selection.None
                };

            }
            return Selection.None;
        }

        private enum Selection
        {
            None,
            UpperLeft,
            UpperRight,
            LowerLeft,
            LowerRight,
            Upper,
            Right,
            Lower,
            Left
        }

        private void OnParentGeometryChanged(GeometryChangedEvent evt)
        {
            Panel.ClampPositionWithinParent();
            Panel.ClampSizeWithinParentSize();
#if UNITY_EDITOR
            SaveFloatingPanelEditorPrefs();
#endif
        }

        private void SaveFloatingPanelEditorPrefs()
        {
            bool inEditorContext = View.panel.contextType == ContextType.Editor;
            GraphPrefsUtility.SetFloat(Panel.XPosPrefsKey, Panel.transform.position.x, inEditorContext);
            GraphPrefsUtility.SetFloat(Panel.YPosPrefsKey, Panel.transform.position.y, inEditorContext);

            GraphPrefsUtility.SetFloat(Panel.WidthPrefsKey, Panel.layout.width, inEditorContext);
            GraphPrefsUtility.SetFloat(Panel.HeightPrefsKey, Panel.layout.height, inEditorContext);
        }
    }
}
#endif