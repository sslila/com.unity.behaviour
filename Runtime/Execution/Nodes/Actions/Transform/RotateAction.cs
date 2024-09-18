using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Rotate",
        story: "Rotate [Transform] by [Rotation]",
        category: "Action/Transform",
        description: "Rotates the transform by an Euler rotation, with an optional duration to rotate over time.",
        id: "10eeb1c8b93f5a9c7ae096bfeed39c62")]
    internal partial class RotateEulerAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<Vector3> Rotation;
        [SerializeReference] public BlackboardVariable<float> Duration;

        [CreateProperty] private float m_Progress;
        [CreateProperty] private Quaternion m_StartRotation;
        private Quaternion m_EndRotation;

        protected override Status OnStart()
        {
            if (Transform.Value == null)
            {
                LogFailure("No Target set.");
                return Status.Failure;
            }

            if (Duration.Value <= 0.0f)
            {
                Transform.Value.rotation = Quaternion.Euler(m_StartRotation.eulerAngles + Rotation.Value);
                return Status.Success;
            }

            m_StartRotation = Transform.Value.rotation;
            m_EndRotation = Quaternion.Euler(m_StartRotation.eulerAngles + Rotation.Value);
            m_Progress = 0.0f;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float normalizedProgress = Mathf.Min(m_Progress / Duration.Value, 1f);
            Transform.Value.rotation = Quaternion.Lerp(m_StartRotation, m_EndRotation, normalizedProgress);
            m_Progress += Time.deltaTime;

            return normalizedProgress == 1 ? Status.Success : Status.Running;
        }

        protected override void OnDeserialize()
        {
            // Only target to reduce serialization size.
            m_EndRotation = Quaternion.Euler(m_StartRotation.eulerAngles + Rotation.Value);
        }
    }
}
