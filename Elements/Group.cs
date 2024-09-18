#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class Group : NodeUI
    {
        VisualElement m_ContentContainer;
        internal VisualElement m_Info;
        VisualElement m_GroupButton;
        VisualElement m_GroupInfo;
        TextField m_NameField;
        Label m_NameLabel;
        Foldout m_FoldoutButton;

        public override VisualElement contentContainer => m_ContentContainer;
        public override bool IsGroup => true;
        public string Name {
            get => m_NameLabel.text;
            set {
                m_NameLabel.text = value;
                m_NameField.SetValueWithoutNotify(value);
            }
        }

        public Group(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Group");

            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/GroupStyles.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/Graph/Assets/GroupLayout.uxml").CloneTree(base.contentContainer);

            m_ContentContainer = this.Q("ContentContainer");
            m_GroupButton = this.Q("GroupButton");
            m_GroupInfo = this.Q("GroupInfo");
            m_FoldoutButton = this.Q<Foldout>("FoldoutButton");
            m_NameField = this.Q<TextField>("NameField");
            m_NameLabel = this.Q<Label>("Name");
            m_Info = this.Q("Info");

            m_GroupButton.RegisterCallback<MouseDownEvent>(OnGroupButton);
            m_FoldoutButton.RegisterValueChangedCallback(OnFoldoutToggled);

            RegisterNameEditCallbacks();
        }

        private void OnGroupButton(MouseDownEvent evt)
        {
            //AddToClassList("NamedGroup");
            SetToNamedGroup();
        }

        internal void SetToNamedGroup()
        {
            m_GroupButton.style.display = DisplayStyle.None;
            m_GroupInfo.style.display = DisplayStyle.Flex;
        }

        private void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            m_ContentContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RegisterNameEditCallbacks()
        {
            m_NameField.isDelayed = true;
            m_NameLabel.RegisterCallback<MouseDownEvent>(OnNameClicked);
            m_NameField.RegisterValueChangedCallback(OnNameEdited);
            m_NameField.RegisterCallback<FocusOutEvent>(OnNameEditUnfocused);
            m_NameField.RegisterCallback<KeyDownEvent>(OnNameEditKeyDown);

        }

        private void OnNameEditUnfocused(FocusOutEvent evt)
        {
            m_NameLabel.style.display = DisplayStyle.Flex;
            m_NameField.style.display = DisplayStyle.None;
        }

        private void OnNameEdited(ChangeEvent<string> evt)
        {
            m_NameLabel.text = evt.newValue;
            m_NameLabel.style.display = DisplayStyle.Flex;
            m_NameField.style.display = DisplayStyle.None;
        }

        private void OnNameClicked(MouseDownEvent evt)
        {
            m_NameLabel.style.display = DisplayStyle.None;
            m_NameField.style.display = DisplayStyle.Flex;
            m_NameField.Focus();

        }

        private void OnNameEditKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                m_NameLabel.style.display = DisplayStyle.Flex;
                m_NameField.style.display = DisplayStyle.None;
            }
        }

        public Vector2 GetNodeWorldPosition(NodeUI node)
        {
            return node.localBound.position + contentContainer.parent.localBound.position + this.localBound.position;
        }
    }
}
#endif