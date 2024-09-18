using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Velocity",
        story: "Set [Target] 's velocity to [Velocity]",
        category: "Action/Physics",
        description: "Sets the linear velocity of the target Rigidbody to a specific value. " +
            "Linear velocity represents the rate of change of the GameObject's position.\n\n" +
        "This requires a RigidBody on the target.",
        id: "7e2ce2d5615c5bbf085d783baf85a85f")]
    internal partial class SetVelocityAction : Action
    {
        [SerializeReference] public BlackboardVariable<Rigidbody> Target;
        [SerializeReference] public BlackboardVariable<Vector3> Velocity;

        protected override Status OnStart()
        {
            if (Target.Value == null)
            {
                LogFailure("No target rigidbody assigned.");
                return Status.Failure;
            }

            Target.Value.velocity = Velocity.Value;
            return Status.Success;
        }
    }
}
