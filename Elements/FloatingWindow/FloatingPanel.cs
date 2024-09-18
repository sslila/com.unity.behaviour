#if UNITY_EDITOR
using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Behavior.GraphFramework
{
    internal class FloatingPanel : ExVisualElement
    {
        internal VisualElement Content { get; }
        
        public string Title
        {
            get => m_PanelAppBar.title;
            set => m_PanelAppBar.title = value;
        }

        private readonly AppBar m_PanelAppBar;
        private readonly IconButton m_CloseButton;

        private bool m_TitleBarPointerDown = false;
        public bool IsCollapsable
        {
            get => m_IsCollapsable;
            set
            {
                m_IsCollapsable = value;
                SetCollapsable();   
            }
        }
        private bool m_IsCollapsable;
        private void SetCollapsable()
        {
            if (m_IsCollapsable)
            {
                Icon collapseIcon = new Icon
                {
                    iconName = "caret-down",
                    name = "CollapseIcon"
                };
                SetupCollapsableAppBarTitle(collapseIcon);   
            }
        }

        public enum DefaultPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        private DefaultPosition m_DefaultPosition;
        private const float k_DefaultHeight = 300f;

        private void SetupCollapsableAppBarTitle(Icon collapseIcon)
        {
            VisualElement titleContainer = new VisualElement();
            titleContainer.name = "CollapseTitleContainer";
            LocalizedTextElement titleText = m_PanelAppBar.Q<LocalizedTextElement>("appui-appbar__compact-title");
            titleContainer.Add(titleText);
            titleContainer.Add(collapseIcon);
            titleContainer.pickingMode = PickingMode.Ignore;
            m_PanelAppBar.Q<VisualElement>("appui-appbar__bar").Add(titleContainer);
            m_PanelAppBar.RegisterCallback<PointerDownEvent>(OnTitleBarPointerDown);
            m_PanelAppBar.RegisterCallback<PointerUpEvent>(OnTitleBarPointerUp);
        }

        internal readonly string XPosPrefsKey;
        internal readonly string YPosPrefsKey;
        internal readonly string WidthPrefsKey;
        internal readonly string HeightPrefsKey;
        internal readonly string IsCollapsedPrefsKey;

        private float m_ExpandedHeight;

        public static FloatingPanel Create(VisualElement content, VisualElement view, string name, DefaultPosition defaultPosition = DefaultPosition.TopLeft, bool showCloseButton = false)
        {
            FloatingPanel panel = new FloatingPanel(content, view, name, defaultPosition, showCloseButton);
            return panel;
        }

        public static void Remove(FloatingPanel panel)
        {
            panel.parent?.Remove(panel);
        }
        
        public void Remove()
        {
            parent.Remove(this);
        }

        private FloatingPanel(VisualElement content, VisualElement view, string name, DefaultPosition defaultPosition = DefaultPosition.TopLeft, bool showCloseButton = false)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/FloatingPanelStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Elements/Assets/FloatingPanelLayout.uxml").CloneTree(this);
         
            this.AddManipulator(new FloatingPanelManipulator(view));

            Content = content;
            this.name = name;
            
            m_DefaultPosition = defaultPosition;
            
            m_PanelAppBar = this.Q<AppBar>("FloatingPanelAppBar");
            // To ensure that the AppBar is detectable by pointer events on all editor versions.
            m_PanelAppBar.pickingMode = PickingMode.Position;

            m_CloseButton = this.Q<IconButton>("CloseButton");
            if (showCloseButton)
            {
                m_CloseButton.style.display = DisplayStyle.Flex;
                m_CloseButton.clicked += Remove;
            }
            
            // Set preference keys.
            XPosPrefsKey =  name + "FrameX";
            YPosPrefsKey =  name + "FrameY";
            WidthPrefsKey = name + "FrameWidth";
            HeightPrefsKey = name + "FrameHeight";
            IsCollapsedPrefsKey = name + "IsCollapsed";

            // Set panel default position and size.
            SetPositionFromDefaultPosition();
            style.width = style.minWidth;
            style.height = k_DefaultHeight;
            
#if UNITY_EDITOR
            bool inEditorContext = view.panel.contextType == ContextType.Editor;

            // If position and size are saved to graph preferences, set those.
            if (GraphPrefsUtility.HasKey(XPosPrefsKey, inEditorContext) && GraphPrefsUtility.HasKey(YPosPrefsKey, inEditorContext))
            {
                transform.position = new Vector3(
                    GraphPrefsUtility.GetFloat(XPosPrefsKey, 0f, inEditorContext),
                    GraphPrefsUtility.GetFloat(YPosPrefsKey, 0f, inEditorContext)
                    );
                ClampPositionWithinParent();
            }
            if (GraphPrefsUtility.HasKey(WidthPrefsKey, inEditorContext) && GraphPrefsUtility.HasKey(HeightPrefsKey, inEditorContext))
            {
                style.width = GraphPrefsUtility.GetFloat(WidthPrefsKey, 0f, inEditorContext);
                style.height = GraphPrefsUtility.GetFloat(HeightPrefsKey, 0f, inEditorContext);
            }
#endif
            // If content has a parent, remove it from existing hierarchy.
            if (content.parent != null)
            {
                content.RemoveFromHierarchy();   
            }
            
            this.Q<VisualElement>("PanelContent").Add(Content);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
#if UNITY_EDITOR
            if (GraphPrefsUtility.GetBool(IsCollapsedPrefsKey, false, evt.destinationPanel.contextType == ContextType.Editor))
            {
                CollapsePanel();
            }
#endif
        }

        private void OnTitleBarPointerDown(PointerDownEvent evt)
        {
            if (IsCollapsable && evt.button == 0)
            {
                m_TitleBarPointerDown = true;
            }
        }

        private void OnTitleBarPointerUp(PointerUpEvent evt)
        {
            // Expand or collapse the floating panel if the panel is set to collapsable.
            if (m_TitleBarPointerDown && IsCollapsable && evt.button == 0)
            {
                if (evt.target is Button or IconButton)
                {
                    return;
                }
                if (!ClassListContains("CollapsedPanel"))
                {
                    CollapsePanel();
                }
                else
                {
                    ExpandPanel();
                }
            }
            m_TitleBarPointerDown = false;
        }

        internal void CollapsePanel()
        {
            RemoveFromClassList("ExpandedPanel");
            AddToClassList("CollapsedPanel");
            this.Q<Icon>("CollapseIcon").iconName = "caret-up";
            m_ExpandedHeight = resolvedStyle.height;
            style.height = m_PanelAppBar.resolvedStyle.height;

            bool inEditorContext = panel.contextType == ContextType.Editor;
            GraphPrefsUtility.SetBool(IsCollapsedPrefsKey, true, inEditorContext);
        }

        internal void ExpandPanel()
        {
            RemoveFromClassList("CollapsedPanel");
            AddToClassList("ExpandedPanel");
            this.Q<Icon>("CollapseIcon").iconName = "caret-down";
            if (m_ExpandedHeight != 0)
            {
                style.height = m_ExpandedHeight;   
            }

            bool inEditorContext = panel.contextType == ContextType.Editor;
            GraphPrefsUtility.SetBool(IsCollapsedPrefsKey, false, inEditorContext);
        }

        internal void PreventCollapsingThisFrame()
        {
            m_PanelAppBar.UnregisterCallback<PointerUpEvent>(OnTitleBarPointerUp);
            schedule.Execute(() => m_PanelAppBar.RegisterCallback<PointerUpEvent>(OnTitleBarPointerUp));
        }

        private void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            ClampPositionWithinParent();
            ClampSizeWithinParentSize();
        }

        internal void ClampSizeWithinParentSize()
        {
            if (parent == null)
            {
                return;
            }
            style.width = Mathf.Clamp(resolvedStyle.width, resolvedStyle.width, parent.worldBound.width);
            style.height = Mathf.Clamp(resolvedStyle.height, resolvedStyle.height, parent.worldBound.height);
        }
        
        internal void ClampPositionWithinParent()
        {
            if (parent == null)
            {
                return;
            }
            Vector3 position = transform.position;
            transform.position = new Vector2(
                Mathf.Clamp(position.x, 0, parent.worldBound.width - resolvedStyle.width),
                Mathf.Clamp(position.y, 0, parent.worldBound.height - resolvedStyle.height));
        }

        private void SetPositionFromDefaultPosition()
        {
            transform.position = m_DefaultPosition switch
            {
                DefaultPosition.TopLeft => new Vector3(0, 0, 0),
                DefaultPosition.TopRight => new Vector3(float.PositiveInfinity, 0, 0),
                DefaultPosition.BottomLeft => new Vector3(0, float.PositiveInfinity, 0),
                DefaultPosition.BottomRight => new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0),
                _ => transform.position
            };
        }
    }
}
#endif