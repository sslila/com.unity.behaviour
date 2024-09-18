using System;
using UnityEngine;

namespace Unity.Behavior
{
    internal interface IBlackboardVariableCast
    {
        public string SourceTypeName { get; }
        public string TargetTypeName { get; }
    }

    [Serializable]
    internal abstract class BlackboardVariableCaster<SourceType, TargetType> : BlackboardVariable<TargetType>, IBlackboardVariableCast
        where TargetType : class
    {
        [SerializeReference]
        private BlackboardVariable<SourceType> m_LinkedVariable;
        protected TargetType m_LinkedObject;
        private bool m_CallbackRegistered = false;

        public string SourceTypeName => typeof(SourceType).Name;
        public string TargetTypeName => typeof(TargetType).Name;

        // Required for serialization
        public BlackboardVariableCaster() { }

        public BlackboardVariableCaster(BlackboardVariable<SourceType> linkedVariable)
        {
            m_LinkedVariable = linkedVariable;
            m_CallbackRegistered = false;
        }

        public override TargetType Value
        {
            get
            {
                if (!m_CallbackRegistered)
                {
                    m_LinkedVariable.OnValueChanged += OnLinkedVariableValueChanged;
                    m_CallbackRegistered = true;
                    OnLinkedVariableValueChanged();
                }

                return m_LinkedObject;
            }
            set => m_LinkedVariable.Value = GetSourceObjectFromTarget(value);
        }

        protected abstract SourceType GetSourceObjectFromTarget(TargetType value);

        protected abstract TargetType GetTargetObjectFromSource(SourceType variable);

        private void OnLinkedVariableValueChanged()
        {
            if (m_LinkedVariable.Value == null)
            {
                m_LinkedObject = null;
                return;
            }

            m_LinkedObject = GetTargetObjectFromSource(m_LinkedVariable);
        }
    }
}
