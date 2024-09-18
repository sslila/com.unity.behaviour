using Unity.Behavior.GraphFramework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior
{
    internal class EventNodeModel : BehaviorGraphNodeModel
    {
        public static readonly string ChannelFieldName = "ChannelVariable";

        public override bool IsSequenceable => true;
        public SerializableType EventChannelType =>
            Fields.FirstOrDefault(field => field.FieldName == ChannelFieldName)?.LinkedVariable?.Type;
        
        public EventNodeModel(NodeInfo nodeInfo) : base (nodeInfo) { }

        protected EventNodeModel(EventNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset) { }

        protected override void EnsureFieldValuesAreUpToDate()
        {
            Type channelType = EventChannelType;
            if (channelType == null) 
            {
                // No channel is assigned, so remove variable fields.
                m_FieldValues.Clear();
                GetOrCreateField(ChannelFieldName, typeof(EventChannelBase));
                return;
            }

            Type eventHandlerType = channelType.GetEvent("Event").EventHandlerType;
            ParameterInfo[] messageParameters = eventHandlerType.GetMethod("Invoke")
                .GetParameters()
                .ToArray();

            // Check if number of message types is correct
            if (messageParameters.Length != m_FieldValues.Count - 1)
            {
                RecreateFields(messageParameters, channelType);
                return;
            }

            // Check if channel message types align with field types
            for (int i = 0; i < messageParameters.Length; ++i)
            {
                int messageFieldIndex = i + 1;  // offset by one due to channel field + any addition field
                ParameterInfo info = messageParameters[i];
                Type fieldValueType = m_FieldValues[messageFieldIndex]?.Type;
                if (fieldValueType == null || !fieldValueType.IsAssignableFrom(info.ParameterType))
                {
                    RecreateFields(messageParameters, channelType);
                    return;
                }
            }
        }

        private void RecreateFields(ParameterInfo[] messageParameters, Type channelType)
        {
            bool MatchesMessageParam(FieldModel field) 
            {
                return messageParameters.Any(param =>
                    field.FieldName == param.Name && (Type)field.Type == param.ParameterType);
            }

            FieldModel channelField = GetOrCreateField(ChannelFieldName, channelType);
            m_FieldValues.RemoveAll(field => field != channelField && !MatchesMessageParam(field));
            for (int m = 0; m < messageParameters.Length; m++)
            {
                ParameterInfo info = messageParameters[m];
                GetOrCreateField(info.Name, info.ParameterType);
            }
        }
    }
}
