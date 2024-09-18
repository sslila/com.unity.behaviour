using System;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Collision",
        story: "Wait for Collision [MessageType] on [Agent]",
        description: "Waits for an OnCollision event on the specified agent.",
        category: "Action/Physics",
        id: "c6f6fad65cfb8e64c977d3bd749a1b2c")]
    internal partial class WaitForCollisionAction : WaitForPhysicsMessageAction
    {
        protected override void RegisterEvents()
        {
            switch (MessageType.Value)
            {
                case EMessageType.Enter:
                    m_CollisionEvents.OnCollisionEnterEvent += HandleCollision;
                    break;
                case EMessageType.Stay:
                    m_CollisionEvents.OnCollisionStayEvent += HandleCollision;
                    break;
                case EMessageType.Exit:
                    m_CollisionEvents.OnCollisionExitEvent += HandleCollision;
                    break;
            }
        }

        protected override void UnregisterEvents()
        {
            if (m_CollisionEvents == null)
            {
                return;
            }

            m_CollisionEvents.OnCollisionEnterEvent -= HandleCollision;
            m_CollisionEvents.OnCollisionExitEvent -= HandleCollision;
            m_CollisionEvents.OnCollisionStayEvent -= HandleCollision;
        }
    }
}