using System;
using UnityEngine;

namespace Unity.Behavior
{
    internal class ComponentToComponentBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType.IsSubclassOf(typeof(Component)) && toType.IsSubclassOf(typeof(Component));
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return Activator.CreateInstance(typeof(ComponentToComponentBlackboardVariable<,>).MakeGenericType(fromType, toType), variable) as BlackboardVariable;
        }
    }
}