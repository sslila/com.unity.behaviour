using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(BranchingConditionComposite))]
    internal class BranchingConditionNodeModel : CompositeNodeModel, IConditionalNodeModel
    {
        [field: SerializeField]
        public List<ConditionModel> ConditionModels { get; set; } = new List<ConditionModel>();

        [field: SerializeField]
        public bool RequiresAllConditionsTrue { get; set; }

        [field: SerializeField]
        public bool ShouldTruncateNodeUI { get; set; }

        public BranchingConditionNodeModel(NodeInfo nodeInfo) : base(nodeInfo)
        {
        }

        protected BranchingConditionNodeModel(BranchingConditionNodeModel originalModel, BehaviorAuthoringGraph asset) : base(
            originalModel, asset)
        {
            ConditionModels = IConditionalNodeModel.GetConditionModelCopies(originalModel, this);
            RequiresAllConditionsTrue = originalModel.RequiresAllConditionsTrue;
            ShouldTruncateNodeUI = originalModel.ShouldTruncateNodeUI;
            OnValidate();
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