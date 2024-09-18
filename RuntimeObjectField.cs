using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class RuntimeObjectField : BaseField<UnityEngine.Object>
    {
        AppUI.UI.LocalizedTextElement m_ValueLabel;
        VisualElement m_IconElement;
        public override UnityEngine.Object value
        {
            get => base.value;
            set
            {
                base.value = value;
                UpdateLabel();
            }
        }

        public RuntimeObjectField()
            : this(null)
        {
            var inputElement = this.Q(className: "unity-base-field__input");
            m_IconElement = new VisualElement();
            m_IconElement.style.display = DisplayStyle.None;
            m_IconElement.AddToClassList("Runtime-Object-Field_Icon");
            inputElement.Add(m_IconElement);


            m_ValueLabel = new AppUI.UI.LocalizedTextElement();
            m_ValueLabel.name = "Runtime-Object-Field_Value-Label";
            inputElement.Add(m_ValueLabel);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RuntimeObjectField(string label)
            : base(label, null)
        {
            AddToClassList("Runtime-Object-Field");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/RuntimeObjectFieldStyles.uss"));

            labelElement.focusable = false;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        }


        public override void SetValueWithoutNotify(UnityEngine.Object newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateLabel();
        }

        void UpdateLabel()
        {
            const string kHasValueClassName = "HasValue";
            EnableInClassList(kHasValueClassName, value != null);
            if (value == null)
            {
                m_ValueLabel.text = string.Empty;
                m_IconElement.style.display = DisplayStyle.None;
                return;
            }
            m_ValueLabel.text = value.name;
#if UNITY_EDITOR
            var icon = UnityEditor.AssetPreview.GetMiniThumbnail(value);
            m_IconElement.style.backgroundImage = icon;
            m_IconElement.style.display = icon == null ? DisplayStyle.None : DisplayStyle.Flex;
#endif
        }
    }

    internal class RuntimeGameObjectField : BaseField<GameObject>
    {
        public RuntimeGameObjectField()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RuntimeGameObjectField(string label)
            : base(label, null)
        {
            AddToClassList("Runtime-Object-Field");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/RuntimeObjectFieldStyles.uss"));
            labelElement.focusable = false;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        }
    }

    internal class RuntimeScriptableObjectField : BaseField<ScriptableObject>
    {
        public RuntimeScriptableObjectField()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RuntimeScriptableObjectField(string label)
            : base(label, null)
        {
            AddToClassList("Runtime-Object-Field");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/RuntimeObjectFieldStyles.uss"));

            labelElement.focusable = false;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        }
    }

    internal class RuntimeEnumField : BaseField<Enum>
    {
        public RuntimeEnumField() : this(null)
        {
        }
        
        public RuntimeEnumField(string label) : base(label, null)
        {
            AddToClassList("Runtime-Object-Field");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/RuntimeObjectFieldStyles.uss"));
            
            labelElement.focusable = false;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        }
    }
    
    internal class RuntimeListField<T> : BaseField<List<T>>
    {
        public RuntimeListField() : this(null)
        {
        }
        
        public RuntimeListField(string label) : base(label, null)
        {
            AddToClassList("Runtime-Object-Field");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/RuntimeObjectFieldStyles.uss"));
            
            labelElement.focusable = false;

            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        }
    }
}