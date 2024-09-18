#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class AddConditionFromSerializedCommand : Command
    {
        public SerializableGUID ConditionNodeGUID;
        public string ConditionType;
    
        public AddConditionFromSerializedCommand(SerializableGUID id, string conditionType, bool markUndo) : base(markUndo)
        {
            ConditionNodeGUID = id;
            ConditionType = conditionType;
        }
    }
}
#endif