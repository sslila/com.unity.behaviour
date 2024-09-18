using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(Start))]
    internal class StartNodeModel : BehaviorGraphNodeModel
    {
        public override bool IsDuplicatable => true;
        public override bool IsRoot => true;

        public override bool HasDefaultInputPort => false;
        public override int MaxInputsAccepted => 0;

        public bool Repeat = true;
        public StartNodeModel(NodeInfo nodeInfo) : base(nodeInfo) { }
        
        protected StartNodeModel(StartNodeModel originalModel, BehaviorAuthoringGraph asset) : base(originalModel, asset)
        {
            Repeat = originalModel.Repeat;
        }

        public override void OnValidate()
        {
            foreach (FieldModel fieldModel in m_FieldValues)
            {
                if (fieldModel.FieldName == "Repeat" && fieldModel.Type.Type == typeof(bool))
                {
                    Repeat = (bool)fieldModel.LocalValue.ObjectValue;
                    break;
                }
            }
            m_FieldValues.Clear();
            base.OnValidate();
        }
    }
}
