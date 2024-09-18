using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Variable Value",
        description: "Sets the value of a given variable.",
        story: "Set [Variable] value to [Value]",
        category: "Action/Blackboard",
        id: "3cf856343a2c414c2cfd1083d30c24fa")]
    internal partial class SetVariableValueAction : Action
    {
        [SerializeReference] public BlackboardVariable Variable;
        [SerializeReference] public BlackboardVariable Value;

        protected override Status OnStart()
        {
            if (Variable == null || Value == null)
            {
                return Status.Failure;
            }
            Variable.ObjectValue = Value.ObjectValue;
            return Status.Success;
        }
    }
}
