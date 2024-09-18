using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    internal class ConditionModel : BaseModel
    {
        [SerializeField] public SerializableType ConditionType;

        [SerializeField] public SerializableGUID ConditionTypeID;

        [SerializeReference] internal BehaviorGraphNodeModel NodeModel;

        [SerializeReference]
        internal List<BehaviorGraphNodeModel.FieldModel> m_FieldValues = new List<BehaviorGraphNodeModel.FieldModel>();

        internal IEnumerable<BehaviorGraphNodeModel.FieldModel> Fields => m_FieldValues;

        [SerializeReference] internal BehaviorGraphNodeModel.FieldModel OperatorFieldModel;

        public ConditionModel()
        {
        }

        internal ConditionModel(BehaviorGraphNodeModel nodeModel, Condition condition, ConditionInfo info)
        {
            Asset = nodeModel.Asset;
            NodeModel = nodeModel;
            ConditionType = info.SerializableType;
            ConditionTypeID = info.TypeID;

            DefineNode();
        }

        public ConditionModel Copy(ConditionModel original, BehaviorGraphNodeModel model)
        {
            ConditionModel copy = new ConditionModel
            {
                NodeModel = model,
                ConditionTypeID = original.ConditionTypeID,
                ConditionType = original.ConditionType
            };

            foreach (BehaviorGraphNodeModel.FieldModel fieldOriginal in original.m_FieldValues)
            {
                VariableModel linkedVariable = fieldOriginal.LinkedVariable;
                if (linkedVariable != null)
                {
                    if (model.Asset.Blackboard.Variables.All(variable => variable.ID != linkedVariable.ID))
                    {
                        // Skip links to variables that do not exist in this asset, which can occur when copying nodes between assets.
                        continue;
                    }
                }

                copy.m_FieldValues.Add(fieldOriginal.Duplicate());
            }

            return copy;
        }

        public void Validate()
        {
            EnsureFieldValuesAreUpToDate();
        }

        protected virtual void EnsureFieldValuesAreUpToDate()
        {
#if UNITY_EDITOR
            ConditionInfo conditionInfo = NodeRegistry.GetConditionInfoFromTypeID(ConditionTypeID);
            if (conditionInfo == null)
            {
                return;
            }

            List<BehaviorGraphNodeModel.FieldModel> fieldsToRemove = new List<BehaviorGraphNodeModel.FieldModel>();
            if (conditionInfo.Variables == null)
            {
                m_FieldValues.Clear();
            }

            foreach (BehaviorGraphNodeModel.FieldModel fieldModel in m_FieldValues)
            {
                bool foundMatch = false;
                foreach (VariableInfo variable in conditionInfo.Variables)
                {
                    Type variableType = variable.Type;
                    Type fieldType = fieldModel.LocalValue?.GetType() ?? fieldModel.LinkedVariable?.GetType();
                    if (fieldModel.FieldName == variable.Name && fieldType != null &&
                        variableType.IsAssignableFrom(fieldType))
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

            foreach (BehaviorGraphNodeModel.FieldModel model in fieldsToRemove)
            {
                m_FieldValues.Remove(model);
            }
#endif
        }

        public void DefineNode()
        {
            EnsureFieldValuesAreUpToDate();
        }

#if UNITY_EDITOR

        public override IVariableLink GetVariableLink(string variableName, Type type) =>
            GetOrCreateField(variableName, type);

        // Sets the variable associated with the field
        internal void SetField(string fieldName, VariableModel val, Type variableType)
        {
            BehaviorGraphNodeModel.FieldModel field = GetOrCreateField(fieldName, variableType);
            field.LinkedVariable = val;
        }

        // Sets the value stored in the field
        internal void SetField<TValue>(string fieldName, TValue value)
        {
            // using the runtime type (value.GetType()) here is necessary for Enums, cause these types are only known at runtime
            BehaviorGraphNodeModel.FieldModel field =
                GetOrCreateField(fieldName, value == null ? typeof(TValue) : value.GetType());
            field.LocalValue.ObjectValue = value;
        }

        internal bool HasField(string fieldName, Type variableType)
        {
            foreach (BehaviorGraphNodeModel.FieldModel fieldModel in m_FieldValues)
            {
                if (fieldModel.FieldName.Equals(fieldName) &&
                    variableType.IsAssignableFrom(fieldModel.LocalValue?.Type))
                {
                    return true;
                }
            }

            return false;
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
        
        internal BehaviorGraphNodeModel.FieldModel GetOrCreateField(string fieldName, Type variableType)
        {
            foreach (BehaviorGraphNodeModel.FieldModel fieldModel in m_FieldValues)
            {
                if (!fieldModel.FieldName.Equals(fieldName))
                {
                    continue;
                }

                if (fieldModel.LocalValue != null && !variableType.IsAssignableFrom(fieldModel.LocalValue.Type))
                {
                    Debug.LogWarning(
                        $"Found mismatched field type for {fieldName} in {this}. Expected {variableType.Name}, but found {fieldModel.LocalValue.Type.Name}.");
                    continue;
                }

                return fieldModel;
            }

            BehaviorGraphNodeModel.FieldModel field = new BehaviorGraphNodeModel.FieldModel();
            field.Type = variableType;
            field.FieldName = fieldName;
            m_FieldValues.Add(field);

            // For the field-local value, attempt to use the assigned default variable value.
            ConditionInfo info = ConditionUtility.GetInfoForConditionType(ConditionType);
            VariableInfo variableInfo = info?.Variables?.SingleOrDefault(item => item.Name.Equals(fieldName));
            if (variableInfo?.DefaultValue is BlackboardVariable defaultValue)
            {
                BlackboardVariable localCopy = BlackboardVariable.CreateForType(defaultValue.Type);
                localCopy.ObjectValue = defaultValue.ObjectValue;
                field.LocalValue = localCopy;
            }

            // If no default value was specified, create an empty variable (with the C# default value).
            field.LocalValue ??= BlackboardVariable.CreateForType(variableType);
            return field;
        }

        internal void CreateOperatorField(string fieldName)
        {
            OperatorFieldModel = GetOrCreateField(fieldName, typeof(ConditionOperator));
            OperatorFieldModel.FieldName = fieldName;
            OperatorFieldModel.LocalValue = new BlackboardVariable<ConditionOperator>
                { Value = ConditionOperator.Equal };
        }
#endif

        internal void SetOperatorValue(Enum newValue)
        {
            OperatorFieldModel.LocalValue.ObjectValue = newValue;
        }

        internal ConditionOperator GetOperatorValue()
        {
            return (ConditionOperator)OperatorFieldModel.LocalValue.ObjectValue;
        }

        internal bool HasOperatorValue()
        {
            return OperatorFieldModel?.LocalValue?.ObjectValue != null;
        }
    }
}