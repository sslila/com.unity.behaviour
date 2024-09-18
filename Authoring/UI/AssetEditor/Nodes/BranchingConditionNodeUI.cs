#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [NodeUI(typeof(BranchingConditionNodeModel))]
    internal class BranchingConditionNodeUI : ConditionalNodeUI
    {
        private const string k_TruePortTitle = "True";
        private const string k_FalsePortTitle = "False";
        private const string k_MultiConditionAllTruePortTitle = "If All Are True";
        private const string k_MultiConditionAnyFalsePortTitle = "If Any Is False";
        private const string k_MultiConditionAnyTruePortTitle = "If Any Is True";
        private const string k_MultiConditionAllFalsePortTitle = "If All Are False";
        
        public BranchingConditionNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Composite");
            AddToClassList("Condition");
            AddToClassList("TwoLineNode");

            Title = "Branch on";
            CreateNodeConditionElements();
        }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);
            UpdatePortTitles();
        }

        private void UpdatePortTitles()
        {
            List<NodeUI> nodePortUIs = GetChildNodeUIs().ToList();
            if (nodePortUIs.Count != 2)
            {
                return;
            }
            NodeUI truePortUI = nodePortUIs[0];
            NodeUI falsePortUI = nodePortUIs[1];

            if (ConditionalNodeModel.ConditionModels.Count <= 1)
            {
                truePortUI.Title = k_TruePortTitle;
                falsePortUI.Title = k_FalsePortTitle;
            }
            else
            {
                if (ConditionalNodeModel.RequiresAllConditionsTrue)
                {
                    truePortUI.Title = k_MultiConditionAllTruePortTitle;
                    falsePortUI.Title = k_MultiConditionAnyFalsePortTitle;
                }
                else
                {
                    truePortUI.Title = k_MultiConditionAnyTruePortTitle;
                    falsePortUI.Title = k_MultiConditionAllFalsePortTitle;
                }
            }
        }
    }
}
#endif