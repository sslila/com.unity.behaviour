#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class SetConditionVariableValueCommandHandler : CommandHandler<SetConditionVariableValueCommand>
    {
        public override bool Process(SetConditionVariableValueCommand command)
        {
            ConditionInfo info = ConditionUtility.GetInfoForConditionType(command.Condition.ConditionType);
            Asset.MarkUndo($"Set condition {info.Name} variable {command.VariableName} value");
            ConditionModel condition = command.Condition;
            condition?.SetField(command.VariableName, command.Value);

            // Have we processed the command and wish to block further processing?
            return true;
        }
    }
}
#endif