using UnityEngine.AI;

namespace Unity.Behavior
{
    internal static class NavMeshAgentExtensions
    {
        internal static bool IsNavigationComplete(this NavMeshAgent agent)
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        }
    }
}