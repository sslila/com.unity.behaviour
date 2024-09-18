using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Navigate To Location",
        description: "Navigates a GameObject to a specified position using NavMeshAgent." +
        "\nIf NavMeshAgent is not available on the [Agent] or its children, moves the Agent using its transform.",
        story: "[Agent] navigates to [Location]",
        category: "Action/Navigation",
        id: "c67c5c55de9fe94897cf61976250cc83")]
    internal partial class NavigateToLocationAction : Action
    {
        [SerializeReference] public BlackboardVariableGameObject Agent;
        [SerializeReference] public BlackboardVariableVector3 Location;
        [SerializeReference] public BlackboardVariableFloat Speed = new BlackboardVariableFloat(1.0f);
        [SerializeReference] public BlackboardVariableFloat DistanceThreshold = new BlackboardVariableFloat(0.2f);
        [SerializeReference] public BlackboardVariableString AnimatorSpeedParam = new BlackboardVariableString("SpeedMagnitude");

        // This will only be used in movement without a navigation agent.
        [SerializeReference] public BlackboardVariable<float> SlowDownDistance = new BlackboardVariable<float>(1.0f);

        private float m_PreviousStoppingDistance;
        private NavMeshAgent m_NavMeshAgent;
        private Animator m_Animator;

        protected override Status OnStart()
        {
            if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Location?.Value, null))
            {
                return Status.Failure;
            }

            return Initialize();
        }

        protected override Status OnUpdate()
        {
            if (ReferenceEquals(Agent?.Value, null) || ReferenceEquals(Location, null))
            {
                return Status.Failure;
            }

            if (m_NavMeshAgent == null)
            {
                Vector3 agentPosition, locationPosition;
                float distance = GetDistanceToLocation(out agentPosition, out locationPosition);
                if (distance <= DistanceThreshold)
                {
                    return Status.Success;
                }

                float speed = Speed;

                if (SlowDownDistance > 0.0f && distance < SlowDownDistance)
                {
                    float ratio = distance / SlowDownDistance;
                    speed = Mathf.Max(0.1f, Speed * ratio);
                }

                Vector3 toDestination = locationPosition - agentPosition;
                toDestination.y = 0.0f;
                toDestination.Normalize();
                agentPosition += toDestination * (speed * Time.deltaTime);
                Agent.Value.transform.position = agentPosition;

                // Look at the target.
                Agent.Value.transform.forward = toDestination;
            }
            else if (m_NavMeshAgent.IsNavigationComplete())
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (m_Animator != null)
            {
                m_Animator.SetFloat(AnimatorSpeedParam, 0);
            }

            if (m_NavMeshAgent != null)
            {
                if (m_NavMeshAgent.isOnNavMesh)
                {
                    m_NavMeshAgent.ResetPath();
                }
                m_NavMeshAgent.stoppingDistance = m_PreviousStoppingDistance;
            }

            m_NavMeshAgent = null;
            m_Animator = null;
        }

        protected override void OnDeserialize()
        {
            Initialize();
        }

        private Status Initialize()
        {
            if (GetDistanceToLocation(out Vector3 agentPosition, out Vector3 locationPosition) <= DistanceThreshold)
            {
                return Status.Failure;
            }

            // If using animator, set speed parameter.
            m_Animator = Agent.Value.GetComponentInChildren<Animator>();
            if (m_Animator != null)
            {
                m_Animator.SetFloat(AnimatorSpeedParam, Speed);
            }

            // If using a navigation mesh, set target position for navigation mesh agent.
            m_NavMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
            if (m_NavMeshAgent != null)
            {
                if (m_NavMeshAgent.isOnNavMesh)
                {
                    m_NavMeshAgent.ResetPath();
                }
                m_NavMeshAgent.speed = Speed;
                m_PreviousStoppingDistance = m_NavMeshAgent.stoppingDistance;
                m_NavMeshAgent.stoppingDistance = DistanceThreshold;
                m_NavMeshAgent.SetDestination(locationPosition);
            }

            return Status.Running;
        }

        private float GetDistanceToLocation(out Vector3 agentPosition, out Vector3 locationPosition)
        {
            agentPosition = Agent.Value.transform.position;
            locationPosition = Location.Value;
            return Vector3.Distance(new Vector3(agentPosition.x, locationPosition.y, agentPosition.z), locationPosition);
        }
    }
}
