#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    internal static class EventChannelUtility
    {
        internal struct EventChannelInfo
        {
            internal string Name;
            internal string Category;
            internal Type VariableModelType;
            
            internal string Path => string.IsNullOrEmpty(Category) ? Name : $"{Category}/{Name}";
        }

        internal static bool IsEventChannelType(Type type, out Type eventChannelModelType)
        {
            var baseType = type.BaseType;
            if (baseType == typeof(EventChannelBase))
            {
                eventChannelModelType = typeof(TypedVariableModel<>).MakeGenericType(type);
                return true;
            }
            eventChannelModelType = null;
            return false;

        }

        internal static IEnumerable<EventChannelInfo> GetEventChannelTypes()
        {   
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
            {
                if (!IsEventChannelType(type, out Type eventChannelModelType))
                {
                    continue;
                }

                var attribute = (EventChannelDescriptionAttribute) Attribute.GetCustomAttribute(type, typeof (EventChannelDescriptionAttribute));
                
                string channelName;
                string category;
                if (attribute != null)
                {
                    channelName = attribute.Name;
                    category = String.IsNullOrEmpty(attribute.Category) ? "Events" : attribute.Category;
                }
                else
                {
                    // Channels generated prior to the introduction of EventChannelDescriptionAttribute will use default info.
                    channelName = type.Name;
                    category = "Events";
                }

                yield return new EventChannelInfo { Name = channelName, Category = category, VariableModelType = eventChannelModelType };
            }
        }

        internal static (string, Type[]) GetMessageDataFromChannelType(Type channelType)
        {
            if (channelType == null)
                return default;
            EventInfo eventInfo = channelType.GetEvent("Event");
            if (eventInfo == null)
                return default;

            Type eventHandlerType = eventInfo.EventHandlerType;
            Type[] eventMessageTypes = eventHandlerType.GetMethod("Invoke")
                .GetParameters()
                .Select(p => p.ParameterType).ToArray();
            var attribute =
                (EventChannelDescriptionAttribute)Attribute.GetCustomAttribute(channelType,
                    typeof(EventChannelDescriptionAttribute));
            return (attribute?.Message, eventMessageTypes);
        }
    }
}
#endif