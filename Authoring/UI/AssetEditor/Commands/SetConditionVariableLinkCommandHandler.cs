#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class SetConditionVariableLinkCommandHandler : CommandHandler<SetConditionVariableLinkCommand>
    {
        public SetConditionVariableLinkCommandHandler()
        {
        }

        public override bool Process(SetConditionVariableLinkCommand command)
        {
            ConditionInfo info = ConditionUtility.GetInfoForConditionType(command.Condition.ConditionType);
            Asset.MarkUndo($"Link variable {command.VariableName} to condition {info.Name}");
            IVariableLink field = command.Condition.GetVariableLink(command.VariableName, command.LinkVariableType);
            if (field != null)
            {
                field.BlackboardVariable = command.Link;
            }

            // Have we processed the command and wish to block further processing?
            return true;
        }
    }
}
#endif