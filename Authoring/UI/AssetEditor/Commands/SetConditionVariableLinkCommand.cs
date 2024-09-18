#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class SetConditionVariableLinkCommand : Command
    {
        public ConditionModel Condition;
        public string VariableName;
        public Type LinkVariableType;
        public VariableModel Link;
        
        public SetConditionVariableLinkCommand(ConditionModel condition, string variableName, Type linkVariableType, VariableModel link, bool markUndo) : base(markUndo)
        {
            Condition = condition;
            VariableName = variableName;
            LinkVariableType = linkVariableType;
            Link = link;
        }
    }
}
#endif