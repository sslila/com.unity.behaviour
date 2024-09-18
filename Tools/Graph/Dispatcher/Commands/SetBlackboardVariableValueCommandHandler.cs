#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class SetBlackboardVariableValueCommandHandler : CommandHandler<SetBlackboardVariableValueCommand>
    {
        public override bool Process(SetBlackboardVariableValueCommand command)
        {
            Asset?.MarkUndo("Set blackboard variable value");
            BlackboardAsset?.MarkUndo("Set blackboard asset variable value");
            command.Variable.ObjectValue = command.Value;
            
            // Have we processed the command and wish to block further processing?
            return true;
        }
    }
}
#endif