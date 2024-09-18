#if UNITY_EDITOR
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class BlackboardAssetElement : VisualElement
    {
        private BlackboardAsset m_Asset;
        public override VisualElement contentContainer => m_Content;
        private VisualElement m_Content;
        private Label m_NameLabel;
        private readonly ListView m_VariableListView;
        private IconButton m_CollapseButton;
        private IconButton m_EditButton;
        private Icon m_Icon;

        public ListView Variables => m_VariableListView;
        
        public string Name 
        {
            get => m_NameLabel.text;
            set => m_NameLabel.text = value;
        }
        
        public BlackboardAssetElement(BlackboardAsset asset) : this()
        {
            m_Asset = asset;
            Name = asset.name;
        }

        public BlackboardAssetElement()
        {
            AddToClassList("BlackboardAssetElement");
            AddToClassList("AssetExpanded");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/BlackboardAssetElementStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/BlackboardAssetElementLayout.uxml").CloneTree(this);
            m_NameLabel = this.Q<Label>("Name");
            m_VariableListView = this.Q<ListView>("Variables");
            m_CollapseButton = this.Q<IconButton>("CollapseIcon");
            m_CollapseButton.clicked += () =>
            {
                if (ClassListContains("AssetExpanded"))
                {
                    Collapse();
                }
                else
                {
                    Expand();  
                }
            };
            m_Icon = this.Q<Icon>("BlackboardAssetIcon");
            m_EditButton = this.Q<IconButton>("EditButton");
            m_EditButton.clicked += () =>
            {
                BlackboardWindowDelegate.Open(m_Asset as BehaviorBlackboardAuthoringAsset);
            };
            
            m_VariableListView.horizontalScrollingEnabled = false;
            m_VariableListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // Get the ScriptableObject editor icon texture.
            m_Icon.image = BlackboardUtils.GetScriptableObjectIcon(m_Asset);
        }

        private void Collapse()
        {
            RemoveFromClassList("AssetExpanded");  
            AddToClassList("AssetCollapsed");
        }

        private void Expand()
        {
            RemoveFromClassList("AssetCollapsed");  
            AddToClassList("AssetExpanded");   
        }
    }
}
#endif