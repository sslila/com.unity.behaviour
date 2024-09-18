using System;
using Unity.Behavior.GenericSerializeReference;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait (Range)",
        description: "Waits for the duration specified between Min and Max value. Both Min and Max are inclusive.",
        story: "Wait between [Min] and [Max] seconds",
        category: "Action/Delay",
        id: "bafd6d03f579d512ad4d1ce6e66725d9")]
    internal partial class WaitRangeAction : Action
    {
        [SerializeReference] public BlackboardVariableFloat Min = new BlackboardVariableFloat(1);
        [SerializeReference] public BlackboardVariableFloat Max = new BlackboardVariableFloat(3);
        [CreateProperty] private float m_Timer = 0.0f;

        protected override Status OnStart()
        {
            m_Timer = UnityEngine.Random.Range(Min, Max);
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
