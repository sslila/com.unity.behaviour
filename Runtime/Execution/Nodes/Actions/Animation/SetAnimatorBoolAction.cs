using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Animator Boolean",
        description: "Sets a boolean parameter on an animator to a specific value.",
        story: "Set [Parameter] in [Animator] to [Value]",
        category: "Action/Animation",
        id: "71645526fca57d73bb57ec1de091381a")]
    internal partial class SetAnimatorBoolAction : Action
    {
        [SerializeReference] public BlackboardVariableString Parameter;
        [SerializeReference] public BlackboardVariableAnimator Animator;
        [SerializeReference] public BlackboardVariableBool Value;

        protected override Status OnStart()
        {
            if (Animator.Value == null)
            {
                LogFailure("No Animator set.");
                return Status.Failure;
            }

            Animator.Value.SetBool(Parameter.Value, Value.Value);
            return Status.Success;
        }
    }
}
