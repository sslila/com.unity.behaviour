#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [Serializable]
    internal class CreateVariableFromSerializedTypeCommand : Command
    {
        public string VariableTypeName;
        
        public CreateVariableFromSerializedTypeCommand(string variableTypeName, bool markUndo) : base(markUndo)
        {
            VariableTypeName = variableTypeName;
        }
    }
}
#endif