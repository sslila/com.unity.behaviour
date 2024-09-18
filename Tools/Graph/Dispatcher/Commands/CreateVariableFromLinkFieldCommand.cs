#if UNITY_EDITOR
using System;

namespace Unity.Behavior.GraphFramework
{
    internal class CreateVariableFromLinkFieldCommand : Command
    {
        public string Name { get; }
        public Type VariableType { get; }
        public object[] Args { get; }
        public BaseLinkField Field { get; }
        
        public CreateVariableFromLinkFieldCommand(BaseLinkField field, string name, Type variableType, params object[] args) : base(true)
        {
            Field = field;
            Name = name;
            VariableType = variableType;
            Args = args;
        }
    }
}
#endif