#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(ConditionalGuardNodeModel))]
    [NodeInspectorUI(typeof(BranchingConditionNodeModel))]
    internal class ConditionalNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        private string m_ConditionRequirementLabel;
        
        public ConditionalNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }
        
        public override void Refresh()
        {
            NodeProperties.Clear();

            if (InspectedNode is not IConditionalNodeModel conditionalNode)
            {
                return;
            }

            m_ConditionRequirementLabel = InspectedNode switch
            {
                ConditionalGuardNodeModel => "Continue if",
                BranchingConditionNodeModel => "Check if",
                _ => m_ConditionRequirementLabel
            };

            NodeProperties.Add(new ConditionRequirementElement(m_ConditionRequirementLabel, conditionalNode));
            NodeProperties.Add(new ConditionInspectorElement(conditionalNode));   
            
            // Hide the truncate NodeUI setting on Conditional Guard actions.
            if (InspectedNode is ConditionalGuardNodeModel)
            {
                VisualElement truncateOptionField = this.Q<VisualElement>("TruncateOptionField");
                truncateOptionField.Hide();
            }
        }

    }
}
#endif