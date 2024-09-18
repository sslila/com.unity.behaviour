using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Behavior
{
    [Serializable]
    internal class SharedBlackboardVariable : BlackboardVariable, ISharedBlackboardVariable
    {
        [SerializeField] private RuntimeBlackboardAsset m_GlobalVariablesRuntimeAsset;
        public override Type Type { get; }
        public override object ObjectValue   
        {
            get
            {
                m_GlobalVariablesRuntimeAsset.Blackboard.GetVariable(GUID, out BlackboardVariable variable);
                return variable;
            }
            set
            {
                m_GlobalVariablesRuntimeAsset.Blackboard.GetVariable(GUID, out BlackboardVariable variable);
                bool valueChanged = !Equals(variable, value);
                if (valueChanged)
                {
                    m_GlobalVariablesRuntimeAsset.Blackboard.SetVariableValue(variable.GUID, value);
                    InvokeValueChanged();
                }
            }
        }
        
        public SharedBlackboardVariable()
        {
            Type = GetType();
        }

        public void SetSharedVariablesRuntimeAsset(RuntimeBlackboardAsset globalVariablesRuntimeAsset)
        {
            m_GlobalVariablesRuntimeAsset = globalVariablesRuntimeAsset;
        }

        internal override BlackboardVariable Duplicate()
        {
            var blackboardVariableDuplicate = CreateForType(Type, true);
            blackboardVariableDuplicate.Name = Name;
            blackboardVariableDuplicate.GUID = GUID;
            return blackboardVariableDuplicate;
        }

        public override bool ValueEquals(BlackboardVariable other)
        {
           return ObjectValue.Equals(other.ObjectValue);
        }
    }
    
    [Serializable]
    internal class SharedBlackboardVariable<DataType> : BlackboardVariable<DataType>, ISharedBlackboardVariable
    {
        [SerializeField] internal RuntimeBlackboardAsset m_SharedVariablesRuntimeAsset;

        public SharedBlackboardVariable(){}
        
        public SharedBlackboardVariable(DataType value) : base(value)
        {
        }
        
        /// <summary>
        /// see <see cref="BlackboardVariable.ObjectValue"/>
        /// </summary>
        public override DataType Value
        {
            get
            {
                m_SharedVariablesRuntimeAsset.Blackboard.GetVariable(GUID, out BlackboardVariable<DataType> variable);
                return variable;
            }
            set
            {
                m_SharedVariablesRuntimeAsset.Blackboard.GetVariable(GUID, out BlackboardVariable<DataType> variable);
                bool valueChanged = !EqualityComparer<DataType>.Default.Equals(variable.Value, value);
                if (valueChanged)
                {
                    m_SharedVariablesRuntimeAsset.Blackboard.SetVariableValue(variable.GUID, value);
                    InvokeValueChanged();
                }
            }
        }
        
        internal override BlackboardVariable Duplicate()
        {
            BlackboardVariable blackboardVariableDuplicate = CreateForType(Type, true);
            blackboardVariableDuplicate.Name = Name;
            blackboardVariableDuplicate.GUID = GUID;
            return blackboardVariableDuplicate;
        }
        
        public void SetSharedVariablesRuntimeAsset(RuntimeBlackboardAsset globalVariablesRuntimeAsset)
        {
            m_SharedVariablesRuntimeAsset = globalVariablesRuntimeAsset;
        }
    }

    internal interface ISharedBlackboardVariable
    {
        void SetSharedVariablesRuntimeAsset(RuntimeBlackboardAsset globalVariablesRuntimeAsset);
    }
}