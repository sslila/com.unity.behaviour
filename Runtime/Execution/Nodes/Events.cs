using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Event channels are used to send and receive event messages.
    /// </summary>
    public abstract class EventChannelBase : ScriptableObject
    {
        /// <summary>
        /// Sends an event message on the channel.
        /// </summary>
        /// <param name="messageData">The Blackboard Variables holding the data for the message.</param>
        public abstract void SendEventMessage(BlackboardVariable[] messageData);
        /// <summary>
        /// Creates an event handler for the channel.
        /// </summary>
        /// <param name="vars">The Blackboard Variables which will receive the data for the message.</param>
        /// <param name="callback">The callback to be called for the event.</param>
        /// <returns>The created event handler.</returns>
        public abstract Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback);
        /// <summary>
        /// registers a listener to the channel.
        /// </summary>
        /// <param name="unityAction">The delegate to register.</param>
        public abstract void RegisterListener(Delegate unityAction);
        /// <summary>
        /// Unregisters a listener from the channel.
        /// </summary>
        /// <param name="unityAction">The delegate to unregister.</param>
        public abstract void UnregisterListener(Delegate unityAction);
    }

    /// <summary>
    /// Event actions are used to send and receive event messages.
    /// </summary>
    public abstract class EventAction : Action
    {
        /// <summary>
        /// The event channel variable.
        /// </summary>
        [SerializeReference] public BlackboardVariable ChannelVariable;

        /// <summary>
        /// A reference to the event channel.
        /// </summary>
        public EventChannelBase EventChannel => ChannelVariable?.ObjectValue as EventChannelBase;
    }

    /// <inheritdoc cref="EventAction"/>
    /// <summary>
    /// Triggers an event message on the assigned channel.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Send Event Message",
        description: "Sends an event message on the assigned channel.",
        category: "Events",
        id: "99ca68fd9e704c8abdaacf5597e42a4a")]
    public partial class TriggerEventAction : EventAction
    {
        [SerializeReference]
        internal BlackboardVariable[] MessageVariables = new BlackboardVariable[4];

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (!EventChannel)
            {
                return Status.Failure;
            }

            EventChannel.SendEventMessage(MessageVariables);
            return Status.Success;
        }
    }

    /// <inheritdoc cref="EventAction"/>
    /// <summary>
    /// Wait for an event message to be received on the assigned channel.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait for Event Message",
        description: "Waits for an event message to be received on the assigned channel.",
        category: "Events",
        id: "f61ef5906ac54fd8b4e786e1b3984ba5")]
    internal partial class WaitForEventAction : EventAction
    {
        [SerializeReference]
        internal BlackboardVariable[] MessageVariables = new BlackboardVariable[4];

        private Delegate m_CaptureVariablesDelegate;
        private EventChannelBase m_CurrentChannel;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (!EventChannel)
            {
                return Status.Failure;
            }

            RegisterListener();
            ChannelVariable.OnValueChanged += UnregisterListener;
            ChannelVariable.OnValueChanged += RegisterListener;

            return Status.Waiting;
        }

        /// <inheritdoc cref="OnEnd" />
        protected override void OnEnd()
        {
            UnregisterListener();
            if (ChannelVariable != null)
            {
                ChannelVariable.OnValueChanged -= UnregisterListener;
                ChannelVariable.OnValueChanged -= RegisterListener;
            }
        }

        protected override void OnDeserialize()
        {
            // if this node is deserialized, it means it was waiting and we need to re-register the listener.

            RegisterListener();
            ChannelVariable.OnValueChanged += UnregisterListener;
            ChannelVariable.OnValueChanged += RegisterListener;
        }

        private void AwakeSelf() => AwakeNode(this);

        private void RegisterListener()
        {
            m_CurrentChannel = EventChannel;
            if (m_CurrentChannel)
            {
                m_CaptureVariablesDelegate = m_CurrentChannel.CreateEventHandler(MessageVariables, AwakeSelf);
                m_CurrentChannel.RegisterListener(m_CaptureVariablesDelegate);
            }
        }

        private void UnregisterListener()
        {
            if (m_CurrentChannel)
            {
                m_CurrentChannel.UnregisterListener(m_CaptureVariablesDelegate);
            }
        }
    }
}
