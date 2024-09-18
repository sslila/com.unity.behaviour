using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable]
    internal abstract class WaitForPhysicsMessageAction : Action
    {
        public enum EMessageType
        {
            Enter,
            Exit,
            Stay,
        }

        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<EMessageType> MessageType;
        [SerializeReference] public BlackboardVariable<string> Tag;
        [Tooltip("[Out Value] This field is assigned with the collided object.")]
        [SerializeReference] public BlackboardVariable<GameObject> CollidedObject;

        protected BehaviorGraphCollisionEvents m_CollisionEvents { get; private set; }

        [CreateProperty]
        protected bool m_HasBeenProcessed;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (CollidedObject is IBlackboardVariableCast)
            {
                var caster = CollidedObject as IBlackboardVariableCast;
                LogFailure($"Invalid CollidedObject variable: Expecting 'GameObject' but is '{caster.SourceTypeName}'. Please provide a valid GameObject variable.");
                return Status.Failure;
            }

            m_HasBeenProcessed = false;

            if (m_CollisionEvents == null)
            {
                m_CollisionEvents = Agent.Value.GetOrAddComponent<BehaviorGraphCollisionEvents>();
            }

            RegisterEvents();

            return Status.Waiting;
        }

        protected override Status OnUpdate()
        {
            return m_HasBeenProcessed ? Status.Success : Status.Waiting;
        }

        protected override void OnEnd()
        {
            UnregisterEvents();
        }

        protected override void OnDeserialize()
        {
            m_CollisionEvents = Agent.Value.GetComponent<BehaviorGraphCollisionEvents>();
            RegisterEvents();
        }

        protected abstract void RegisterEvents();

        protected abstract void UnregisterEvents();

        protected void HandleCollision(GameObject other)
        {
            if (other == null || !IsRunning)
            {
                return;
            }

            if (Tag != null && !string.IsNullOrEmpty(Tag.Value) && !other.CompareTag(Tag.Value))
            {
                return;
            }

            if (CollidedObject != null)
            {
                CollidedObject.Value = other;
            }

            m_HasBeenProcessed = true;
            AwakeNode(this);
        }
    }
}
