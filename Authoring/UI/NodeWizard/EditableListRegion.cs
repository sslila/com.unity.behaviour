#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class EditableListRegion : VisualElement
    {
        private readonly List<string> m_EditableList;

        internal event System.Action OnListUpdated;

        internal Button AddButton;
        internal string FieldPlaceholderName;

        private const string k_DuplicateErrorText = "Names must be unique and cannot be empty.";

        private TextFieldWithValidation m_LastAddedTextField;

        internal EditableListRegion(List<string> listToEdit)
        {
            ResourceLoadAPI
                .Load<VisualTreeAsset>(
                    "Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/EditableListRegionLayout.uxml")
                .CloneTree(this);
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>(
                "Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/EditableListRegionStylesheet.uss"));
            m_EditableList = listToEdit;

            AddButton = this.Q<Button>("AddItemButton");
            AddButton.RegisterCallback<ClickEvent>(OnAddNewItemClicked);
        }

        internal void OnAddNewItemClicked(ClickEvent _)
        {
            m_EditableList.Add(FieldPlaceholderName != null
                ? $"{FieldPlaceholderName} {m_EditableList.Count + 1}"
                : "");
            UpdateList();
            m_LastAddedTextField?.Focus();
        }

        internal void UpdateList()
        {
            VisualElement listItemContainer = this.Q<VisualElement>("ListItemContainer");
            listItemContainer.Clear();

            int i = 0;
            foreach (string itemName in m_EditableList)
            {
                TemplateContainer listItemUI = ResourceLoadAPI
                    .Load<VisualTreeAsset>(
                        "Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/EditableListItemLayout.uxml")
                    .CloneTree();
                TextFieldWithValidation textField = listItemUI.Q<TextFieldWithValidation>("ItemField");
                textField.Value = itemName;
                textField.userData = i;
                textField.RegisterValueChangingCallback(OnListItemUpdate);
                m_LastAddedTextField = textField;

                textField.m_CustomValidationMethods.Add(() =>
                {
                    if (IncludesDuplicates() && IsFieldDuplicate(textField))
                    {
                        textField.tooltip = k_DuplicateErrorText;
                        return false;
                    }

                    return true;
                });

                IconButton removeButton = listItemUI.Q<IconButton>("RemoveItemButton");
                removeButton.userData = i;
                removeButton.RegisterCallback<ClickEvent>(OnRemoveListFieldItem);

                listItemContainer.Add(listItemUI);
                i++;
            }

            OnListUpdated?.Invoke();
        }

#if UNITY_EDITOR
        private bool IsFieldDuplicate(TextFieldWithValidation field)
        {
            int count = m_EditableList.Count(str => str == field.Field.value);
            return count >= 2;
        }
#endif

        internal void OnListItemUpdate(ChangingEvent<string> evt)
        {
            if (evt.target is not TextFieldWithValidation textField)
            {
                return;
            }

            int index = (int)textField.userData;
            m_EditableList[index] = evt.newValue;

            OnListUpdated?.Invoke();
        }

        private void OnRemoveListFieldItem(ClickEvent e)
        {
            if (e.target is not IconButton removeButton)
            {
                return;
            }

            m_EditableList.RemoveAt((int)removeButton.userData);
            UpdateList();
        }

        public bool IncludesDuplicates()
        {
            return m_EditableList.Select(RemoveSpaces).ToList().GroupBy(value => value)
                .Any(g => g.Count() > 1 || string.IsNullOrWhiteSpace(g.Key));
        }

        private static string RemoveSpaces(string str)
        {
            return str.Replace(" ", string.Empty);
        }

        internal bool AllItemFieldsValid()
        {
            List<TextFieldWithValidation> fields = this.Query<TextFieldWithValidation>().ToList();
            foreach (TextFieldWithValidation validationTextField in fields)
            {
                validationTextField.Validate();
            }

            return fields.All(field => field.IsValid);
        }
    }
}
#endif