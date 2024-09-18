using UnityEngine;
using System;

namespace Unity.Behavior
{
    [Serializable] //TODO, Generic SerializeReference doesnt work 2022
    internal class UnityObjectToUnityObjectBlackboardVariable<SourceType, TargetType> : BlackboardVariable<TargetType>
        where SourceType : UnityEngine.Object where TargetType : UnityEngine.Object
    {
        [SerializeReference]
        private BlackboardVariable<SourceType> m_LinkedVariable;
        protected TargetType m_LinkedObject;
        private bool m_CallbackRegistered = false;
        public UnityObjectToUnityObjectBlackboardVariable(BlackboardVariable<SourceType> linkedVariable)
        {
            m_LinkedVariable = linkedVariable;
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
            set
            {
                m_LinkedVariable.Value = value as SourceType;
            }
        }

        private void OnLinkedVariableValueChanged()
        {
            if (m_LinkedVariable.Value == null)
            {
                m_LinkedObject = null;
                return;
            }

            m_LinkedObject = m_LinkedVariable.Value as TargetType;
        }
    }
}
