#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class SetNodeVariableLinkCommandHandler : CommandHandler<SetNodeVariableLinkCommand>
    {
        public SetNodeVariableLinkCommandHandler()
        {
        }

        public override bool Process(SetNodeVariableLinkCommand command)
        {
            Asset.MarkUndo("Link variable");
            IVariableLink field = command.NodeModel.GetVariableLink(command.VariableName, command.LinkVariableType);
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