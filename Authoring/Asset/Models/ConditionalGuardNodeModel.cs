using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(ConditionalGuardAction))]
    internal class ConditionalGuardNodeModel : BehaviorGraphNodeModel, IConditionalNodeModel
    {
        public override bool IsSequenceable => true;

        [field: SerializeReference]
        public List<ConditionModel> ConditionModels { get; set; } = new List<ConditionModel>();

        [field: SerializeField]
        public bool RequiresAllConditionsTrue { get; set; }

        [field: SerializeField]
        public bool ShouldTruncateNodeUI { get; set; }

        public ConditionalGuardNodeModel(NodeInfo nodeInfo) : base(nodeInfo)
        {
        }

        protected ConditionalGuardNodeModel(ConditionalGuardNodeModel originalModel, BehaviorAuthoringGraph asset) : base(
            originalModel, asset)
        {
            ConditionModels = IConditionalNodeModel.GetConditionModelCopies(originalModel, this);
            RequiresAllConditionsTrue = originalModel.RequiresAllConditionsTrue;
            ShouldTruncateNodeUI = originalModel.ShouldTruncateNodeUI;
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

            IConditionalNodeModel.UpdateConditionModels(this);
        }
    }
}