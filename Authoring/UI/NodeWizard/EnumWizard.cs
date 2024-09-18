#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class EnumWizard : VisualElement
    {
        private const string k_NameField = "EnumNameField";
        private const float k_WizardWidth = 316f;

        private const string k_NameFieldPlaceholderName = "New Enum";
        private const string k_MemberPlaceholderName = "Member";
        
        private readonly List<string> m_EnumMembers = new();
        private readonly EditableListRegion m_EnumMembersRegion;
        
        internal WizardStepper Stepper;
        private Modal m_Modal;

        internal delegate void OnEnumTypeCreatedCallback(string enumClassName);
        internal OnEnumTypeCreatedCallback OnEnumTypeCreated;
        private TextFieldWithValidation m_NameField;

        public EnumWizard(Modal modal, WizardStepper stepper)
        {
            m_Modal = modal;
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/EnumWizardStylesheet.uss"));
            var viewTemplate = ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/EnumWizardLayout.uxml");
            viewTemplate.CloneTree(this);

            m_NameField = this.Q<TextFieldWithValidation>(k_NameField);
            this.Q<HelpText>().Text = "Enum types can be used to logically group values, e.g. 'Status => Idle, Fleeing, Attacking, Patrolling'. You can then create variables in the blackboard and control the flow of your behavior, for example by using a 'Switch' node.";
            
            // Set up a custom region for enum member creation.
            m_EnumMembersRegion = new EditableListRegion(m_EnumMembers);
            m_EnumMembersRegion.AddButton.title = "Add " + k_MemberPlaceholderName;
            m_EnumMembersRegion.FieldPlaceholderName = k_MemberPlaceholderName;
            Add(m_EnumMembersRegion);
            
            // Add one default member.
            m_EnumMembers.Add(k_MemberPlaceholderName + " 1");
            m_EnumMembersRegion.UpdateList();

            Stepper = stepper;
            Stepper.StepperContainer.style.width = k_WizardWidth;

            UpdateCreateButtonState();
            
            Stepper.ConfirmButton.clicked += OnCreateClicked;
            
            m_EnumMembersRegion.OnListUpdated += () =>
            {
                ValidateAllTextFields();
                UpdateCreateButtonState();
            };

            
            m_NameField.PlaceholderText = k_NameFieldPlaceholderName;
            m_NameField.OnItemValidation += UpdateCreateButtonState;
            m_NameField.Value = m_NameField.PlaceholderText;
            
            UpdateCreateButtonState();
        }

        private void UpdateCreateButtonState()
        {
            bool allValid = m_EnumMembers.Count > 0 && m_NameField.ValidateWithoutNotify() && !m_EnumMembersRegion.IncludesDuplicates() && m_EnumMembersRegion.AllItemFieldsValid();
            
            Stepper.ConfirmButton.SetEnabled(allValid);
        }

        private void ValidateAllTextFields()
        {
            List<TextFieldWithValidation> validationFields = this.Query<TextFieldWithValidation>().ToList();
            foreach (TextFieldWithValidation validationTextField in validationFields)
            {
                validationTextField.Validate();
            }
        }

        private void OnCreateClicked()
        {
            string className = GeneratorUtils.RemoveSpaces(m_NameField.Value);
            if (EnumGeneratorUtility.CreateEnumAsset(className, m_EnumMembers))
            {
                OnEnumTypeCreated?.Invoke(className);
                m_Modal.Dismiss();
            }
        }

        internal void OnShow()
        {
            schedule.Execute(m_NameField.Focus);
        }
    }
}
#endif