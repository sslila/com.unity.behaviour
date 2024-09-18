using System.IO;
using Unity.Behavior.GenerativeAI;
using UnityEngine;
using Unity.Behavior.GraphFramework;
using Button = Unity.AppUI.UI.Button;

#if UNITY_EDITOR
using System.Text;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ActionNodeWizard : BaseNodeWizard
    {
        NodeInfo m_Info;
        
        private TextArea m_GenAiDescriptionField;
        private Button m_GenerateWithAiButton;
        private Button m_CreateWithAiButton;
        private VisualElement m_GenAiRegion;

        public ActionNodeWizard()
        {
            // If the user has not applied a value to the name field, use the placeholder value when moving forward in the stepper.
            NameField.PlaceholderText = "New Action";
            AddToRequiredFields(StoryField.Field);
        }
        
        internal void SetupGenAiButton()
        {
            m_GenerateWithAiButton = new Button
            {
                title = "Use Generative AI"
            };
            m_GenerateWithAiButton.Hide();
            Stepper.Q<VisualElement>("WizardButtonContainer").Add(m_GenerateWithAiButton);
            m_GenerateWithAiButton.clicked += DisplayGenerateWithAILayout;
            MuseBehaviorUtilities.RegisterSessionStatusChangedCallback(OnSessionStatusChanged);
            if (!MuseBehaviorUtilities.IsSessionUsable)
            {
                m_GenerateWithAiButton.tooltip = MuseUtility.k_UserCallToAction;
            }
            m_GenerateWithAiButton.SetEnabled(MuseBehaviorUtilities.IsSessionUsable);
            Stepper.NextButton.clicked += ToggleGenAIButtonVisibility;
            Stepper.BackButton.clicked += ToggleGenAIButtonVisibility;
        }

        private void OnSessionStatusChanged(bool isUsable)
        {
            if (isUsable)
            {
                m_GenerateWithAiButton.tooltip = string.Empty;
            }
        }

        private void ToggleGenAIButtonVisibility()
        {
            if (CreateButton.style.display == DisplayStyle.Flex)
            {
#if ENABLE_MUSE_BEHAVIOR
                m_GenerateWithAiButton.Show();
#endif
            }
            else
            {
                m_GenerateWithAiButton.Hide();
            }
            
            // Remove Generative AI element from steps when going back from it, and display creation option buttons again.
            if (Stepper.ContainsStep(m_GenAiRegion))
            {
                Stepper.RemoveStep(m_GenAiRegion);
                Stepper.NextButton.Hide();
                Stepper.ConfirmButton.Show();
#if ENABLE_MUSE_BEHAVIOR
                m_GenerateWithAiButton.Show();
#endif
            }
        }

        internal void SetupGenAiLayout()
        {
            var fieldRegion = this.Q<VisualElement>("FieldRegion");
            var genAiLayout = ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/GenerativeAI/Assets/GenAiWizardLayout.uxml").CloneTree();
            genAiLayout.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/BaseNodeWizardStyles.uss"));
            fieldRegion.Add(genAiLayout);
            
            m_GenAiDescriptionField = genAiLayout.Q<TextArea>("GenAiDescriptionField");
            m_GenAiDescriptionField.RegisterValueChangingCallback(_ => Validate());

            m_GenAiRegion = this.Q<VisualElement>("GenAiRegion");
            m_GenAiRegion.Hide();

            HelpText gptHelpBox = genAiLayout.Q<HelpText>("GenAiHelpBox");
            gptHelpBox.Text =
                "Describe how the node should work. Try to include what should happen in OnStart(), OnUpdate(), and OnEnd().";

            m_CreateWithAiButton = m_GenAiRegion.Q<Button>("GenAiCreateButton");
            m_CreateWithAiButton.clicked += OnGenAiCreateClicked;
        }

        private void DisplayGenerateWithAILayout()
        {
            if (!MuseBehaviorUtilities.IsSessionUsable)
            {
                MuseBehaviorUtilities.OpenMuseDropdown();
                return;
            }
            Stepper.AddStep(this.Q<VisualElement>("GenAiRegion"), OnShowStoryStep);
            Stepper.OnNextButtonClicked();
            Stepper.ConfirmButton.Hide();
            m_GenerateWithAiButton.Hide();
        }

        protected override void SetHelpTexts()
        {
            SetInfoHelpText("Actions describe and contain agent behaviour logic, for example an agent attacking an enemy agent.");
            SetStoryHelpText("Describe your action. For example: 'Agent attacks Target'.");
        }

        internal void SetupEditWizard(NodeInfo info)
        {
            CreateEditButton();
            m_Info = info;

            // Pre-fill fields for the action that is being edited
            NameField.Value = info.Name;
            CategoryField.value = info.Category;
            StoryField.Value = info.Story;
            FillPropertiesFromInfo(info);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Validate Generative AI buttons if the Generative AI step is used.
            if (m_GenAiRegion != null)
            {
                MuseBehaviorUtilities.RegisterSessionStatusChangedCallback(_ => EnableGenerateWithAiButton());
                m_GenerateWithAiButton?.SetEnabled(IsAllowedToCreate());
                m_CreateWithAiButton.SetEnabled(IsAllowedToCreate() && !string.IsNullOrEmpty(m_GenAiDescriptionField?.value));
            }
        }

        private void EnableGenerateWithAiButton()
        {
            m_GenerateWithAiButton?.SetEnabled(IsAllowedToCreate());
        }

        internal override void SetupWizardStepperModal(WizardStepper stepper, Modal modal)
        {
            Stepper = stepper;
            Modal = modal;
            Stepper.NextButton.clicked += () => { NameField.Value ??= NameField.PlaceholderText; };
            Stepper.AddStep(this.Q<VisualElement>("NameCategoryView"), OnShowNameCategoryStep);
            Stepper.AddStep(this.Q<VisualElement>("StoryView"), OnShowStoryStep, OnHideStoryStep);
            
            CreateButton = Stepper.ConfirmButton;
            CreateButton.SetEnabled(false);
            CreateButton.clicked += OnCreateClicked;
        }

        private void OnShowNameCategoryStep()
        {
            NameField.Focus();
        }

        protected override void OnCreateClicked()
        {
            var data = new NodeGeneratorUtility.NodeData
            {
                NodeType = NodeGeneratorUtility.NodeType.Action,
                Name = NameValue,
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables(),
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown)
            };

            CreateNodeFromNodeData(data, "Action");
        }

        private void OnGenAiCreateClicked()
        {
#if ENABLE_MUSE_BEHAVIOR
            string fileName = GeneratorUtils.ToPascalCase(NameValue);

            string path = EditorUtility.SaveFilePanel(
                $"Create {NodeGeneratorUtility.GetNodeTypeString(NodeGeneratorUtility.NodeType.Action)} Node Script",
                Application.dataPath,
                fileName,
                "cs");

            if (path.Length == 0)
            {
                return;
            }

            var data = new NodeGeneratorUtility.NodeData
            {
                NodeType = NodeGeneratorUtility.NodeType.Action,
                Name = NameValue,
                ClassName = Path.GetFileNameWithoutExtension(path),
                Story = Sentence.ToString(),
                Variables = Sentence.GetStoryVariables(),
                Category = WizardUtils.GetCategoryFieldValue(CategoryField, CategoryDropdown)
            };

            var classString = NodeGeneratorUtility.MakeClassString(data);
            var prompt = WrapPrompt(classString);
            var aiModel = new MuseChatModel();
            aiModel.Chat(prompt, response =>
            {
                string output = response.output;
                //just in case the LLM removes this namespace, we add it back
                if (!output.Contains("using Action = Unity.Behavior.Action;"))
                {
                    output = output.Insert(0,"using Action = Unity.Behavior.Action;\n");
                }
                
                string date = System.DateTime.Now.ToString("yyyy-MM-dd");
                string legalHeader = $"// {date} AI-Tag\n" +
                                  "// This was created with assistance from Muse, a Unity Artificial Intelligence product.\n";
                output = output.Insert(0,legalHeader);

                // A problem started popping up when we switch backend LLM which is causing markdown tags to appear 
                // when they should not. This line sanitizes the output to remove them. This should stop some failures
                // when the AI does not generate the expected output
                output = SanitizeOutput(output);
                
                using (StreamWriter outfile = new StreamWriter(path))
                {
                    outfile.Write(output);
                }

                OnNodeTypeCreated?.Invoke(data);
                AssetDatabase.Refresh();
                string relativePath = path.StartsWith(Application.dataPath) ? ("Assets" + path.Substring(Application.dataPath.Length)) : path;
                MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePath, typeof(MonoScript));
                AssetDatabase.OpenAsset(script);
                MuseBehaviorUtilities.UpdateUsage();
            }, "code-generation");
#endif
        }

        string SanitizeOutput(string output)
        {
            StringBuilder sb = new();
            
            using (StringReader reader = new(output))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("```"))
                        continue;
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
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

        private string WrapPrompt(string template)
        {
            var prompt = ResourceLoadAPI.Load<TextAsset>("Packages/com.unity.behavior/Authoring/GenerativeAI/Assets/Prompts/ActionPrompt.txt");
            var promptText = prompt.text;
            promptText = promptText.Replace("{template}", template);
            promptText = promptText.Replace("{description}", m_GenAiDescriptionField.value);
            return promptText;
        }

        protected override VisualElement CreatePreviewUI(VisualElement nodeElement)
        {
            var nodeContent = new ActionNodeUI(null);
            nodeContent.pickingMode = PickingMode.Ignore;
            nodeContent.InitFromNodeInfo(CreateNodeInfo());

            return nodeContent;
        }

        protected override void SetupCategoryDropdown()
        {
            base.SetupCategoryDropdown();
            SetDefaultCategory(k_ActionCategoryName, m_Info);
        }
    }
}
#endif