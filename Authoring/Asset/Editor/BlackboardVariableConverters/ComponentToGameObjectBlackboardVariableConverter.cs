using System;
using UnityEngine;

namespace Unity.Behavior
{
    internal class ComponentToGameObjectBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsSubclassOf(typeof(Component)) && toType == typeof(GameObject);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return Activator.CreateInstance(typeof(ComponentToGameObjectBlackboardVariable<>).MakeGenericType(fromType), variable) as BlackboardVariable;
        }
    }
}