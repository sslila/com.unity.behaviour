using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace UnityEngine.UIExtras
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class NavigationDrawer : VisualElement
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<NavigationDrawer, UxmlTraits> { }
#endif
        VisualElement m_Content;
        NavigationDrawerButton m_MenuButton;
        public override VisualElement contentContainer => m_Content != null ? m_Content : base.contentContainer;

        public NavigationDrawer()
        {
            AddToClassList("NavigationDrawer");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/NavigationDrawer/Assets/NavigationDrawerStyle.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/NavigationDrawer/Assets/NavigationDrawerLayout.uxml").CloneTree(this);
            m_MenuButton = this.Q<NavigationDrawerButton>("NavigationDrawer-ButtonButton");
            m_Content = this.Q(className: "NavigationDrawer-Content");

            this.focusable = true;
            RegisterCallback<BlurEvent>(OnFocusLost);
        }

        private void OnFocusLost(BlurEvent evt)
        {
            RemoveFromClassList("NavigationDrawer__Expanded");
        }

        internal void OnNavigationButtonClicked(NavigationDrawerButton navigationDrawerButton)
        {
            if (navigationDrawerButton == m_MenuButton)
            {
                ToggleInClassList("NavigationDrawer__Expanded");
                return;
            }

            RemoveFromClassList("NavigationDrawer__Expanded");
            for (int i = 0; i < m_Content.childCount; ++i)
            {
                if (m_Content[i] is NavigationDrawerButton button)
                {
                    if (button == navigationDrawerButton)
                    {
                        navigationDrawerButton.AddToClassList("NavigationDrawerButton__Selected");
                    }
                    else
                    {
                        button.RemoveFromClassList("NavigationDrawerButton__Selected");
                    }
                }
            }
        }
    }
}