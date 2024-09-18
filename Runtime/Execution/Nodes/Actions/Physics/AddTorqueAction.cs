using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Add Torque",
        story: "Apply torque [TorqueValue] to [Target]",
        category: "Action/Physics",
        description: "Applies torque to the target Rigidbody.",
        id: "eeae23a7831be63cc6c7b0f007e5d5f2")]
    internal partial class AddTorqueAction : Action
    {
        [SerializeReference] public BlackboardVariable<Rigidbody> Target;
        [SerializeReference] public BlackboardVariable<Vector3> TorqueValue;
        [SerializeReference] public BlackboardVariable<ForceMode> ForceMode;

        protected override Status OnStart()
        {
            if (Target.Value == null)
            {
                LogFailure("No target rigidbody assigned.");
                return Status.Failure;
            }

            Target.Value.AddTorque(TorqueValue, ForceMode);
            return Status.Success;
        }
    }
}