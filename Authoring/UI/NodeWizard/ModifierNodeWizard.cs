#if UNITY_EDITOR
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ModifierNodeWizard : BaseNodeWizard
    {
        NodeInfo m_Info;

        internal ModifierNodeWizard()
        {
            // If the user has not applied a value to the name field, use the placeholder value when moving forward in the stepper.
            NameField.PlaceholderText = "New Modifier";
        }

        protected override void SetHelpTexts()
        {
            SetInfoHelpText("Modifiers affect the execution of their connected subgraphs. For example, a repeat modifier causes its branch to perform its operation more than once.");
            SetStoryHelpText("Describe how the modifier affects the execution of the graph. For example: 'Repeat X times'.");
        }

        internal void SetupEditWizard(NodeInfo info)
        {
            CreateEditButton();
            m_Info = info;

            // Pre-fill fields for the modifier that is being edited
            NameField.Value = info.Name;
            CategoryField.value = info.Category;
            StoryField.Value = info.Story;
            FillPropertiesFromInfo(info);
        }

        internal override void SetupWizardStepperModal(WizardStepper stepper, Modal modal)
        {
            Stepper = stepper;
            Modal = modal;
            stepper.NextButton.clicked += () => { NameField.Value ??= NameField.PlaceholderText; };
            stepper.AddStep(this.Q<VisualElement>("NameCategoryView"));
            stepper.AddStep(this.Q<VisualElement>("StoryView"), OnShowStoryStep, OnHideStoryStep);
                  
            CreateButton = Stepper.ConfirmButton;
            CreateButton.SetEnabled(false);
            CreateButton.clicked += OnCreateClicked;
        }

        protected override void OnCreateClicked()
        {
            var data = new NodeGeneratorUtility.NodeData
            {
                NodeType = NodeGeneratorUtility.NodeType.Modifier,
                Name = NameValue,
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables(),
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown)
            };

            CreateNodeFromNodeData(data, "Modifier");
        }

        protected override void OnEditClicked()
        {
            bool variablesUnchanged = AreOldVariablesUnchanged(m_Info);
            bool userConfirmed = true;
            if (!variablesUnchanged)
            {
                userConfirmed = ShowEditConfirmDialog();
            }
            if (!userConfirmed)
            {
                return;
            }
            var data = new NodeGeneratorUtility.NodeData
            {
                NodeType = NodeGeneratorUtility.NodeType.Action,
                Name = NameValue,
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables(),
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown)
            };

            if (NodeGeneratorUtility.Edit(data, m_Info))
            {
                Modal.Dismiss();
            }
        }

        protected override VisualElement CreatePreviewUI(VisualElement nodeElement)
        {
            var nodeContent = new ModifierNodeUI(null);
            nodeContent.pickingMode = PickingMode.Ignore;
            nodeContent.InitFromNodeInfo(CreateNodeInfo());

            return nodeContent;
        }

        protected override void SetupCategoryDropdown()
        {
            base.SetupCategoryDropdown();
            SetDefaultCategory(k_FlowCategoryName, m_Info);
        }
    }
}
#endif