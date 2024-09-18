using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_UXML_UI_SERIALIZATION
using Unity.Properties;
#endif

namespace Unity.Behavior.GraphFramework
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class EditableLabel : BaseField<string>
    {
#if ENABLE_UXML_UI_SERIALIZATION
        internal static readonly BindingId textProperty = nameof(Text);
        internal static readonly BindingId isDelayedProperty = nameof(isDelayed);
        internal static readonly BindingId multilineProperty = nameof(multiline);
#endif

#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<EditableLabel, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
                { name = "text" };

            private readonly UxmlBoolAttributeDescription m_Multiline = new UxmlBoolAttributeDescription
                { name = "multiline" };

            private readonly UxmlBoolAttributeDescription m_Delayed = new UxmlBoolAttributeDescription
                { name = "delayed" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                EditableLabel editableLabel = ve as EditableLabel;
                editableLabel.Text = m_Text.GetValueFromBag(bag, cc);
                editableLabel.isDelayed = m_Delayed.GetValueFromBag(bag, cc);
                editableLabel.multiline = m_Multiline.GetValueFromBag(bag, cc);
            }
        }
#endif

#if ENABLE_UXML_UI_SERIALIZATION
        [CreateProperty]
        [UxmlAttribute("text")]
#endif
        public string Text
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;
                m_Field.SetValueWithoutNotify(value);
                
#if ENABLE_UXML_UI_SERIALIZATION
                if (changed)
                    NotifyPropertyChanged(in textProperty);
#endif
            }
        }

#if ENABLE_UXML_UI_SERIALIZATION
        [CreateProperty]
        [UxmlAttribute("delayed")]
#endif
        public bool isDelayed
        {
            get => m_Field.isDelayed;
            set
            {
                var changed = m_Field.isDelayed != value;
                m_Field.isDelayed = value;
                
#if ENABLE_UXML_UI_SERIALIZATION
                if (changed)
                    NotifyPropertyChanged(in isDelayedProperty);
#endif
            }
        }

#if ENABLE_UXML_UI_SERIALIZATION
        [CreateProperty]
        [UxmlAttribute]
#endif
        public bool multiline
        {
            get => m_Field.multiline;
            set
            {
                var changed = m_Field.multiline != value;
                m_Field.multiline = value;
                
#if ENABLE_UXML_UI_SERIALIZATION
                if (changed)
                    NotifyPropertyChanged(in multilineProperty);
#endif
            }
        }

        private Label m_Label;
        private TextField m_Field;
        bool m_IsEditing;

        public EditableLabel()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditableLabel(string label)
            : base(label, null)
        {
            focusable = false;
            AddToClassList("EditableLabel");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/EditableLabelStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Elements/Assets/EditableLabelLayout.uxml").CloneTree(this);

            m_Label = this.Q<Label>("Text");
            m_Field = this.Q<TextField>("TextField");
            this.Q(className: "unity-base-field__input")?.RemoveFromHierarchy();

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            RegisterCallback<MouseDownEvent>(OnPointerDown);
            m_Field.RegisterValueChangedCallback(OnTextChanged);
            m_Field.RegisterCallback<KeyDownEvent>(e =>
            {
                switch (e.keyCode)
                {
                    case KeyCode.Escape:
                        m_Field.SetValueWithoutNotify(Text);
                        ToggleTitleFields();
                        break;
                }
            });
        }

        private void OnTextChanged(ChangeEvent<string> evt)
        {
            Text = m_Field.value;
            evt.StopImmediatePropagation();
            SendEvent(ChangeEvent<string>.GetPooled(evt.previousValue, evt.newValue));
        }

        private void OnPointerDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && !m_IsEditing)
            {
                ToggleTitleFields();
                evt.StopPropagation();
            }
        }

        private void OnFieldFocusOut(BlurEvent evt)
        {
            ToggleTitleFields();
        }

        public void ToggleTitleFields()
        {
            m_IsEditing = !m_IsEditing;

            if (m_IsEditing)
            {

                m_Field.SetValueWithoutNotify(Text);
                m_Field.Q<TextElement>().style.color = m_Label.resolvedStyle.color;
                AddToClassList("Editing");
                focusable = true;
                m_Field.style.display = DisplayStyle.Flex;
                m_Label.style.display = DisplayStyle.None;
                m_Field.RegisterCallback<BlurEvent>(OnFieldFocusOut);
                m_Field.Focus();
                m_Field.SelectAll();
            }
            else
            {
                m_Field.UnregisterCallback<BlurEvent>(OnFieldFocusOut);
                focusable = false;
                m_Field.style.display = DisplayStyle.None;
                m_Label.style.display = DisplayStyle.Flex;
                RemoveFromClassList("Editing");
            }
        }
    }
}
