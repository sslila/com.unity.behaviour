#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.Behavior
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class InputFieldWithValidation : ExVisualElement, INotifyValueChanging<string>
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<InputFieldWithValidation, UxmlTraits> {}
#endif
        internal INotifyValueChanging<string> Field;
        
        internal event System.Action OnItemValidation;
        internal delegate bool ValidationDelegate();
        internal List<ValidationDelegate> m_CustomValidationMethods = new List<ValidationDelegate>();

        internal bool IsValid = true;

        internal string Value
        {
            get => Field.value;
            set => Field.value = value;
        }
        public string value
        {
            get => Value;
            set => Value = value;
        }

        internal bool ValidateWithoutNotify()
        {
            IsValid = ValidateFieldForInvalidCharactersOrEmpty();
            if (IsValid)
            {
                // If field text content was valid, run custom validations.
                foreach (ValidationDelegate validationMethod in m_CustomValidationMethods)
                {
                    IsValid = validationMethod();
                    if (!IsValid)
                    {
                        break;
                    }
                }
            }
            
            if (Field is IInputElement<string> inputElement)
            {
                inputElement.invalid = !IsValid;
            }
            
            return IsValid;
        }

        protected virtual bool ValidateFieldForInvalidCharactersOrEmpty()
        {
            if (Field.value == null)
            {
                return false;
            }
            bool isValid = Field.value.Replace(" ", string.Empty).IsValidIdentifier();
            if (Field is VisualElement element)
            {
                element.tooltip = isValid ? null : Util.InvalidIdentifierErrorMessage;  
            }

            return isValid;
        }

        protected void AddFieldToValidate(INotifyValueChanging<string> field)
        {
            if (field is VisualElement element)
            {
                Add(element);   
            }
            Field = field;
            Field.RegisterValueChangingCallback(Validate);
        }

        public void Validate(ChangingEvent<string> evt = null)
        {
            ValidateWithoutNotify();
            OnItemValidation?.Invoke();

            if (evt == null)
            {
                return;
            }
            evt.StopPropagation();
            ChangingEvent<string> newChangingEvent = ChangingEvent<string>.GetPooled();
            newChangingEvent.previousValue = evt?.previousValue;
            newChangingEvent.newValue = evt?.newValue;
            newChangingEvent.target = this;
            SendEvent(newChangingEvent);
        }

        public void SetValueWithoutNotify(string newValue)
        {
            Field.SetValueWithoutNotify(newValue);
        }

        public new virtual void Focus() { }
    }

#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class TextFieldWithValidation : InputFieldWithValidation
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<TextFieldWithValidation, UxmlTraits> {}
#endif

        private TextField m_TextField => Field as TextField;

        internal string PlaceholderText
        {
            get => m_TextField.placeholder;
            set => m_TextField.placeholder = value;
        }

        public TextFieldWithValidation()
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/TextFieldWithValidation_uss.uss"));
            AddFieldToValidate(new TextField());
        }

        public override void Focus()
        {
            m_TextField.Focus();
        }
    }
    
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class StoryFieldWithValidation : InputFieldWithValidation
    {
        internal WordTypeSentence Sentence;
        private const string k_InvalidIdentifierErrorMessage = "Variable name must start with letter or '_'.\nIt can only contain letters, '_', and numbers.";
        private TextArea m_TextArea => Field as TextArea;
        
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<StoryFieldWithValidation, UxmlTraits> {}
#endif

        public StoryFieldWithValidation()
        {
            TextArea textArea = new TextArea();
            textArea.autoResize = true;
            // A temporary fix to go around an issue where the App UI TextArea scroll is visible by default when the element has a smaller size than default.
            textArea.Q<ScrollView>().verticalScrollerVisibility = ScrollerVisibility.Hidden;
            AddFieldToValidate(textArea);
        }

        protected override bool ValidateFieldForInvalidCharactersOrEmpty()
        {
            if (Sentence == null)
            {
                return true;
            }

            Sentence.UpdateWordTypeList(this.Q<UnityEngine.UIElements.TextField>().cursorIndex, Value);
            if (!Sentence.WordTypeParameters.Any())
            {
                IsValid = true;
            }
            else
            {
                foreach (WordTypePair parameter in Sentence.WordTypeParameters)
                {
                    IsValid = parameter.Word.Replace(" ", string.Empty).IsValidIdentifier();
                    if (!IsValid)
                    {
                        break;
                    }
                }
            }

            m_TextArea.invalid = !IsValid;
            tooltip = IsValid ? null : k_InvalidIdentifierErrorMessage;

            return IsValid;
        }

        public override void Focus()
        {
            m_TextArea.Focus();
        }
    }
}
#endif