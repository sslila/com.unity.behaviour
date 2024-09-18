#if UNITY_EDITOR
using System;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(SetValueNodeModel))]
    internal class SetValueNodeUI : BehaviorNodeUI
    {
        private string k_ValueFieldName => SetValueNodeModel.k_ValueFieldName;
        private string k_VariableFieldName => SetValueNodeModel.k_VariableFieldName;
        private BaseLinkField m_VariableLinkField;
        private BaseLinkField m_ValueLinkField;
        private BehaviorGraphNodeModel m_Model => Model as BehaviorGraphNodeModel;

        public SetValueNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Action");

            // If the field already exists in the data, we know which type should be assigned on the type.
            BehaviorGraphNodeModel.FieldModel fieldModel = (Model as BehaviorGraphNodeModel).m_FieldValues
                .FirstOrDefault(field => field.FieldName == k_VariableFieldName);
            Type variableType = fieldModel?.LinkedVariable != null ? fieldModel.LinkedVariable.Type : typeof(object);
            m_VariableLinkField = new BaseLinkField()
            {
                FieldName = k_VariableFieldName, 
                LinkVariableType = variableType, 
                Model = nodeModel
            };
            
            Add(new Label("Set"));
            Add(m_VariableLinkField);
            Add(new Label("to"));
            CreateNewValueField();

            m_VariableLinkField.OnLinkChanged += assignedVariable =>
            {
                m_VariableLinkField.Dispatcher.DispatchImmediate(new SetNodeVariableLinkCommand(m_Model, m_VariableLinkField.FieldName, m_VariableLinkField.LinkVariableType, m_VariableLinkField.LinkedVariable, true));
                Type fieldType = assignedVariable?.Type ?? typeof(object);
                m_VariableLinkField.LinkVariableType = fieldType; 
                UpdateModelFields(assignedVariable, fieldType); 
                UpdateLinkFields();
            };
        }

        private void UpdateModelFields(VariableModel assignedVariable, Type fieldType)
        {
            // Update variable field
            m_Model.RemoveField(k_VariableFieldName);
            var variableField = m_Model.GetOrCreateField(k_VariableFieldName, fieldType);
            variableField.LinkedVariable = assignedVariable;
            variableField.LocalValue = BlackboardVariable.CreateForType(fieldType);

            // Update value field
            m_Model.RemoveField(k_ValueFieldName);
            var valueField = m_Model.GetOrCreateField(k_ValueFieldName, fieldType);
            valueField.LocalValue = BlackboardVariable.CreateForType(fieldType);
        }

        internal override void UpdateLinkFields()
        {
            CreateNewValueField();
            base.UpdateLinkFields();
        }

        private void CreateNewValueField()
        {
            if (m_ValueLinkField != null)
            {
                m_ValueLinkField.RemoveFromHierarchy();
                m_ValueLinkField = null;
            }

            if (m_VariableLinkField.LinkedVariable == null)
            {
                // Clear the linked variable and local value stored in the field model.
                BehaviorGraphNodeModel.FieldModel fieldModel = (Model as BehaviorGraphNodeModel).m_FieldValues
                    .FirstOrDefault(field => field.FieldName == k_ValueFieldName);
                if (fieldModel != null)
                {
                    fieldModel.LocalValue = BlackboardVariable.CreateForType(typeof(object));
                    fieldModel.LinkedVariable = null;
                }
                
                m_ValueLinkField = new BaseLinkField() { FieldName = k_ValueFieldName, Model = Model };
                m_ValueLinkField.SetEnabled(false);
            }
            else
            {
                // With the new field created, we need to update the field model's local value type. 
                BehaviorGraphNodeModel.FieldModel fieldModel = (Model as BehaviorGraphNodeModel).GetOrCreateField(k_ValueFieldName, m_VariableLinkField.LinkedVariable.Type);
                if (fieldModel.LocalValue.Type != m_VariableLinkField.LinkedVariable.Type)
                {
                    fieldModel.LocalValue = BlackboardVariable.CreateForType(m_VariableLinkField.LinkedVariable.Type);
                }
                
                // Set up the new value field.
                m_ValueLinkField = LinkFieldUtility.CreateNodeLinkField(k_ValueFieldName, m_VariableLinkField.LinkedVariable.Type);
                m_ValueLinkField.FieldName = k_ValueFieldName;
                m_ValueLinkField.Model = Model;
            }
            Add(m_ValueLinkField);
        }
    }
}
#endif