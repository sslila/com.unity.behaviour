#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Behavior
{
    internal class SequencingNodeWizard : BaseNodeWizard
    {
        internal Button AddPortButton => m_CustomPortsRegion.AddButton;
        private NodeInfo m_Info;

        private const string k_CustomPortsHelpText = "Add output ports for your new node. You can customize the ports by adding a name.";
        private const string k_PortsPreviewElementName = "Ports-Preview";

        private readonly HelpText m_CustomPortHelpBox;
        private readonly EditableListRegion m_CustomPortsRegion;
        private const string k_PortPlaceholderName = "Port";

        private readonly List<string> m_CustomPortNames = new();
        private List<string> m_OldPortNames = new();
        private VisualElement m_PortsPreviewElement = new VisualElement() { name = k_PortsPreviewElementName };
        private bool m_ShowPortsPreview = false;

        internal SequencingNodeWizard()
        {
            NameField.PlaceholderText = "New Sequencing";

            // Set up a custom region for port creation.
            m_CustomPortsRegion = new EditableListRegion(m_CustomPortNames);
            m_CustomPortsRegion.AddButton.title = "Add " + k_PortPlaceholderName;
            m_CustomPortsRegion.FieldPlaceholderName = k_PortPlaceholderName;
            m_CustomPortsRegion.Hide();
            Add(m_CustomPortsRegion);

            m_CustomPortsRegion.OnListUpdated += () =>
            {
                RefreshNodePreviewUI();
                Validate();
            };

            m_CustomPortHelpBox = new HelpText(k_CustomPortsHelpText);
            m_CustomPortHelpBox.AddToClassList("HelpText");
            m_CustomPortsRegion.Insert(0, m_CustomPortHelpBox);
            
            // Move confirm button container to last step.
            m_CustomPortsRegion.Add(this.Q<VisualElement>("ButtonContainer"));
        }

        private void OnShowStoryViewAndPorts()
        {
            m_ShowPortsPreview = true;
            Stepper.PreviewElement.Show();
            RefreshNodePreviewUI();
        }

        private void OnHideStoryViewAndPorts()
        {
            m_ShowPortsPreview = false;
            Stepper.PreviewElement.Hide();
        }

        protected override void SetHelpTexts()
        {
            SetInfoHelpText("Sequencing nodes control the execution flow of their connected branches, imposing both an order of execution as well as conditions for a sequence's completion.");
            SetStoryHelpText("Describe your sequencing node. For example: 'Execute children until one succeeds or all fail'.");
        }

        internal void SetupEditWizard(NodeInfo info, NodeModel model)
        {
            CreateEditButton();
            m_Info = info;

            // Pre-fill fields for the sequencing that is being edited
            NameField.Value = info.Name;
            CategoryField.value = info.Category;
            StoryField.Value = info.Story;
            FillPropertiesFromInfo(info);
            foreach (PortModel portModel in model.OutputPortModels)
            {
                m_OldPortNames.Add(portModel.Name);
            }

            FillCustomPortsFromOutputPorts(m_OldPortNames);
        }

        internal void SetupCustomPorts(List<string> portNames)
        {
            foreach (var portName in portNames)
            {
                m_CustomPortNames.Add(portName);
            }
            m_CustomPortsRegion.UpdateList();
        }

        private void FillCustomPortsFromOutputPorts(List<string> portNames)
        {
            foreach (var portName in portNames)
            {
                m_CustomPortNames.Add(portName);
            }

            m_CustomPortsRegion.UpdateList();
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
                NodeType = NodeGeneratorUtility.NodeType.Composite,
                Name = NameValue,
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables(),
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown),
                Ports = m_CustomPortNames,
                OldPorts = m_OldPortNames
            };

            if (NodeGeneratorUtility.Edit(data, m_Info))
            {
                Modal.Dismiss();
            }
        }

        internal override void SetupWizardStepperModal(WizardStepper stepper, Modal modal)
        {
            Stepper = stepper;
            Modal = modal;
            stepper.NextButton.clicked += () => { NameField.Value ??= NameField.PlaceholderText; };
            stepper.AddStep(this.Q<VisualElement>("NameCategoryView"));
            stepper.AddStep(this.Q<VisualElement>("StoryView"), OnShowStoryStep, OnHideStoryStep);
            stepper.AddStep(m_CustomPortsRegion, OnShowStoryViewAndPorts, OnHideStoryViewAndPorts);
            
            CreateButton = Stepper.ConfirmButton;
            CreateButton.SetEnabled(false);
            CreateButton.clicked += OnCreateClicked;
        }

        protected override void OnCreateClicked()
        {
            var data = new NodeGeneratorUtility.NodeData
            {
                NodeType = NodeGeneratorUtility.NodeType.Composite,
                Name = NameValue,
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables(),
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown),
                Ports = m_CustomPortNames
            };

            CreateNodeFromNodeData(data, "Sequence");
        }

        protected override bool IsValid()
        {
            List<TextFieldWithValidation> textFields = m_CustomPortsRegion.Query<TextFieldWithValidation>().ToList();

            return !m_CustomPortsRegion.IncludesDuplicates() && textFields.All(textField => textField.IsValid);
        }
        
        protected override bool HasDuplicateVariables()
        {
            List<string> variableNames = m_CustomPortNames.Select(GeneratorUtils.RemoveSpaces).ToList();
            variableNames.AddRange(Sentence.WordTypeParameters.Select(parameter => GeneratorUtils.RemoveSpaces(parameter.Word)));

            return variableNames.Count != variableNames.Distinct().Count();
        }

        protected override bool HasTypeNameInVariables()
        {
            List<string> portNamesWithoutSpaces = m_CustomPortNames.Select(GeneratorUtils.RemoveSpaces).ToList();
            return base.HasTypeNameInVariables() || portNamesWithoutSpaces.Contains(GeneratorUtils.RemoveSpaces(NameField.Value));
        }

        protected override VisualElement CreatePreviewUI(VisualElement nodeElement)
        {
            VisualElement previewContent = new VisualElement() { name = "Composite-Preview" };

            var nodePreview = new CompositeNodeUI(null);
            nodePreview.pickingMode = PickingMode.Ignore;
            nodePreview.style.position = Position.Relative;
            nodePreview.style.alignSelf = Align.Center;
            nodePreview.InitFromNodeInfo(CreateNodeInfo());
            previewContent.Add(nodePreview);

            if (nodePreview.ClassListContains("TwoLineNode"))
            {
                previewContent.AddToClassList("Composite-Preview__TwoLineNode");
            }

            if (!m_ShowPortsPreview || m_CustomPortNames.Count == 0)
            {
                return previewContent;
            }

            Port outputPort = new Port(new PortModel("Output", PortDataFlowType.Output)) { Style = PortStyle.Edge };
            outputPort.pickingMode = PickingMode.Ignore;
            nodePreview.OutputPortsContainer.Add(outputPort);

            previewContent.Add(m_PortsPreviewElement);
            m_PortsPreviewElement.Clear();
            previewContent.EnableInClassList("Composite-Preview__HasPorts", true);


            int i = 0;
            foreach (var portName in m_CustomPortNames)
            {
                var portUI = new FloatingPortNodeUI(null) { Title = portName };
                portUI.pickingMode = PickingMode.Ignore;
                portUI.style.position = Position.Relative;
                portUI.style.alignSelf = Align.Center;
                portUI.pickingMode = PickingMode.Ignore;

                Port port = new Port(new PortModel("Input", PortDataFlowType.Input)) { Style = PortStyle.Edge };
                portUI.InputPortsContainer.Add(port);
                port.pickingMode = PickingMode.Ignore;

                Edge edge = new Edge() { Start = outputPort, End = port };
                edge.pickingMode = PickingMode.Ignore;
                m_PortsPreviewElement.Add(edge);

                m_PortsPreviewElement.Add(portUI);
                if (i != 0)
                {
                    portUI.style.marginLeft = 20;
                }
                ++i;
            }

            return previewContent;
        }
        
        protected override void SetupCategoryDropdown()
        {
            base.SetupCategoryDropdown();
            SetDefaultCategory(k_FlowCategoryName, m_Info);
        }
    }
}
#endif