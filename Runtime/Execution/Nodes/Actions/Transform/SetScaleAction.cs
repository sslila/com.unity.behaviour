using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Scale",
        story: "Set [Transform] scale to [ScaleValue]",
        category: "Action/Transform",
        description: "Sets the transform's scale to a specific value, with an optional duration to scale over time.",
        id: "31e76f9a2f18ec898c84367943358f2b")]
    internal partial class SetScaleAction : Action
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
                Transform.Value.localScale = ScaleValue.Value;
                return Status.Success;
            }

            m_StartScale = Transform.Value.localScale;
            m_EndScale = ScaleValue.Value;
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
            m_EndScale = ScaleValue.Value;
        }
    }
}
