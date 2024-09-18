using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(AbortModifier))]
    [NodeModelInfo(typeof(RestartModifier))]
    internal class AbortNodeModel : ModifierNodeModel, IConditionalNodeModel
    {
        public AbortType ModelAbortType { get => m_ModelAbortType; set => m_ModelAbortType = value; }
        [SerializeField]
        private AbortType m_ModelAbortType;

        internal enum AbortType
        {
            Abort,
            Restart
        }

        [field: SerializeReference]
        public List<ConditionModel> ConditionModels { get; set; } = new List<ConditionModel>();

        [field: SerializeField]
        public bool RequiresAllConditionsTrue { get; set; }

        [field: SerializeField]
        public bool ShouldTruncateNodeUI { get; set; }

        public AbortNodeModel(NodeInfo nodeInfo) : base(nodeInfo)
        {
            ModelAbortType = typeof(RestartModifier).IsAssignableFrom(NodeType) ? AbortType.Restart : AbortType.Abort;
        }

        protected AbortNodeModel(AbortNodeModel originalModel, BehaviorAuthoringGraph asset) : base(
            originalModel, asset)
        {
            ConditionModels = IConditionalNodeModel.GetConditionModelCopies(originalModel, this);
            ModelAbortType = originalModel.ModelAbortType;
            RequiresAllConditionsTrue = originalModel.RequiresAllConditionsTrue;
            ShouldTruncateNodeUI = originalModel.ShouldTruncateNodeUI;
        }

        public override void OnValidate()
        {
            base.OnValidate();
            UpdateNodeType();
            
            IConditionalNodeModel.UpdateConditionModels(this);
        }

        public override void OnDefineNode()
        {
            base.OnDefineNode();
            foreach (ConditionModel conditionModel in ConditionModels)
            {
                conditionModel.DefineNode();
            }
        }

        private void UpdateNodeType()
        {
            NodeType = ModelAbortType == AbortType.Abort ? typeof(AbortModifier) : typeof(RestartModifier);
            Type type = NodeType;
            NodeDescriptionAttribute attribute = type.GetCustomAttribute<NodeDescriptionAttribute>();
            if (attribute != null)
            {
                NodeTypeID = attribute.GUID;
            }
        }
    }
}