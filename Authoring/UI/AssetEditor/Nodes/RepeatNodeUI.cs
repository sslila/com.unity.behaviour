#if UNITY_EDITOR
using UnityEngine;
using Unity.Behavior.GraphFramework;
using System;

namespace Unity.Behavior
{
    [NodeUI(typeof(RepeatNodeModel))]
    internal class RepeatNodeUI : ConditionalNodeUI
    {
        RepeatNodeModel RepeatModel => Model as RepeatNodeModel;
        public RepeatNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Modifier");
            UpdateConditionVisuals();
            CreateNodeConditionElements();

            UpdateNodeTitle();
        }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);
            UpdateNodeTitle();
            UpdateConditionVisuals();
        }

        private void UpdateNodeTitle()
        {
            NodeInfo info = NodeRegistry.GetInfo(RepeatModel.NodeType);
            if (info != null)
            {
                Title = info.Name;

                if (RepeatModel.ConditionModels.Count > 1 && RepeatModel.Mode == RepeatNodeModel.RepeatMode.Condition)
                {
                    string titleString = !RepeatModel.RequiresAllConditionsTrue ? " Any Are True" : " All Are True";
                    Title += titleString;
                }
            }
        }

        private void UpdateConditionVisuals()
        {
            bool isRepeatWhileNode = RepeatModel.NodeType.Type == typeof(RepeatWhileConditionModifier);
            EnableInClassList("TwoLineNode", isRepeatWhileNode);
            if (!isRepeatWhileNode)
            {
                return;
            }
        }
    }
}
#endif