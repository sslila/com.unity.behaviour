#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.AppUI.UI;

namespace Unity.Behavior
{    
    internal class ComparisonConditionElement : VisualElement
    {
        internal readonly ConditionModel ConditionModel;
        internal readonly ComparisonAttribute Attribute;
        internal ComparisonType ComparisonType;

        internal Dropdown OperationDropdownField;
        private Label m_CompareRelationLabel;
        
        private readonly Dictionary<string, ConditionOperator> m_AllOperatorMapping = new()
        {
            { "Equal", ConditionOperator.Equal },
            { "Not Equal", ConditionOperator.NotEqual },
            { "Greater", ConditionOperator.Greater },
            { "Lower", ConditionOperator.Lower },
            { "Greater / Equal", ConditionOperator.GreaterOrEqual },
            { "Lower / Equal", ConditionOperator.LowerOrEqual },
        };
        
        private readonly Dictionary<string, ConditionOperator> m_BooleanOperatorMapping = new()
        {
            { "Equal", ConditionOperator.Equal },
            { "Not Equal", ConditionOperator.NotEqual }
        };

        internal ComparisonConditionElement(ConditionModel conditionModel, string fieldName, ComparisonType comparisonType)
        {
            AddToClassList("Behavior-Reflection");
            ConditionModel = conditionModel;
            ComparisonType = comparisonType;
            
                    
            // Get the attribute attached to the condition operator field on the type, if there is one.
            Attribute = TryGetAttribute();

            if (Attribute == null)
            {
                return;
            }

            ComparisonType = Attribute.ComparisonType;

            if (ConditionModel.OperatorFieldModel == null)
            {
                ConditionModel.CreateOperatorField(fieldName);
            }
            
            // Create the dropdown field.
            SetupDropdownElement(GetChoicesFromComparisonType());
        }

        private ComparisonAttribute TryGetAttribute()
        {
            Type type =ConditionModel.ConditionType;
            
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttributes(typeof(ComparisonAttribute)).FirstOrDefault() is ComparisonAttribute attribute)
                {
                    return attribute;
                }
            }
            return null;
        }

        private object GetOperatorMappingValueFromKey(string key)
        {
            switch (ComparisonType)
            {
                case ComparisonType.All or ComparisonType.BlackboardVariables:
                    if (m_AllOperatorMapping.TryGetValue(key, out ConditionOperator allOperator))
                    {
                        return allOperator;
                    }
                    break;
                case ComparisonType.Boolean:
                    if (m_BooleanOperatorMapping.TryGetValue(key, out ConditionOperator booleanOperator))
                    {
                        return booleanOperator;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If key is not found from the mapping, default to the first value.
            return m_AllOperatorMapping.First().Value;
        }

        internal void SetupDropdownElement(List<string> choices)
        {
            if (m_CompareRelationLabel == null)
            {
                m_CompareRelationLabel = new Label(" to ");
            }
            else
            {
                m_CompareRelationLabel.RemoveFromHierarchy();
            }

            if (OperationDropdownField != null)
            {
                OperationDropdownField.RemoveFromHierarchy();
                OperationDropdownField = null;
            }

            // Set the displayed value to the serialized value.
            int index = 0;
            if (ConditionModel.HasOperatorValue())
            {
                ConditionOperator savedOperator = ConditionModel.GetOperatorValue();
                string choice = m_AllOperatorMapping.FirstOrDefault(x => x.Value == savedOperator).Key;
                if (choice != null)
                {
                    index = choices.IndexOf(choice);
                    var mappingValue = GetOperatorMappingValueFromKey(choice);
                    RefreshCompareRelationLabel((Enum)mappingValue);   
                }
            }

            OperationDropdownField = new Dropdown();
            OperationDropdownField.bindItem = (item, i) => item.label = choices[i];
            OperationDropdownField.sourceItems = choices;
            if (choices.Count > 0 && index >= 0 && index < choices.Count)
            {
                OperationDropdownField.selectedIndex = index;
            }

            // Reset the dropdown to the first option when selection is out of the current mapping.
            if (choices.Count > 0 && OperationDropdownField.selectedIndex == -1)
            {
                OperationDropdownField.selectedIndex = 0;
                RefreshCompareRelationLabel((Enum)GetOperatorMappingValueFromKey("Equal"));
                ConditionModel.SetOperatorValue((Enum)GetOperatorMappingValueFromKey("Equal"));
            }
            
            OperationDropdownField.RegisterValueChangedCallback( evt =>
            {
                string key = choices[evt.newValue.First()];
                var newEnumValue = (Enum)GetOperatorMappingValueFromKey(key);
                ConditionModel.SetOperatorValue(newEnumValue);
                RefreshCompareRelationLabel(newEnumValue);
                ConditionModel.Asset.SetAssetDirty();
            });
            
            Add(OperationDropdownField);
            Add(m_CompareRelationLabel);
        }

        internal List<string> GetChoicesFromComparisonType()
        {
            List<string> choices = ComparisonType switch
            {
                ComparisonType.All => m_AllOperatorMapping.Keys.ToList(),
                ComparisonType.Boolean => m_BooleanOperatorMapping.Keys.ToList(),
                ComparisonType.BlackboardVariables => m_AllOperatorMapping.Keys.ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return choices;
        }

        private void RefreshCompareRelationLabel(Enum newEnumValue)
        {
            if (newEnumValue is ConditionOperator)
            {
                switch (newEnumValue)
                {
                    case ConditionOperator.Equal:
                    case ConditionOperator.NotEqual:
                        m_CompareRelationLabel.text = " to";
                        break;

                    case ConditionOperator.Greater:
                    case ConditionOperator.GreaterOrEqual:
                    case ConditionOperator.Lower:
                    case ConditionOperator.LowerOrEqual:
                        m_CompareRelationLabel.text = " than";
                        break;
                }
            }
            else
            {
                m_CompareRelationLabel.text = " to";
            }
        }

        private bool IsNumeric(Type type) => type == typeof(int) || type == typeof(float) || type == typeof(double);

        public void RefreshOperatorsFromVariable(BaseLinkField variableField)
        {
            List<string> choices = new List<string>();
            if (variableField.LinkedVariable != null)
            {
                if (IsNumeric(variableField.LinkedVariable.Type))
                {
                    choices = m_AllOperatorMapping.Keys.ToList();
                }
                else
                {
                    choices = m_BooleanOperatorMapping.Keys.ToList();
                }
            }
            
            SetupDropdownElement(choices);
            OperationDropdownField.SetEnabled(variableField.LinkedVariable != null);
        }
    }
}
#endif