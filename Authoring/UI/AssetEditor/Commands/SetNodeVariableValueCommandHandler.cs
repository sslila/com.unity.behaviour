#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class SetNodeVariableValueCommandHandler : CommandHandler<SetNodeVariableValueCommand>
    {
        public override bool Process(SetNodeVariableValueCommand command)
        {
            Asset.MarkUndo("Set node variable value");
            BehaviorGraphNodeModel behaviorNodeModel = command.NodeModel as BehaviorGraphNodeModel;
            behaviorNodeModel?.SetField(command.VariableName, command.Value);

            // Have we processed the command and wish to block further processing?
            return true;
        }
    }
}
#endif