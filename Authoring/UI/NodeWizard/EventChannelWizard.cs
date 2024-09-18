#if UNITY_EDITOR
using System;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class EventChannelWizard : BaseNodeWizard
    {
        private const int k_MaxParameters = 4;
        private const string k_PropertiesErrorText = "Maximum number of parameters is four. Try reducing the amount of non-text parameters.";
        private const float k_WizardWidth = 316f;

        internal delegate void OnEventChannelTypeCreatedCallback(EventChannelGeneratorUtility.EventChannelData eventChannelData);
        internal OnEventChannelTypeCreatedCallback OnEventChannelTypeCreated;

        internal EventChannelWizard()
        {
            // If the user has not applied a value to the name field, use the placeholder value when moving forward in the stepper.
            NameField.PlaceholderText = "New Event Channel";
            
            NameField.RegisterValueChangingCallback(OnNameChanged);
            AddToRequiredFields(StoryField.Field);
            
            StoryField.m_CustomValidationMethods.Add(() =>
            {
                if (!IsValid())
                {
                    StoryField.tooltip = k_PropertiesErrorText;
                    return false;
                }

                return true;
            });
        }

        protected override void SetHelpTexts()
        {
            SetInfoHelpText("Event channels can be used to send and receive messages between nodes, including nodes in other behaviour graphs. For example, an event channel can be used to send a message when an agent spots an enemy.");
            SetStoryHelpText("Describe a message that will be sent with the event. For example: 'Agent has spotted Enemy'.");
        }

        private void OnNameChanged(ChangingEvent<string> evt)
        {
            EnsureRegionVisibility();
        }

        protected override bool ShowDependentRegions() =>
            base.ShowDependentRegions() || !string.IsNullOrWhiteSpace(NameField.Value);


        protected override void OnEditClicked()
        {
        }

        protected override VisualElement CreatePreviewUI(VisualElement nodeElement)
        {
            nodeElement.AddToClassList("Action");
            
            var message = Sentence.ToString();
            Type[] messageFieldTypes = Sentence.WordTypeParameters.Select(p => p.Type).ToArray();

            var nodePreviewUI = new VisualElement();
            nodePreviewUI.pickingMode = PickingMode.Ignore;
            nodePreviewUI.name = "Event-Preview";

            CreateNodePreview(typeof(TriggerEventAction));
            CreateNodePreview(typeof(WaitForEventAction));

            return nodePreviewUI;

            VisualElement CreateNodePreview(Type type)
            {
                EventActionNodeUI eventnodeUI = new EventActionNodeUI(null);
                eventnodeUI.InitFromInfo(type, message, messageFieldTypes);

                eventnodeUI.style.position = Position.Relative;

                nodePreviewUI.Add(eventnodeUI);
                return eventnodeUI;
            }
        }

        internal override void SetupWizardStepperModal(WizardStepper stepper, Modal modal)
        {
            Stepper = stepper;
            Modal = modal;
            Stepper.NextButton.clicked += () => { NameField.Value ??= NameField.PlaceholderText; };
            Stepper.AddStep(this.Q<VisualElement>("NameCategoryView"));
            Stepper.AddStep(this.Q<VisualElement>("StoryView"), OnShowStoryStep, OnHideStoryStep);

            Stepper.StepperContainer.style.width = k_WizardWidth;
            
            CreateButton = Stepper.ConfirmButton;
            CreateButton.SetEnabled(false);
            CreateButton.clicked += OnCreateClicked;
        }

        protected override void OnCreateClicked()
        {
            var data = new EventChannelGeneratorUtility.EventChannelData
            {
                Name = NameValue,
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown),
                Parameters = Sentence.WordTypeParameters.Select(p => (p.Word, p.Type)).ToArray(),
                Message = Sentence.ToString()
            };

            if (EventChannelGeneratorUtility.CreateEventChannelAsset(data))
            {
                OnEventChannelTypeCreated?.Invoke(data);
                Modal.Dismiss();
            }
        }

        protected override bool IsValid()
        {
            return Sentence.WordTypeParameters.Count() <= k_MaxParameters;
        }
        
        protected override void SetupCategoryDropdown()
        {
            base.SetupCategoryDropdown();
            CategoryDropdown.SetValueWithoutNotify(new []{ CategoryDropdown.sourceItems.IndexOf("Events") });
        }
    }
}
#endif