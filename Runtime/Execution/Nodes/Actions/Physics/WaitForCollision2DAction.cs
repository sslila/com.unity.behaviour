#if UNITY_PHYSICS_2D
using System;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Collision 2D",
        story: "Wait for Collision [MessageType] 2D on [Agent]",
        description: "Waits for an OnCollision event 2D on the specified agent.",
        category: "Action/Physics",
        id: "052fe53d512e3efb9caf40020014227c")]
    internal partial class WaitForCollision2DAction : WaitForPhysicsMessageAction
    {
        protected override void RegisterEvents()
        {
            switch (MessageType.Value)
            {
                case EMessageType.Enter:
                    m_CollisionEvents.OnCollisionEnterEvent2D += HandleCollision;
                    break;
                case EMessageType.Stay:
                    m_CollisionEvents.OnCollisionStayEvent2D += HandleCollision;
                    break;
                case EMessageType.Exit:
                    m_CollisionEvents.OnCollisionExitEvent2D += HandleCollision;
                    break;
            }
        }

        protected override void UnregisterEvents()
        {
            if (m_CollisionEvents == null)
            {
                return;
            }

            m_CollisionEvents.OnCollisionEnterEvent2D -= HandleCollision;
            m_CollisionEvents.OnCollisionExitEvent2D -= HandleCollision;
            m_CollisionEvents.OnCollisionStayEvent2D -= HandleCollision;
        }
    }
}
#endif
