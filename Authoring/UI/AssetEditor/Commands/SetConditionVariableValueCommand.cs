#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class SetConditionVariableValueCommand : Command
    {
        public ConditionModel Condition;
        public string VariableName;
        public object Value;
        
        public SetConditionVariableValueCommand(ConditionModel condition, string variableName, object value, bool markUndo) : base(markUndo)
        {
            Condition = condition;
            VariableName = variableName;
            Value = value;
        }
    }
}
#endif