using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Rotation",
        story: "Set [Transform] rotation to [Rotation]",
        category: "Action/Transform",
        description: "Sets the transform's rotation to a specific Euler rotation, with an optional duration to rotate over time.",
        id: "e17c5f777033528a9a9cb8de37abe4f8")]
    internal partial class SetRotationAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<Vector3> Rotation;
        [SerializeReference] public BlackboardVariable<float> Duration;

        [CreateProperty] protected float m_Progress = 0.0f;
        [CreateProperty] protected Quaternion m_StartRotation;
        protected Quaternion m_EndRotation;

        protected override Status OnStart()
        {
            if (Transform.Value == null)
            {
                LogFailure("No Transform set.");
                return Status.Failure;
            }

            if (Duration.Value <= 0.0f)
            {
                Transform.Value.rotation = Quaternion.Euler(Rotation.Value);
                return Status.Success;
            }

            m_StartRotation = Transform.Value.rotation;
            m_EndRotation = Quaternion.Euler(Rotation.Value);
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
            m_EndRotation = Quaternion.Euler(Rotation.Value);
        }
    }
}
