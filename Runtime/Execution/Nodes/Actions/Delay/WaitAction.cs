using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait",
        description: "Waits for a specified number of seconds.",
        story: "Wait for [SecondsToWait] seconds",
        category: "Action/Delay",
        id: "0d783cab60fd446bb4d768334502687a")]
    internal partial class WaitAction : Action
    {
        [SerializeReference] public BlackboardVariableFloat SecondsToWait;
        [CreateProperty] private float m_Timer = 0.0f;

        protected override Status OnStart()
        {
            m_Timer = SecondsToWait;
            if (m_Timer <= 0.0f)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            m_Timer -= Time.deltaTime;
            if (m_Timer <= 0)
            {
                return Status.Success;
            }

            return Status.Running;
        }
    }
}
