using System;
using UnityEngine;

namespace Unity.Behavior
{
    internal class GameObjectToComponentBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(GameObject) && toType.IsSubclassOf(typeof(Component));
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return Activator.CreateInstance(typeof(GameObjectToComponentBlackboardVariable<>).MakeGenericType(toType), variable) as BlackboardVariable;
        }
    }
}