using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Animator Float",
        description: "Sets a float parameter on an animator to a specific value.",
        story: "Set [Parameter] in [Animator] to [Value]",
        category: "Action/Animation",
        id: "9a2ac4602f49217d4d76ded1bf16c9e1")]
    internal partial class SetAnimatorFloatAction : Action
    {
        [SerializeReference] public BlackboardVariableString Parameter;
        [SerializeReference] public BlackboardVariableAnimator Animator;
        [SerializeReference] public BlackboardVariableFloat Value;

        protected override Status OnStart()
        {
            if (Animator.Value == null)
            {
                LogFailure("No Animator set.");
                return Status.Failure;
            }

            Animator.Value.SetFloat(Parameter.Value, Value.Value);
            return Status.Success;
        }
    }
}
