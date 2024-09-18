using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(RepeaterModifier))]
    [NodeModelInfo(typeof(RepeatUntilFailModifier))]
    [NodeModelInfo(typeof(RepeatUntilSuccessModifier))]
    [NodeModelInfo(typeof(RepeatWhileConditionModifier))]
    internal class RepeatNodeModel : ModifierNodeModel, IConditionalNodeModel
    {
        [field: SerializeReference]
        public List<ConditionModel> ConditionModels { get; set; } = new List<ConditionModel>();

        [field: SerializeField]
        public bool RequiresAllConditionsTrue { get; set; }

        [field: SerializeField]
        public bool ShouldTruncateNodeUI { get; set; }

        [Serializable]
        public enum RepeatMode
        {
            Forever,
            UntilSuccess,
            UntilFail,
            Condition
        }

        [SerializeField]
        private RepeatMode m_RepeatMode;

        public RepeatMode Mode
        {
            get => m_RepeatMode;
            set
            {
                m_RepeatMode = value;
            }
        }

        public RepeatNodeModel(NodeInfo nodeInfo) : base(nodeInfo)
        {
        }

        protected RepeatNodeModel(RepeatNodeModel originalModel, BehaviorAuthoringGraph asset)
            : base(originalModel, asset)
        {
            Mode = originalModel.Mode;
            ConditionModels = IConditionalNodeModel.GetConditionModelCopies(originalModel, this);
            RequiresAllConditionsTrue = originalModel.RequiresAllConditionsTrue;
            ShouldTruncateNodeUI = originalModel.ShouldTruncateNodeUI;
            UpdateNodeType();
        }

        public override void OnDefineNode()
        {
            base.OnDefineNode();
            foreach (ConditionModel conditionModel in ConditionModels)
            {
                conditionModel.DefineNode();
            }
        }

        public override void OnValidate()
        {
            base.OnValidate();
            UpdateNodeType();
            
            IConditionalNodeModel.UpdateConditionModels(this);
        }

        private void UpdateNodeType()
        {
            switch (Mode)
            {
                case RepeatMode.Forever:
                    NodeType = typeof(RepeaterModifier);
                    break;

                case RepeatMode.UntilSuccess:
                    NodeType = typeof(RepeatUntilSuccessModifier);
                    break;

                case RepeatMode.UntilFail:
                    NodeType = typeof(RepeatUntilFailModifier);
                    break;

                case RepeatMode.Condition:
                    NodeType = typeof(RepeatWhileConditionModifier);
                    break;
            }
            Type type = NodeType;
            NodeDescriptionAttribute attribute = type.GetCustomAttribute<NodeDescriptionAttribute>();
            if (attribute != null)
            {
                NodeTypeID = attribute.GUID;
            }
        }
    }
}