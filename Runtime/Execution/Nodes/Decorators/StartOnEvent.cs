using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Starts the subgraph upon receiving an event message.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Start On Event Message", 
        category: "Events", 
        story: "When a message is received on [ChannelVariable]", 
        description: "Starts the subgraph upon receiving an event message.",
        id: "a90ecb9b9ff9932eb96f04424549494c")]
    internal partial class StartOnEvent : Modifier
    {
        [Serializable]
        internal enum TriggerBehavior
        {
            Default,
            Restart,
            Once
        }

        /// <summary>
        /// The event channel to listen to.
        /// </summary>
        [SerializeReference]
        public BlackboardVariable ChannelVariable;
        private EventChannelBase EventChannel => ChannelVariable?.ObjectValue as EventChannelBase;

        /// <summary>
        /// The variables sent with the event message.
        /// </summary>
        [SerializeReference]
        internal BlackboardVariable[] MessageVariables = new BlackboardVariable[4];

        [SerializeField]
        internal TriggerBehavior Mode = TriggerBehavior.Default;
        private Delegate m_CaptureVariablesDelegate;
        private EventChannelBase m_CurrentChannel;
        [CreateProperty]
        private bool m_BranchRunning = false;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (!EventChannel)
            {
                return Status.Failure;
            }

            m_BranchRunning = false;
            RegisterListener();
            ChannelVariable.OnValueChanged += UnregisterListener;
            ChannelVariable.OnValueChanged += RegisterListener;

            return Status.Waiting;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            if (Child != null)
            {
                EndNode(Child);                
            }
            
            // Otherwise, reset and wait for next message.
            m_BranchRunning = false;
            if (Mode == TriggerBehavior.Default)
            {
                // If interrupt is enabled, the delegate will still be registered.
                m_CurrentChannel.RegisterListener(m_CaptureVariablesDelegate);
            }
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
            // If the mode is Restart OR the child branch is not running yet.
            if (Mode == TriggerBehavior.Restart || !m_BranchRunning)
            {
                RegisterListener();
            }

            // else the child branch is running, so we don't need to do anything as the graph will start them.

            ChannelVariable.OnValueChanged += UnregisterListener;
            ChannelVariable.OnValueChanged += RegisterListener;
        }

        private void OnMessageReceived()
        {
            // If interrupts disabled, unregister from future events to prevent further messages writing to variables.
            if (Mode != TriggerBehavior.Restart)
            {
                m_CurrentChannel.UnregisterListener(m_CaptureVariablesDelegate);
            }
            
            // No subgraph exists. Awaken this node. 
            if (Child == null)
            {
                AwakeNode(this);
                return;
            }

            // A subgraph exists but is not running. Start it.
            if (!m_BranchRunning)
            {
                // The order is important as it is possible to have a child Triggering an Event
                // that would need the current child to end. i.e.:
                // StartOnEvent<State>
                //      -> Switch<State>
                //          -> State 'A'
                //              -> TriggerEvent: 'To State B'
                //          -> State 'B'
                m_BranchRunning = true;
                StartNode(Child);
                return;
            }
            
            // The subgraph is running. Check for interrupt.
            if (Mode == TriggerBehavior.Restart)
            {
                EndNode(Child);
                StartNode(Child);
            }
        }
        
        private void RegisterListener()
        {
            m_CurrentChannel = EventChannel;
            if (m_CurrentChannel)
            {
                m_CaptureVariablesDelegate = m_CurrentChannel.CreateEventHandler(MessageVariables, OnMessageReceived);
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