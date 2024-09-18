using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Scale",
        story: "Scale [Transform] by [ScaleValue]",
        category: "Action/Transform",
        description: "Scales the transform by a value, with an optional duration to scale over time.",
        id: "d06901c11b6e0fb28fd06643e5d6314a")]
    internal partial class ScaleAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<Vector3> ScaleValue = new BlackboardVariable<Vector3>(Vector3.one);
        [SerializeReference] public BlackboardVariable<float> Duration;

        [CreateProperty] private float m_Progress = 0.0f;
        [CreateProperty] private Vector3 m_StartScale;
        private Vector3 m_EndScale;

        protected override Status OnStart()
        {
            if (Transform.Value == null)
            {
                LogFailure("No Transform set.");
                return Status.Failure;
            }

            if (Duration.Value <= 0.0f)
            {
                Transform.Value.localScale = Vector3.Scale(Transform.Value.localScale, ScaleValue.Value);
                return Status.Success;
            }

            m_StartScale = Transform.Value.localScale;
            m_EndScale = Vector3.Scale(m_StartScale, ScaleValue.Value);
            m_Progress = 0.0f;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float normalizedProgress = Mathf.Min(m_Progress / Duration.Value, 1f);
            Transform.Value.localScale = Vector3.Lerp(m_StartScale, m_EndScale, normalizedProgress);
            m_Progress += Time.deltaTime;

            return normalizedProgress == 1 ? Status.Success : Status.Running;
        }

        protected override void OnDeserialize()
        {
            // Only target to reduce serialization size.
            m_EndScale = Vector3.Scale(m_StartScale, ScaleValue.Value);
        }
    }
}
