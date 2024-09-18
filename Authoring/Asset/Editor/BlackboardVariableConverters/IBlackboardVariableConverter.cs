using System;

namespace Unity.Behavior
{
    internal interface IBlackboardVariableConverter
    {
        bool CanConvert(Type fromType, Type toType);
        
        BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable);
    }
}