using System;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Trigger",
        story: "Wait for Trigger [MessageType] on [Agent]",
        description: "Waits for an OnTrigger event on the specified agent.",
        category: "Action/Physics",
        id: "d8b09f7730a93ec5e2b0f0f5a09daa07")]
    internal partial class WaitForTriggerAction : WaitForPhysicsMessageAction
    {
        protected override void RegisterEvents()
        {
            switch (MessageType.Value)
            {
                case EMessageType.Enter:
                    m_CollisionEvents.OnTriggerEnterEvent += HandleCollision;
                    break;
                case EMessageType.Stay:
                    m_CollisionEvents.OnTriggerStayEvent += HandleCollision;
                    break;
                case EMessageType.Exit:
                    m_CollisionEvents.OnTriggerExitEvent += HandleCollision;
                    break;
            }
        }

        protected override void UnregisterEvents()
        {
            if (m_CollisionEvents == null)
            {
                return;
            }

            m_CollisionEvents.OnTriggerEnterEvent -= HandleCollision;
            m_CollisionEvents.OnTriggerExitEvent -= HandleCollision;
            m_CollisionEvents.OnTriggerStayEvent -= HandleCollision;
        }
    }
}
