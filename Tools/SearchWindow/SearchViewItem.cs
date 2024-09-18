using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace UnityEngine.UIExtras
{
    internal class SearchViewItem : VisualElement
    {
        Label m_Label;
        Icon m_Icon;
        Icon m_NextIcon;

        string m_Name;
        public string Name => m_Name;
        public string Description { get => tooltip; set { tooltip = value; } }
        public SearchViewItem()
        {
            focusable = false;
            AddToClassList("SearchItem");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/SearchWindow/Assets/SearchItemStyle.uss"));
            VisualTreeAsset visualTree = ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/SearchWindow/Assets/SearchItem.uxml");
            visualTree.CloneTree(this);

            m_Label = this.Q<Label>("Label");
            m_Icon = this.Q<Icon>("Icon");
            m_NextIcon = this.Q<Icon>("NextIcon");
            m_NextIcon.iconName = "sub-menu-indicator";
            tabIndex = -1;
        }

        public TreeNode<SearchView.Item> Item {
            get => userData as TreeNode<SearchView.Item>;
            set {
                userData = value;
                if (value.Value.Icon != null)
                {
                    m_Icon.image = value.Value.Icon;
                }
                else if (!string.IsNullOrEmpty(value.Value.IconName))
                {
                    m_Icon.iconName = value.Value.IconName;                    
                }

                m_Icon.EnableInClassList("HiddenIcon", value.Value.Icon == null && string.IsNullOrEmpty(value.Value.IconName));
                m_Name = value.Value.Name;
                m_Label.text = Name;
                Description = value.Value.Description;

                if (!value.Value.Enabled)
                {
                    m_Label.style.color = new StyleColor(Color.gray);
                }

                if (value.ChildCount == 0)
                {
                    m_NextIcon.style.visibility = Visibility.Hidden;
                }
                else
                {
                    m_NextIcon.style.visibility = Visibility.Visible;
                }
            }
        }
    }
}