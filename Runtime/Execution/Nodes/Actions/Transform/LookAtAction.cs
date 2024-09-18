using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Look At",
        description: "Rotates the Transform to look at the Target.",
        story: "[Transform] looks at [Target]",
        category: "Action/Transform",
        id: "64086e5d43aae3c075dab693be8ecdc0")]
    internal partial class LookAtAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [Tooltip("True: the node process the LookAt every update with status Running." +
            "\nFalse: the node process the LookAt only once.")]
        [SerializeReference] public BlackboardVariable<bool> Continuous = new BlackboardVariable<bool>(false);
        [SerializeReference] public BlackboardVariable<bool> LimitToYAxis = new BlackboardVariable<bool>(false);

        protected override Status OnStart()
        {
            if (Transform.Value == null || Target.Value == null)
            {
                LogFailure($"Missing Transform or Target.");
                return Status.Failure;
            }

            ProcessLookAt();
            return Continuous.Value ? Status.Running : Status.Success;
        }

        protected override Status OnUpdate()
        {
            if (Continuous.Value)
            {
                ProcessLookAt();
                return Status.Running;
            }
            return Status.Success;
        }

        void ProcessLookAt()
        {
            Vector3 targetPosition = Target.Value.position;

            if (LimitToYAxis.Value)
            {
                targetPosition.y = Transform.Value.position.y;
            }
            Transform.Value.LookAt(targetPosition, Transform.Value.up);
        }
    }
}
