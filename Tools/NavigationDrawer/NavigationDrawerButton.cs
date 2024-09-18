using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;
#if ENABLE_UXML_UI_SERIALIZATION
using Unity.Properties;
#endif

namespace UnityEngine.UIExtras
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class NavigationDrawerButton : VisualElement
    {
#if ENABLE_UXML_UI_SERIALIZATION
        internal static readonly BindingId textProperty = nameof(Text);
        internal static readonly BindingId iconProperty = nameof(IconPath);
#endif

        public VisualElement m_Icon;
        public Label m_Label;
        NavigationDrawer m_NavigationDrawer;
        string m_IconPath;

        public delegate void NavigationDrawerButtonClicked();
        public event NavigationDrawerButtonClicked OnClicked;

#if ENABLE_UXML_UI_SERIALIZATION
        [CreateProperty]
        [UxmlAttribute("text")]
#endif
        public string Text 
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;
                
#if ENABLE_UXML_UI_SERIALIZATION
                if (changed)
                    NotifyPropertyChanged(textProperty);
#endif
            }
        }
        
#if ENABLE_UXML_UI_SERIALIZATION
        [CreateProperty]
        [UxmlAttribute("icon")]
#endif
        internal string IconPath 
        {
            get => m_IconPath;
            set
            {
                var changed = m_IconPath != value;
                SetIcon(value);
                    
#if ENABLE_UXML_UI_SERIALIZATION
                if (changed)
                    NotifyPropertyChanged(iconProperty);
#endif
            }
        }

#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<NavigationDrawerButton, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };
            private readonly UxmlStringAttributeDescription m_Icon = new UxmlStringAttributeDescription { name = "icon" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                NavigationDrawerButton button = ve as NavigationDrawerButton;
                button.Text = m_Text.GetValueFromBag(bag, cc);
                string iconPath = m_Icon.GetValueFromBag(bag, cc);
                button.SetIcon(iconPath);
            }
        }
#endif

        public NavigationDrawerButton()
        {
            AddToClassList("NavigationDrawerButton");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/NavigationDrawer/Assets/NavigationDrawerButtonStyle.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/NavigationDrawer/Assets/NavigationDrawerButtonLayout.uxml").CloneTree(this);

            m_Icon = this.Q(className: "NavigationDrawerButton-Icon");
            m_Label = this.Q<Label>(className: "NavigationDrawerButton-Label");

            RegisterCallback<AttachToPanelEvent>(e =>
            {
                m_NavigationDrawer = this.GetFirstAncestorOfType<NavigationDrawer>();
            });

            RegisterCallback<DetachFromPanelEvent>(e =>
            {
                RemoveFromClassList("NavigationDrawerButton__Selected");
            });

            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
        }

        void SetIcon(string iconPath)
        {
            m_IconPath = iconPath;
            if (!string.IsNullOrEmpty(iconPath))
            {
                Texture2D texture = ResourceLoadAPI.Load<Texture2D>(iconPath);
                if (texture != null)
                {
                    m_Icon.style.backgroundImage = texture;
                }
            }
        }

        void OnMouseDownEvent(MouseDownEvent e)
        {
            m_NavigationDrawer?.OnNavigationButtonClicked(this);
            OnClicked?.Invoke();
            e.StopImmediatePropagation();
        }
    }
}