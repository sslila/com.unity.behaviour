using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.Behavior
{
    internal class ActionNodeModel : BehaviorGraphNodeModel
    {
        public override bool IsSequenceable => true;

        public ActionNodeModel(NodeInfo nodeInfo) : base(nodeInfo) { }

        protected ActionNodeModel(ActionNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
        }

        public override void OnValidate()
        {
            base.OnValidate();
            
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(NodeTypeID);
            if (nodeInfo == null)
            {
                return;
            }
            MemberInfo[] fields = GetFieldsAndProperties(nodeInfo.SerializableType);
            if (fields.Length != m_FieldValues.Count)
            {
                FixNodeFields(fields);
                return;
            }

            for (int i = 0; i < fields.Length; ++i)
            {
                if (!fields[i].Name.Equals(m_FieldValues[i].FieldName))
                {
                    FixNodeFields(fields);
                    return;
                }
            }
        }
        
        private void FixNodeFields(MemberInfo[] fields)
        {
            // Cache the old fields.
            Dictionary<string, FieldModel> oldFields = new Dictionary<string, FieldModel>(m_FieldValues.Count);
            foreach (FieldModel oldField in m_FieldValues)
            {
                oldFields.Add(oldField.FieldName, oldField);
            }

            // Create the new fields.
            List<FieldModel> fieldValues = new List<FieldModel>(fields.Length);
            for (int fieldIndex = 0; fieldIndex < fields.Length; ++fieldIndex)
            {
                MemberInfo memberInfo = fields[fieldIndex];
                if (oldFields.TryGetValue(memberInfo.Name, out FieldModel oldField))
                {
                    fieldValues.Add(oldField);
                    continue;
                }
                
                // If the field is a blackboard variable, create an associated field model.
                Type variableFieldType = GetTypeFromMemberInfo(memberInfo); 
                if (variableFieldType.IsGenericType && variableFieldType.GetGenericTypeDefinition() == typeof(BlackboardVariable<>))
                {
                    fieldValues.Add(GetOrCreateField(memberInfo.Name, variableFieldType.GetGenericArguments()[0]));
                }
            }
            m_FieldValues = fieldValues;
        }
        
        private static Type GetTypeFromMemberInfo(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
            }
            throw new Exception("Invalid MemberInfo");
        }
        
        private static MemberInfo[] GetFieldsAndProperties(Type actionType)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            return actionType.GetFields(bindingFlags)
                .Concat<MemberInfo>(actionType.GetProperties(bindingFlags)).ToArray();
        }
    }
}