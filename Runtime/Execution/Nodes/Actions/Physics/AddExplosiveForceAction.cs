using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Add Explosive Force",
        story: "Apply explosive force to [Target]",
        category: "Action/Physics",
        description: "Applies a force that simulates explosion effects to the target RigidBody.\n\n" +
        "Note: ExplosionOffset is an offset from the position of the ExplosionOrigin object. If the ExplosionOrigin is unset, ExplosionOffset will be a world position. You can set ExplosionOrigin to the same Target to make it relative to your target.",
        id: "60a80a024024618ad0eaca18c58d15a8")]
    internal partial class AddExplosiveForceAction : Action
    {
        [SerializeReference] public BlackboardVariable<Rigidbody> Target;
        [SerializeReference] public BlackboardVariable<Transform> ExplosionOrigin;
        [SerializeReference] public BlackboardVariable<Vector3> ExplosionOffset;
        [SerializeReference] public BlackboardVariable<float> ExplosionForce;
        [SerializeReference] public BlackboardVariable<float> ExplosionRadius;
        [SerializeReference] public BlackboardVariable<float> ExplosionUpModifier;
        [SerializeReference] public BlackboardVariable<ForceMode> ForceMode;

        protected override Status OnStart()
        {
            if (Target.Value == null)
            {
                LogFailure("No target rigidbody assigned.");
                return Status.Failure;
            }

            Vector3 explosionPosition = ExplosionOffset.Value;
            if (ExplosionOrigin.Value != null)
            {
                explosionPosition += ExplosionOrigin.Value.position;
            }

            Target.Value.AddExplosionForce(ExplosionForce, explosionPosition, ExplosionRadius, ExplosionUpModifier, ForceMode);
            return Status.Success;
        }
    }
}
