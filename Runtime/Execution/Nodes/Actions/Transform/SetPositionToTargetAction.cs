using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Position To Target",
        story: "Set [Transform] position to [Target]",
        category: "Action/Transform",
        description: "Sets the transform's position to a specific target position.",
        id: "da27dccea336068ee53d4331b60fbf7e")]
    internal partial class SetPositionToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<Transform> Target;
        [SerializeReference] public BlackboardVariable<float> Duration;
        [Tooltip("Use Spherical linear interpolation (Slerp) instead of linear (Lerp).")]
        [SerializeReference] public BlackboardVariable<bool> UseSlerp = new BlackboardVariable<bool>(false);

        [CreateProperty] private float m_Progress = 0.0f;
        [CreateProperty] private Vector3 m_Origin;
        private Vector3 m_Destination;

        protected override Status OnStart()
        {
            if (Transform.Value == null || Target.Value == null)
            {
                LogFailure("No Transform or Target set.");
                return Status.Failure;
            }

            if (Duration.Value <= 0.0f)
            {
                Transform.Value.position = Target.Value.position;
                return Status.Success;
            }

            m_Origin = Transform.Value.position;
            m_Destination = Target.Value.position;
            m_Progress = 0;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float normalizedProgress = Mathf.Min(m_Progress / Duration.Value, 1f);
            Transform.Value.position = UseSlerp.Value ?
                Vector3.Slerp(m_Origin, m_Destination, normalizedProgress)
                : Vector3.Lerp(m_Origin, m_Destination, normalizedProgress);

            m_Progress += Time.deltaTime;
            return normalizedProgress == 1 ? Status.Success : Status.Running;
        }

        protected override void OnDeserialize()
        {
            // Only target to reduce serialization size.
            m_Destination = Target.Value.position;
        }
    }
}
