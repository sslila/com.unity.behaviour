#if UNITY_EDITOR
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ConditionalNodeUI : BehaviorNodeUI
    {
        private VisualElement m_ConditionFieldContainer;
        internal IConditionalNodeModel ConditionalNodeModel => Model as IConditionalNodeModel;

        private const string k_NoConditionAssignedText = "No Condition Assigned";
        private const string k_ConditionAssignedPostfix = " Condition";
        protected string ConditionsAssignedPostfix = " Conditions";
        protected string ConditionElementPrefix;

        protected ConditionalNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/ConditionNodeStylesheet.uss"));
            CreateNodeConditionElements();
        }

        internal override void UpdateLinkFields()
        {
            CreateNodeConditionElements();
            
            // Refresh the fields.
            this.Query<BaseLinkField>().ForEach(field =>
            {
                if (field.userData is ConditionModel condition)
                {
                    field.UpdateValue(condition.GetVariableLink(field.FieldName, field.LinkVariableType));   
                }
            });
            this.Query<ComparisonConditionElement>().ForEach(comparisonElement =>
            {
                if (comparisonElement.Attribute != null)
                {
                    StoryElementUtility.LinkComparisonElementFields(comparisonElement, comparisonElement.parent);   
                }
            });
        }

        protected void CreateNodeConditionElements()
        {
            NodeValueContainer.Clear();
            
            if (m_ConditionFieldContainer == null)
            {
                m_ConditionFieldContainer = new VisualElement();
                m_ConditionFieldContainer.name = "ConditionFieldContainer";
            }
            else
            {
                m_ConditionFieldContainer.Clear();
            }
            if (ConditionalNodeModel.ConditionModels == null || !ConditionalNodeModel.ConditionModels.Any())
            {
                m_ConditionFieldContainer.Add(new Label(k_NoConditionAssignedText));
            }
            else if (ConditionalNodeModel.ShouldTruncateNodeUI)
            {
                CreateTruncatedNodeUI(ConditionalNodeModel);
            }
            else
            {
                CreateConditionFieldElements(ConditionalNodeModel, m_ConditionFieldContainer);
            }

            NodeValueContainer.Add(m_ConditionFieldContainer);
        }

        private void CreateTruncatedNodeUI(IConditionalNodeModel model)
        {
            if (!string.IsNullOrEmpty(ConditionElementPrefix))
            {
                switch (model.ConditionModels.Count)
                {
                    case > 1:
                        m_ConditionFieldContainer.Add(new Label(ConditionElementPrefix + " " + model.ConditionModels.Count + ConditionsAssignedPostfix));
                        break;
                    default:
                        m_ConditionFieldContainer.Add(new Label(ConditionElementPrefix + " " + model.ConditionModels.Count + k_ConditionAssignedPostfix));
                        break;
                }
            }
            else
            {
                switch (model.ConditionModels.Count)
                {
                    case > 1:
                        m_ConditionFieldContainer.Add(new Label(model.ConditionModels.Count + ConditionsAssignedPostfix));
                        break;
                    default:
                        m_ConditionFieldContainer.Add(new Label(model.ConditionModels.Count + k_ConditionAssignedPostfix));
                        break;
                }
            }
        }

        private void CreateConditionFieldElements(IConditionalNodeModel model, VisualElement container)
        {
            for (int index = 0; index < model.ConditionModels.Count; index++)
            {
                ConditionModel condition = model.ConditionModels[index];
                NodeConditionElement element = new NodeConditionElement(condition);
                if (index != 0)
                {
                    element.AddToClassList("ConditionElementMargin");  
                }
                // Add a prefix to the condition element if one is assigned.
                if (!string.IsNullOrEmpty(ConditionElementPrefix))
                {
                    element.Insert(0, new Label(ConditionElementPrefix));
                }
                
                container.Add(element);
            }   
        }
        
    }
}
#endif