using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Add Force",
        story: "Apply force [ForceValue] to [Target]",
        description: "Applies physics force to target Rigidbody.",
        category: "Action/Physics",
        id: "2c889157ee1f5446e7f4fbe05ef4f0bb")]
    internal partial class AddForceAction : Action
    {
        [SerializeReference] public BlackboardVariable<Rigidbody> Target;
        [SerializeReference] public BlackboardVariable<Vector3> ForceValue;
        [SerializeReference] public BlackboardVariable<ForceMode> ForceMode;

        protected override Status OnStart()
        {
            if (Target.Value == null)
            {
                LogFailure("No target rigidbody assigned.");
                return Status.Failure;
            }

            Target.Value.AddForce(ForceValue, ForceMode.Value);
            return Status.Success;
        }

    }
}
