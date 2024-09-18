#if UNITY_PHYSICS_2D
using System;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Trigger 2D",
        story: "Wait for Trigger [MessageType] 2D on [Agent]",
        description: "Waits for an OnTrigger event 2D on the specified agent.",
        category: "Action/Physics",
        id: "a813ee3b7029c3cb9f45717e3bae2995")]
    internal partial class WaitForTrigger2DAction : WaitForPhysicsMessageAction
    {
        protected override void RegisterEvents()
        {
            switch (MessageType.Value)
            {
                case EMessageType.Enter:
                    m_CollisionEvents.OnTriggerEnterEvent2D += HandleCollision;
                    break;
                case EMessageType.Stay:
                    m_CollisionEvents.OnTriggerStayEvent2D += HandleCollision;
                    break;
                case EMessageType.Exit:
                    m_CollisionEvents.OnTriggerExitEvent2D += HandleCollision;
                    break;
            }
        }

        protected override void UnregisterEvents()
        {
            if (m_CollisionEvents == null)
            {
                return;
            }

            m_CollisionEvents.OnTriggerEnterEvent2D -= HandleCollision;
            m_CollisionEvents.OnTriggerExitEvent2D -= HandleCollision;
            m_CollisionEvents.OnTriggerStayEvent2D -= HandleCollision;
        }
    }
}
#endif
