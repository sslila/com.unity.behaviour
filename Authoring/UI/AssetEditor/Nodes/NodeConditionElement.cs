#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class NodeConditionElement : VisualElement
    {
        private ConditionModel m_Model;

        public NodeConditionElement(ConditionModel conditionModel)
        {
            m_Model = conditionModel;
            if (m_Model != null)
            {
                ConditionInfo info = ConditionUtility.GetInfoForConditionType(m_Model.ConditionType);
                CreateFields(info);   
            }
        }
        
        internal void InitFromConditionInfo(ConditionInfo info)
        {
            CreateFields(info);
        }

        private void CreateFields(ConditionInfo conditionInfo)
        {
            tooltip = conditionInfo.Name;
            Clear();
            
            StoryElementUtility.CreateStoryElement(conditionInfo.Story , conditionInfo.Variables, this, (variableName, type) =>
            {
                BaseLinkField field = LinkFieldUtility.CreateConditionLinkField(variableName, type, m_Model);
                field.FieldName = variableName;
                field.Model = m_Model;
                return field;
            }, (fieldName, comparisonEnum) => new ComparisonConditionElement(m_Model, fieldName, comparisonEnum));   
        }
    }
}
#endif