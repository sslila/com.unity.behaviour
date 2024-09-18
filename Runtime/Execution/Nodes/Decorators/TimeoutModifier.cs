using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Time Out", 
        description: "Terminates the execution of its branch after a specified number of seconds.", 
        story: "Time out after [Duration] seconds", 
        category: "Flow",
        id: "a6394ca78d6f93415c22fb52c9d5577c")]
    internal partial class TimeOutModifier : Modifier
    {
        [SerializeReference] public BlackboardVariable<float> Duration;
        [CreateProperty] private float m_Timer = 0.0f;

        protected override Status OnStart()
        {
            if (Child == null)
            {
                LogFailure("No child node to timeout for.");
                return Status.Failure;
            }

            m_Timer = Duration;
            if (m_Timer <= 0.0f)
            {
                LogFailure("Duration set to zero. Child was not executed.");
                return Status.Failure;
            }

            return StartNode(Child) switch
            {
                Status.Success => Status.Success,
                Status.Failure => Status.Failure,
                _ => Status.Running
            };
        }

        protected override Status OnUpdate()
        {
            m_Timer -= Time.deltaTime;
            if (m_Timer <= 0)
            {
                EndNode(Child);
                return Status.Failure;
            }

            return Child.CurrentStatus switch
            {
                Status.Success => Status.Success,
                Status.Failure => Status.Failure,
                _ => Status.Running
            };
        }
    }
}
