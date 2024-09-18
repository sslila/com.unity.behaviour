#if UNITY_EDITOR

using System;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class LinkField<TValueType, TFieldType> : BaseLinkField, INotifyValueChanged<TValueType> where TFieldType : VisualElement, INotifyValueChanged<TValueType>, new()
    {
        private readonly TFieldType m_Field;

        private Tooltip m_FieldTooltip;

        public void SetValueWithoutNotify(TValueType newValue)
        {
            m_Field.SetValueWithoutNotify(newValue);
        }

        internal TFieldType Field => m_Field;

        public TValueType value
        {
            get => m_Field.value;
            set
            {
                m_Field.SetValueWithoutNotify(value);

                using LinkFieldValueChangeEvent changeEvent = LinkFieldValueChangeEvent.GetPooled(this, value);
                SendEvent(changeEvent);
            }
        }

        internal LinkField() : this(typeof(TValueType))
        {
        }

        internal LinkField(Type runtimeType)
        {
            LinkVariableType = runtimeType;

            m_Field = new TFieldType { name = "InputField" };
            FieldContainer.Clear();
            FieldContainer.Add(m_Field);

            SetFieldIcon(runtimeType);
            tooltip = LinkVariableType.Name;
            this.SetPreferredTooltipPlacement(PopoverPlacement.Top);

            m_Field.RegisterValueChangedCallback(OnValueChanged);

            VisualElement linkFieldSpacer = new VisualElement();
            linkFieldSpacer.AddToClassList("LinkButton");
            linkFieldSpacer.style.position = Position.Relative;
            linkFieldSpacer.style.visibility = Visibility.Hidden;
            m_Field.hierarchy.Add(linkFieldSpacer);
        }

        private void OnValueChanged(ChangeEvent<TValueType> evt)
        {
            // todo appUI fields send change events where newValue == previousValue.
            // if (evt.newValue.Equals(evt.previousValue))
            //     return;

            using LinkFieldValueChangeEvent changeEvent = LinkFieldValueChangeEvent.GetPooled(this, evt.newValue);
            SendEvent(changeEvent);
        }

        internal override void UpdateValue(IVariableLink field)
        {
            if (field.Value == null)
            {
                SetValueWithoutNotify(default);
            }
            else
            {
                SetValueWithoutNotify((TValueType)field.Value);
            }

            base.UpdateValue(field);
        }

        internal override void SetValue(object value)
        {
            if (value == null || LinkVariableType.IsAssignableFrom(value.GetType())) {
                this.value = (TValueType)value;
            }
        }
    }
}
#endif