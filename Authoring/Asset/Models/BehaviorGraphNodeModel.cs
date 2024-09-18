using Unity.Behavior.GraphFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    internal class BehaviorGraphNodeModel : NodeModel
    {
        [SerializeField]
        public SerializableType NodeType;

        [SerializeField]
        public SerializableGUID NodeTypeID;

        public virtual bool IsRoot => false;

        public BehaviorGraphNodeModel(NodeInfo nodeInfo)
        {
            if (nodeInfo == null)
            {
                return;
            }
            NodeType = nodeInfo.SerializableType;
            NodeTypeID = nodeInfo.TypeID;
        }
        
        public BehaviorGraphNodeModel() { }

        protected BehaviorGraphNodeModel(BehaviorGraphNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
            NodeType = nodeModelOriginal.NodeType;
            NodeTypeID = nodeModelOriginal.NodeTypeID;

            foreach (FieldModel fieldModelOriginal in nodeModelOriginal.Fields)
            {
                VariableModel linkedVariable = fieldModelOriginal.LinkedVariable;
                if (linkedVariable != null && asset.Blackboard.Variables.All(variable => variable.ID != linkedVariable.ID))
                {
                    // Skip links to variables that do not exist in this asset, which can occur when copying nodes between assets.
                    continue;
                }
                
                m_FieldValues.Add(fieldModelOriginal.Duplicate());
            }
        }

        public override void OnValidate()
        {
            base.OnValidate();

            EnsureFieldValuesAreUpToDate();
            EnsurePortsAreUpToDate();
        }

        public override void OnDefineNode()
        {
            base.OnDefineNode();
            EnsurePortsAreUpToDate();
        }

        protected virtual void EnsureFieldValuesAreUpToDate()
        {
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(NodeTypeID);
            if (nodeInfo == null)
            {
                return;
            }

            List<FieldModel> fieldsToRemove = new List<FieldModel>();
            if (nodeInfo.Variables == null)
            {
                m_FieldValues.Clear();
            }
            foreach (FieldModel fieldModel in m_FieldValues)
            {
                bool foundMatch = false;
                foreach (VariableInfo variable in nodeInfo.Variables)
                {
                    Type variableType = variable.Type;
                    Type fieldType = fieldModel.LocalValue?.GetType() ?? fieldModel.LinkedVariable?.GetType();  
                    if (fieldModel.FieldName == variable.Name && fieldType != null && variableType.IsAssignableFrom(fieldType))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    fieldsToRemove.Add(fieldModel);
                }
            }
            foreach (FieldModel model in fieldsToRemove)
            {
                m_FieldValues.Remove(model);
            }
        }

        protected internal virtual void EnsurePortsAreUpToDate() { }

        public override IVariableLink GetVariableLink(string variableName, Type type) => GetOrCreateField(variableName, type);

        #region Blackboard Values
        [Serializable]
        internal class FieldModel : IVariableLink
        {
            [SerializeField]
            public string FieldName;

            [SerializeField]
            public SerializableType Type;

            [SerializeReference]
            public BlackboardVariable LocalValue;

            [SerializeReference]
            public VariableModel LinkedVariable;

            public FieldModel() { }

            private FieldModel(FieldModel fieldModelOriginal)
            {
                FieldName = fieldModelOriginal.FieldName;
                LocalValue = fieldModelOriginal.LocalValue?.Duplicate();
                LinkedVariable = fieldModelOriginal.LinkedVariable;
            }

            object IVariableLink.Value
            {
                get => LocalValue?.ObjectValue;
                set
                {
                    if (LocalValue != null)
                    {
                        LocalValue.ObjectValue = value;
                    }
                }
            }

            VariableModel IVariableLink.BlackboardVariable
            {
                get => LinkedVariable;
                set => LinkedVariable = value;
            }

            internal FieldModel Duplicate() => new (this);
        }

        [SerializeReference]
        internal List<FieldModel> m_FieldValues = new List<FieldModel>();

        internal IEnumerable<FieldModel> Fields => m_FieldValues;

        // Sets the variable associated with the field
        internal void SetField(string fieldName, VariableModel val, Type variableType)
        {
            FieldModel field = GetOrCreateField(fieldName, variableType);
            field.LinkedVariable = val;
        }

        // Sets the value stored in the field
        internal void SetField<TValue>(string fieldName, TValue value)
        {
            // using the runtime type (value.GetType()) here is necessary for Enums, cause these types are only known at runtime
            FieldModel field = GetOrCreateField(fieldName, value == null ? typeof(TValue) : value.GetType());
            field.LocalValue.ObjectValue = value;
        }

        internal bool HasField(string fieldName, Type variableType)
        {
            foreach (FieldModel fieldModel in m_FieldValues)
            {
                if (fieldModel.FieldName.Equals(fieldName) && variableType.IsAssignableFrom(fieldModel.LocalValue?.Type))
                {
                    return true;
                }
            }
            return false;
        }

        internal FieldModel GetOrCreateField(string fieldName, Type variableType)
        {
            foreach (FieldModel fieldModel in m_FieldValues)
            {
                if (!fieldModel.FieldName.Equals(fieldName))
                {
                    continue;
                }
                if (fieldModel.LocalValue != null && !variableType.IsAssignableFrom(fieldModel.LocalValue.Type) && !variableType.IsSubclassOf(fieldModel.LocalValue.Type))
                {
                    Debug.LogWarning($"Found mismatched field type for {fieldName} in {NodeType.Type.Name}. Expected {fieldModel.LocalValue.Type.Name}, but found {variableType.Name}.");
                    continue;
                }
                return fieldModel;
            }
            FieldModel field = new FieldModel();
            field.Type = variableType;
            field.FieldName = fieldName;
            m_FieldValues.Add(field);

            // For the field-local value, attempt to use the assigned default variable value.
            NodeInfo info = NodeRegistry.GetInfoFromTypeID(NodeTypeID);
            if (info?.Variables != null)
            {
                VariableInfo variableInfo = info.Variables.SingleOrDefault(item => item.Name.Equals(fieldName));
                if (variableInfo?.DefaultValue is BlackboardVariable defaultValue)
                {
                    BlackboardVariable localCopy = BlackboardVariable.CreateForType(defaultValue.Type);
                    localCopy.ObjectValue = defaultValue.ObjectValue;
                    field.LocalValue = localCopy;
                }
            }
            // If no default value was specified, create an empty variable (with the C# default value).
            field.LocalValue ??= BlackboardVariable.CreateForType(variableType);
            return field;
        }

        internal bool RemoveField(string fieldName)
        {
            int index = -1;
            for (int i = 0; i < m_FieldValues.Count; i++)
            {
                if (m_FieldValues[i].FieldName.Equals(fieldName))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return false;

            m_FieldValues.RemoveAt(index);
            return true;
        }
        #endregion

    }
}