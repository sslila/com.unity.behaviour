using System;
using UnityEngine;
using Unity.Behavior;
using Unity.Properties;
using Modifier = Unity.Behavior.Modifier;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Cooldown",
    description: "Imposes a mandatory wait time between executions to regulate action frequency.",
    story: "Cooldowns for [duration] seconds after execution",
    category: "Flow",
    id: "a9ff45a058927aa68b4328a5daf34161")]
internal partial class CooldownModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<float> Duration;
    [CreateProperty] private float m_CooldownRemainingTime;

    protected override Status OnStart()
    {
        if (m_CooldownRemainingTime > 0)
        {
            return Status.Failure;
        }

        m_CooldownRemainingTime = Duration.Value;

        if (Child == null)
        {
            return Status.Success;
        }

        var status = StartNode(Child);
        if (status == Status.Running)
        {
            return Status.Waiting;
        }
        
        return status;
    }

    protected override Status OnUpdate()
    {
        if (m_CooldownRemainingTime > 0)
        {
            m_CooldownRemainingTime -= Time.deltaTime;
        }

        var status = Child.CurrentStatus;
        if (status == Status.Running)
        {
            return Status.Waiting;
        }

        return status;
    }

    protected override void OnEnd()
    {
        m_CooldownRemainingTime = 0;
    }
}

