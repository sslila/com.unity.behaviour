using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Position",
        story: "Set [Transform] position to [Location]",
        category: "Action/Transform",
        description: "Sets the target's position to a specific location.",
        id: "4e73d3459af87461a8891aa77fe75006")]
    internal partial class SetPositionAction : Action
    {
        [SerializeReference] public BlackboardVariable<Transform> Transform;
        [SerializeReference] public BlackboardVariable<Vector3> Location;
        [Tooltip("Should the action be performed on transform.localPosition instead of transform.position?")]
        [SerializeReference] public BlackboardVariable<bool> UseLocal = new BlackboardVariable<bool>(false);
        [SerializeReference] public BlackboardVariable<float> Duration;
        [Tooltip("Use Spherical linear interpolation (Slerp) instead of linear (Lerp).")]
        [SerializeReference] public BlackboardVariable<bool> UseSlerp = new BlackboardVariable<bool>(false);

        [CreateProperty] private float m_Progress = 0.0f;
        [CreateProperty] private Vector3 m_Origin;
        private Vector3 m_Destination;

        protected override Status OnStart()
        {
            if (Transform.Value == null)
            {
                LogFailure("No Transform provided.");
                return Status.Failure;
            }

            if (Duration.Value <= 0.0f)
            {
                if (UseLocal.Value)
                {
                    Transform.Value.position = Location.Value;
                }
                else
                {
                    Transform.Value.localPosition = Location.Value;
                }

                return Status.Success;
            }

            m_Origin = UseLocal.Value ? Transform.Value.localPosition : Transform.Value.position;
            m_Destination = Location.Value;
            m_Progress = 0;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            float normalizedProgress = Mathf.Min(m_Progress / Duration.Value, 1f);

            var newLocation = UseSlerp.Value ?
                Vector3.Slerp(m_Origin, m_Destination, normalizedProgress)
                : Vector3.Lerp(m_Origin, m_Destination, normalizedProgress);

            if (UseLocal.Value)
            {
                Transform.Value.localPosition = newLocation;
            }
            else
            {
                Transform.Value.position = newLocation;
            }

            m_Progress += Time.deltaTime;

            return normalizedProgress == 1 ? Status.Success : Status.Running;
        }

        protected override void OnDeserialize()
        {
            // Only target to reduce serialization size.
            m_Destination = Location.Value;
        }
    }
}
