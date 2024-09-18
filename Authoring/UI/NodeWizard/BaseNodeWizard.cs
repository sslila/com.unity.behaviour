#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;
using Button = Unity.AppUI.UI.Button;
using Unity.Behavior.GraphFramework;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace Unity.Behavior
{
    internal abstract class BaseNodeWizard : VisualElement
    {
        private const string k_InfoHelpBoxName = "InfoHelpBox";
        private const string k_StoryHelpBoxName = "StoryHelpBox";
        private const string k_PropertiesHelpBoxName = "PropertiesHelpBox";
        private const string k_PropertiesRegionName = "PropertiesRegion";
        private const string k_StoryFieldName = "StoryField";
        private const string k_PropertiesContainerName = "PropertiesContainer";
        private const string k_NameFieldName = "NameField";
        private const string k_CategoryFieldName = "CategoryField";
        private const string k_CategoryDropdownName = "CategoryDropdown";

        internal const string k_ActionCategoryName = "Action";
        internal const string k_FlowCategoryName = "Flow";
        private const string k_EventCategoryName = "Events";
        
        private readonly List<INotifyValueChanging<string>> m_RequiredFields = new ();
        internal Button CreateButton;

        internal const string k_TypeNameErrorText = "Variable can not share name with the asset. Try renaming either the name or a variable.";
        private const string k_DuplicateVariablesErrorText = "Variable names need to be unique, try renaming a word with a variable set to it.";
        

        protected TextFieldWithValidation NameField { get; }
        protected TextField CategoryField { get; }
        protected Dropdown CategoryDropdown { get; }
        
        protected internal StoryEditor StoryEditor { get; }
        protected StoryFieldWithValidation StoryField { get; }
        
        protected VisualElement PreviewRegion { get; }

        internal WordTypeSentence Sentence { get; set; }

        protected string NameValue => NameField.Value;

        internal WizardStepper Stepper;
        internal Modal Modal;

        internal delegate void OnNodeTypeCreatedCallback(NodeGeneratorUtility.NodeData nodeData);
        internal OnNodeTypeCreatedCallback OnNodeTypeCreated;
        
        internal delegate void OnConditionTypeCreatedCallback(ConditionGeneratorUtility.ConditionData conditionData);
        internal OnConditionTypeCreatedCallback OnConditionTypeCreated;
        protected BaseNodeWizard()
        {
            Sentence = new WordTypeSentence();

            AddToClassList("BaseNodeWizard");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/BaseNodeWizardStyles.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/BaseNodeWizardLayout.uxml").CloneTree(this);

            NameField = this.Q<TextFieldWithValidation>(k_NameFieldName);
            AddToRequiredFields(NameField.Field);

            CategoryField = this.Q<TextField>(k_CategoryFieldName);
            CategoryField.Hide();
            CategoryDropdown = this.Q<Dropdown>(k_CategoryDropdownName);

            StoryEditor = this.Q<StoryEditor>();
            
            // Refresh node preview and validate when properties are changed in the story editor.
            StoryEditor.OnPropertyValueChanged += () =>
            {
                RefreshNodePreviewUI();
                Validate();
            };
            
            StoryField = this.Q<StoryFieldWithValidation>(k_StoryFieldName);
            StoryField.RegisterValueChangingCallback(OnStoryFieldChanged);
            Sentence = StoryEditor.Sentence;
            StoryField.Sentence = Sentence;

            PreviewRegion = new VisualElement();
            PreviewRegion.name = "PreviewRegion";
            PreviewRegion.AddToClassList("Region");
            VisualElement nodePreviewContainer = new VisualElement();
            nodePreviewContainer.name = "NodeContainer";
            
            SetupStoryFieldCustomValidations();
            
            RegisterCallback<AttachToPanelEvent>(OnWizardAttachedToPanel);
        }

        internal void SetupSuggestedNodeProperties(string nodeName, string category)
        {
            NameField.Value = nodeName;
            CategoryField.value = category;
        }

        private void OnWizardAttachedToPanel(AttachToPanelEvent evt)
        {
            SetupCategoryDropdown();
            SetHelpTexts();
        }

        protected abstract void SetHelpTexts();

        internal abstract void SetupWizardStepperModal(WizardStepper stepper, Modal modal);
        
        /// <summary>
        /// Handler for when the 'Create' button is clicked
        /// </summary>
        protected abstract void OnCreateClicked();

        /// <summary>
        /// Runs all validation rules (base wizard and custom rules added by implementing IsValid()
        /// </summary>
        internal void Validate()
        {
            List<InputFieldWithValidation> validationFields = this.Query<InputFieldWithValidation>().ToList();
            foreach (InputFieldWithValidation validationTextField in validationFields)
            {
                validationTextField.Validate();
            }
            CreateButton.SetEnabled(IsAllowedToCreate());
            OnValidate();
        }

        protected bool IsAllowedToCreate()
        {
            return !IsRequiredFieldMissing() & IsValid() & NameField.IsValid & StoryField.IsValid;
        }

        protected virtual void OnValidate() { }

        /// <summary>
        /// Use to add custom validation logic to your wizard.  
        /// </summary>
        /// <returns>'true' if the validation succeeds, 'false' otherwise.</returns>
        protected virtual bool IsValid() => true;


        /// <summary>
        /// Clears the NodeContainer and re-creates its content based on the current sentence.
        /// </summary>
        protected void RefreshNodePreviewUI()
        {
            var stepperPreview = Stepper.PreviewElement;
            stepperPreview.Clear();

            if (!stepperPreview.IsVisible())
            {
                return;
            }

            if (!HasDuplicateVariables())
            {
                VisualElement preview = CreatePreviewUI();
                preview.SetEnabled(false);
                stepperPreview.Add(preview);
            }
        }

        /// <summary>
        /// Creates and returns a new preview UI for the node.
        /// </summary>
        /// <returns>The newly created preview UI.</returns>
        private VisualElement CreatePreviewUI()
        {
            var previewUI = new VisualElement();
            previewUI.AddToClassList("NodePreview");
            previewUI.Add(CreatePreviewUI(previewUI));
            return previewUI;
        }

        protected void CreateEditButton()
        {
            CreateButton.title = "Confirm Edit";
            CreateButton.clicked -= OnCreateClicked;
            CreateButton.clicked += OnEditClicked;
        }

        protected virtual void OnEditClicked()
        {
            throw new NotImplementedException();
        }

        protected abstract VisualElement CreatePreviewUI(VisualElement visualElement);
        
        protected void OnShowStoryStep()
        {
            Stepper.PreviewElement.Show();
            NameField.Value ??= NameField.PlaceholderText;
            UpdateStory();
            StoryField.Focus();
        }

        protected void OnHideStoryStep()
        {
            Stepper.PreviewElement.Hide();
        }

        /// <summary>
        /// creates a UI preview for each word-type pair in a sentence based on the type.
        /// </summary>
        /// <returns>a field for each word-type pair in the current sentence.</returns>
        protected IEnumerable<VisualElement> CreateSentencePreview()
        {
            foreach (var wordTypePair in Sentence.WordTypePairs)
            {
                string word = wordTypePair.Word;
                Type type = wordTypePair.Type;

                VisualElement field = type == typeof(RegularText)
                    ? new Label(word)
                    : LinkFieldUtility.CreateNodeLinkField(word, type);
                field.SetEnabled(false);
                yield return field;
            }
        }

        protected NodeInfo CreateNodeInfo()
        {
            var nodeInfo = new NodeInfo();
            nodeInfo.Name = string.IsNullOrEmpty(NameValue) ? NameField.PlaceholderText : NameValue;
            nodeInfo.StoryInfo.Story = Sentence.ToString();
            foreach (var variable in Sentence.GetStoryVariables())
            {
                nodeInfo.StoryInfo.Variables.Add(new VariableInfo() { Name = variable.Key, Type = variable.Value });
            }
            return nodeInfo;
        }

        protected void SetInfoHelpText(string helpText) => SetHelpText(k_InfoHelpBoxName, helpText);

        protected void SetStoryHelpText(string helpText)
        {
            SetHelpText(k_StoryHelpBoxName, helpText);
        }

        protected void SetPropertiesHelpText(string helpText) => SetHelpText(k_PropertiesHelpBoxName, helpText);

        protected virtual void EnsureRegionVisibility()
        {
            var isVisible = ShowDependentRegions();
            this.Q<VisualElement>(k_PropertiesRegionName).SetVisible(isVisible);
        }

        protected virtual bool ShowDependentRegions() => !string.IsNullOrWhiteSpace(StoryField.Value);

        private void SetHelpText(string helpBoxName, string helpText)
        {
            HelpText helpBox = this.Q<HelpText>(helpBoxName);
            if (helpBox != null)
            {
                helpBox.Text = helpText;   
            }
        }

        private void OnStoryFieldChanged(ChangingEvent<string> evt)
        {
            if (evt.target is not StoryFieldWithValidation { name: k_StoryFieldName } storyField)
            {
                return;
            }

            EnsureRegionVisibility();
            UpdateStory();
        }

        protected void UpdateStory()
        {
            StoryEditor.RefreshPropertiesUI();
            RefreshNodePreviewUI();
            Validate();
        }

        protected void AddToRequiredFields(INotifyValueChanging<string> field)
        {
            m_RequiredFields.Add(field);
            field.RegisterValueChangingCallback(_ => Validate());
        }

        private bool IsRequiredFieldMissing()
        {
            return m_RequiredFields.Any(field => string.IsNullOrWhiteSpace(field.value));
        }

        protected virtual bool HasDuplicateVariables()
        {
            return Sentence.WordTypeParameters.Select(p => ObjectNames.NicifyVariableName(p.Word)).Distinct().Count() != Sentence.WordTypeParameters.Count();
        }

        protected virtual bool HasTypeNameInVariables()
        {
            return Sentence.WordTypeParameters.Any(parameter => parameter.Word == GeneratorUtils.RemoveSpaces(NameField.Value));
        }

        private void SetupStoryFieldCustomValidations()
        {
            StoryField.m_CustomValidationMethods.Add(() =>
            {
                if (!HasTypeNameInVariables())
                {
                    return true;
                }

                StoryField.tooltip = k_TypeNameErrorText;
                return false;
            });

            if (StoryField != null)
            {
                StoryField.m_CustomValidationMethods.Add(() =>
                {
                    if (!HasDuplicateVariables())
                    {
                        ClearDuplicateFieldHighlights();
                        return true;
                    }
                    
                    HighlightDuplicateFields();
                    StoryField.tooltip = k_DuplicateVariablesErrorText;
                    return false;
                });   
            }
        }

        private void ClearDuplicateFieldHighlights()
        {
            foreach (VisualElement element in this.Q<VisualElement>(k_PropertiesContainerName).Children().ToList())
            {
                element.RemoveFromClassList("Invalid");
            }
        }

        private void HighlightDuplicateFields()
        {
            List<WordTypePair> pairs = Sentence.WordTypePairs.Select(wordPair => new WordTypePair(ObjectNames.NicifyVariableName(wordPair.Word), wordPair.Type)).ToList();
            var duplicateEntries = pairs
                .Select((pair, index) => new { Index = index, Entry = pair })
                .Where(item => item.Entry.Type != typeof(RegularText))
                .GroupBy(item => item.Entry.Word)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group.Select(item => new { Index = item.Index, Word = item.Entry.Word, Type = item.Entry.Type }));

            // Add highlight class on all property fields that have been marked as duplicates.
            List<VisualElement> fields = this.Q<VisualElement>(k_PropertiesContainerName).Children().ToList();
            var duplicates = duplicateEntries.ToList();
            foreach (var duplicate in duplicates.Where(duplicate => fields.Count > duplicate.Index && duplicate.Type != typeof(RegularText)))
            {
                fields[duplicate.Index].AddToClassList("Invalid");
            }
            
            // Remove the class from of the property fields that are not duplicates.
            for (int i = 0; i < fields.Count; i++)
            {
                if (duplicates.All(item => item.Index != i))
                {
                    fields[i].RemoveFromClassList("Invalid");
                }
            }
        }
        
        protected virtual void SetupCategoryDropdown()
        {
            List<string> categories = NodeRegistry.Instance.NodeCategories.ToList();
            AddCategoryItem(k_ActionCategoryName, categories);
            AddCategoryItem(k_FlowCategoryName, categories);
            AddCategoryItem(k_EventCategoryName, categories);
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

        private void AddCategoryItem(string categoryName, List<string> categoryList)
        {
            if (!categoryList.Contains(categoryName))
            {
                categoryList.Add(categoryName);
            }
        }
        
        internal void FillPropertiesFromInfo(NodeInfo info)
        {
            var indexes = new List<int>();
            var words = info.Story.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (Regex.IsMatch(words[i], @"^\[.*?\]$"))
                {
                    indexes.Add(i);
                }
            }
            StoryField.Value = StoryField.Value.Replace("[", string.Empty).Replace("]", string.Empty);
            Sentence.UpdateWordTypeList(StoryField.Q<UnityEngine.UIElements.TextField>().cursorIndex, StoryField.Value);
            if (info.Variables != null)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    Type type = info.Variables[i].Type;
                    Type genericType = type.GetGenericArguments()?[0];
                    Sentence.WordTypePairs[indexes[i]].Type = genericType ?? type;
                }
            }
            EnsureRegionVisibility();
            UpdateStory();
        }

        internal bool AreOldVariablesUnchanged(NodeInfo info)
        {
            // Check to see if all of the old variables are present and maintain their original types.
            Dictionary<string, Type> oldVariables = new ();
            Dictionary<string, Type> newVariables = Sentence.GetStoryVariables();
            if (info.Variables != null)
            {
                foreach (var variable in info.Variables)
                {
                    string storyVariableName = "[" + variable.Name + "]";
                    if (!info.Story.Contains(storyVariableName))
                    {
                        continue;
                    }

                    Type type = variable.Type;
                    Type genericType = type.GetGenericArguments()?[0];
                    oldVariables.Add(variable.Name, genericType);
                }
            }

            foreach ((string oldVariableName, Type oldVariableType) in oldVariables)
            {
                if (!newVariables.ContainsKey(oldVariableName) || newVariables[oldVariableName] != oldVariableType)
                {
                    return false;
                }
            }
            return true;
        }

        internal bool ShowEditConfirmDialog()
        {
            return EditorUtility.DisplayDialog("Variables missing or mismatch in type", "Variables are missing or the type of a variable has changed, which may cause compilation errors on the generated node script. Are you sure you want to confirm your changes?", "Yes", "No");
        }
        
        internal void SetVariableSuggestions(Dictionary<string, Type> variableSuggestions)
        {
            Sentence.AddSuggestions(variableSuggestions);
        }
        
        internal void SetStoryField(string story)
        {
            story = story.Replace("[", string.Empty).Replace("]", string.Empty);
            StoryField.Value = story;
            StoryEditor.UpdateWordTypeList();
        }

        internal void SetDefaultCategory(string defaultCategory, NodeInfo nodeInfo)
        {
            if (nodeInfo == null)
            {
                CategoryDropdown.SetValueWithoutNotify(new []{ CategoryDropdown.sourceItems.IndexOf(defaultCategory) });
                return;
            } 
            CategoryDropdown.SetValueWithoutNotify(new []{ CategoryDropdown.sourceItems.IndexOf(nodeInfo.Category) });
        }

        protected void CreateNodeFromNodeData(NodeGeneratorUtility.NodeData nodeData, string postfix)
        {
            if (NodeGeneratorUtility.Create(nodeData, postfix))
            {
                OnNodeTypeCreated?.Invoke(nodeData);
                Modal.Dismiss();
            }
        }

        protected void CreateConditionFromConditionData(ConditionGeneratorUtility.ConditionData conditionData,
            string postfix)
        {
            OnConditionTypeCreated?.Invoke(conditionData);
        }
        internal virtual void OnShow()
        {
            schedule.Execute(NameField.Focus);
        }
    }
}
#endif