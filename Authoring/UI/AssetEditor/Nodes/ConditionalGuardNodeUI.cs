#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(ConditionalGuardNodeModel))]
    internal class ConditionalGuardNodeUI : ConditionalNodeUI
    {
        public ConditionalGuardNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Action");
            AddToClassList("Condition");
            AddToClassList("ShowNodeColor");
            
            UpdateGuardConditionElement();
            CreateNodeConditionElements();

            NodeValueContainer.style.display = DisplayStyle.Flex;
        }
        
        internal override void UpdateLinkFields()
        {
            UpdateGuardConditionElement();
            base.UpdateLinkFields();
        }

        private void UpdateGuardConditionElement()
        {
            if (ConditionalNodeModel.ConditionModels.Count > 1)
            {
                ConditionalNodeModel.ShouldTruncateNodeUI = true;
                ConditionElementPrefix = ConditionalNodeModel.RequiresAllConditionsTrue ? "If all of" : "If any of";   
            }
            else
            {
                ConditionalNodeModel.ShouldTruncateNodeUI = false;
                ConditionElementPrefix = "If";
            }
            
            ConditionsAssignedPostfix = " Conditions are true";
        }
    }
}
#endif