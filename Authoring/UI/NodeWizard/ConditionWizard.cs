#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ConditionWizard : BaseNodeWizard
    {
        private const string k_NameFieldPlaceholderName = "New Condition";
        private const string k_ConditionWizardHelpText = "Create a custom rule to use as a condition on your node. When conditions are evaluated, they return either true or false.";
        private const string k_ConditionStoryHelpText = "Describe the condition being checked. For example: 'Agent is in proximity to Enemy'";

        private Action<string> m_OnComplete;
        
        public ConditionWizard(Action<string> onOnComplete)
        {
            m_OnComplete = onOnComplete;
            
            StoryField.RegisterValueChangingCallback(OnStoryFieldChanged);

            NameField.PlaceholderText = k_NameFieldPlaceholderName;
            NameField.OnItemValidation += UpdateCreateButtonState;
            StoryField.OnItemValidation += UpdateCreateButtonState;
            AddToRequiredFields(StoryField.Field);
            StoryEditor.SupportedTypes = BlackboardRegistry.GetStoryVariableTypesWithOperators();
        }

        protected override void SetHelpTexts()
        {
            SetInfoHelpText(k_ConditionWizardHelpText);
            SetStoryHelpText(k_ConditionStoryHelpText);
        }

        internal override void SetupWizardStepperModal(WizardStepper stepper, Modal modal)
        {
            Stepper = stepper;
            Modal = modal;
            Stepper.NextButton.clicked += () => { NameField.Value ??= NameField.PlaceholderText; };
            Stepper.AddStep(this.Q<VisualElement>("NameCategoryView"));
            Stepper.AddStep(this.Q<VisualElement>("StoryView"), OnShowStoryStep, OnHideStoryStep);
            
            CreateButton = Stepper.ConfirmButton;
            Stepper.ConfirmButton.SetEnabled(false);
            Stepper.ConfirmButton.clicked += OnCreateClicked;
        }

        protected override VisualElement CreatePreviewUI(VisualElement visualElement)
        {
            NodeConditionElement conditionElement = new NodeConditionElement(null);
            conditionElement.AddToClassList("ConditionPreview");
            
            ConditionInfo info = new ConditionInfo();
            info.Name = string.IsNullOrEmpty(NameValue) ? NameField.PlaceholderText : NameValue;
            info.StoryInfo.Story = Sentence.ToString();
            if (string.IsNullOrEmpty(info.Story))
            {
                return new VisualElement();
            }
            foreach (KeyValuePair<string, Type> variable in Sentence.GetStoryVariables())
            {
                info.StoryInfo.Variables.Add(new VariableInfo() { Name = variable.Key, Type = variable.Value });
            }
            
            conditionElement.InitFromConditionInfo(info);
            List<VisualElement> operatorFields = conditionElement.Children().Where(child => child is EnumLinkField<ConditionOperator> or EnumLinkField<BooleanOperator>).ToList();
            foreach (VisualElement field in operatorFields)
            {
                // Add a preview dropdown element instead of using the EnumLinkField.
                int index = conditionElement.IndexOf(field);
                field.RemoveFromHierarchy();
                conditionElement.Insert(index, new Dropdown());
            }
            conditionElement.pickingMode = PickingMode.Ignore;

            return conditionElement;
        }

        private void OnStoryFieldChanged(ChangingEvent<string> evt)
        {
           StoryField.Validate();
        }

        private void UpdateCreateButtonState()
        {
            bool allValid = NameField.ValidateWithoutNotify() && StoryField.ValidateWithoutNotify() && !string.IsNullOrEmpty(StoryField.Value);
            
            Stepper.ConfirmButton.SetEnabled(allValid);
        }

        protected override void OnCreateClicked()
        {
            ConditionGeneratorUtility.ConditionData data = new ConditionGeneratorUtility.ConditionData
            {
                Name = NameField.Value,
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown),
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables()
            };
            
            if (ConditionGeneratorUtility.CreateConditionAsset(data))
            {
                m_OnComplete(data.Name);
                Modal.Dismiss();
            }
        }

        protected override void SetupCategoryDropdown()
        {
            List<string> categories = GetConditionCategories();
            categories.Sort();
            categories.Insert(0, "Add custom category...");
            CategoryDropdown.bindItem = (item, i) => item.label = categories[i];
            CategoryDropdown.sourceItems = categories;
            CategoryDropdown.SetValueWithoutNotify(new []{ 1 });
            
            CategoryDropdown.RegisterValueChangedCallback( evt =>  {    
                if (evt.newValue.Last() == 0)
                {
                    if (CategoryField.value == null)
                    {
                        CategoryField.placeholder = "New Category";
                        CategoryField.value = CategoryField.placeholder;
                    }
                    CategoryField.Show();
                }
                else
                {
                    CategoryField.Hide();
                }
            });
        }

        private static List<string> GetConditionCategories()
        {
            List<string> categories = new List<string>();
            List<Condition> conditions = ConditionUtility.GetConditions();
            foreach (ConditionInfo info in conditions.Select(condition => ConditionUtility.GetInfoForConditionType(condition.GetType())).
                         Where(info => !categories.Contains(info.Category)))
            {
                categories.Add(info.Category);
            }

            return categories;
        }
    }
}
#endif