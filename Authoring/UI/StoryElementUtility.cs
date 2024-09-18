#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal static class StoryElementUtility
    {
        internal delegate BaseLinkField OnCreateLinkField(string name, SerializableType type);
        
        internal delegate ComparisonConditionElement OnCreateComparisonElement(string name, ComparisonType comparisonEnum);
        
        internal static void CreateStoryElement(string story, List<VariableInfo> variables, VisualElement element, OnCreateLinkField onCreateLinkField, OnCreateComparisonElement comparisonElementCallback = null)
        {
            string[] words = story.Split(' ');
            string currentLabel = string.Empty;

            for (int i = 0; i < words.Length; ++i)
            {
                string word = words[i];
                if (word.StartsWith("[") && word.EndsWith("]"))
                {
                    if (currentLabel.Length != 0)
                    {
                        Label label = new Label(currentLabel);
                        label.AddToClassList("BTLabelExtraSpace");
                        element.Add(label);
                        currentLabel = string.Empty;
                    }

                    word = word.TrimStart('[');
                    word = word.TrimEnd(']');
                    FindAndAddField(word, variables, element, onCreateLinkField, comparisonElementCallback);
                    if (i == 0)
                    {
                        element.AddToClassList("BehaviorGraphNode-Offset");
                    }
                }
                else
                {
                    if (currentLabel.Length != 0)
                    {
                        currentLabel += " ";
                    }

                    currentLabel += word;
                }
            }

            // Set up any comparison elements with variables linked to them.
            List<ComparisonConditionElement> comparisonElements = element.Query<ComparisonConditionElement>().ToList();
            foreach (ComparisonConditionElement comparisonElement in comparisonElements)
            {
                if (comparisonElement.Attribute == null)
                {
                    continue;
                }
                // If the comparison element has been defined as a blackboard variable comparison, link the variable fields.
                LinkComparisonElementFields(comparisonElement, element);   
            }

            if (currentLabel.Length != 0)
            {
                Label label = new Label(currentLabel);
                element.Add(label);
            }
        }

        private static void FindAndAddField(string fieldName, List<VariableInfo> variables, VisualElement element, OnCreateLinkField onCreateLinkField, OnCreateComparisonElement comparisonElementCallback)
        {
            if (variables != null)
            {
                for (int i = 0; i < variables.Count; ++i)
                {
                    VariableInfo variable = variables[i];
                    string nicifiedName = Util.NicifyVariableName(variable.Name).Replace(" ", "");
                    string nicifiedFieldName = Util.NicifyVariableName(fieldName).Replace(" ", "");
                    if (nicifiedFieldName.Equals(nicifiedName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (variable.Type.Type == typeof(BlackboardVariable<ConditionOperator>))
                        {
                            if (comparisonElementCallback == null)
                            {
                                return;
                            }
                            ComparisonConditionElement comparisonElement = comparisonElementCallback(variable.Name, ComparisonType.All);
                            element.Add(comparisonElement);
                        }
                        else
                        {
                            BaseLinkField field = onCreateLinkField(variable.Name, variable.Type);
                            field.FieldName = variable.Name;
                            element.Add(field);

                            if (variable.Tooltip != null)
                            {
                                element.tooltip = variable.Tooltip;
                                field.tooltip = variable.Tooltip;
                            }
                        }
                    }
                }
            }
        }
        
        internal static void LinkComparisonElementFields(ComparisonConditionElement comparisonElement, VisualElement element)
        {
            BaseLinkField variableField = null;
            BaseLinkField valueField = null;
            List<BaseLinkField> fields = element.Query<BaseLinkField>().ToList();
            foreach (BaseLinkField field in fields)
            {
                if (field.FieldName == comparisonElement.Attribute.Variable)
                {
                    variableField = field;
                    variableField.FieldName = field.FieldName;
                }   
                else if (field.FieldName == comparisonElement.Attribute.ComparisonValue)
                {
                    valueField = field;
                    valueField.FieldName = field.FieldName;
                }
            }
            
            // If the left operand is defined as a Blackboard variable
            if (variableField != null)
            {
                comparisonElement.RefreshOperatorsFromVariable(variableField);
                if (valueField != null)
                {
                    RefreshComparisonValueLinkField(valueField, variableField, comparisonElement);
                }
            }
            // If the right operand is defined as a Blackboard variable
            else if (valueField != null)
            {
                comparisonElement.RefreshOperatorsFromVariable(valueField);
            }

            // If the variable link is changed
            if (variableField != null)
            {
                variableField.OnLinkChanged += _ =>
                {
                    RefreshVariableLinkField(variableField, comparisonElement.ConditionModel);
                    comparisonElement.RefreshOperatorsFromVariable(variableField);
                    RefreshComparisonValueLinkField(valueField, variableField, comparisonElement);
                    
                    // Ensure that the NodeUI is being updated on link change.
                    BehaviorNodeUI nodeUI = comparisonElement.GetFirstAncestorOfType<BehaviorNodeUI>();
                    nodeUI?.UpdateLinkFields();
                };
            }
        }

        private static void RefreshVariableLinkField(BaseLinkField variableField, ConditionModel model)
        {
            VariableModel assignedVariable = variableField.LinkedVariable;
            BehaviorGraphNodeModel.FieldModel fieldModel = model.m_FieldValues.First(field => field.FieldName == variableField.FieldName);
            if (assignedVariable == null)
            {
                variableField.LinkVariableType = typeof(object);
                fieldModel.LocalValue = BlackboardVariable.CreateForType(typeof(object));
                return;
            }
            
            variableField.LinkVariableType = assignedVariable.Type;
            fieldModel.LocalValue = BlackboardVariable.CreateForType(assignedVariable.Type);
        }

        private static void RefreshComparisonValueLinkField(BaseLinkField valueField, BaseLinkField variableField, ComparisonConditionElement comparisonElement)
        {
            if (valueField == null)
            {
                return;
            }
            
            string fieldName = valueField.FieldName;
            VisualElement parentElement = valueField.parent;
            valueField.RemoveFromHierarchy();
            if (variableField != null)
            {
                if (variableField.LinkedVariable == null)
                {
                    // Unlink the existing field and create a new link field.
                    comparisonElement.ConditionModel.RemoveField(fieldName);
                    valueField = LinkFieldUtility.CreateConditionLinkField(fieldName, typeof(BlackboardVariable), comparisonElement.ConditionModel);
                    valueField.SetEnabled(false);
                }
                else
                {
                    var variableType = variableField.LinkedVariable.Type;
                    valueField = LinkFieldUtility.CreateConditionLinkField(fieldName, variableType, comparisonElement.ConditionModel);
                
                    // With the new field created, we need to update the field model's local value type. 
                    if (!comparisonElement.ConditionModel.HasField(fieldName, variableType))
                    {
                        // Remove the field if it exists but has a different type.
                        comparisonElement.ConditionModel.RemoveField(fieldName);
                        // Add the new field
                        comparisonElement.ConditionModel.GetOrCreateField(fieldName, variableType);
                    }
                }   
            }

            valueField.FieldName = fieldName;
            valueField.Model = comparisonElement.ConditionModel;
            parentElement?.Add(valueField);
        }
    }
}
#endif