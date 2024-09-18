#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class CreateNodeCommandHandler : CommandHandler<CreateNodeCommand>
    {
        public override bool Process(CreateNodeCommand command)
        {
            NodeModel node = Asset.CreateNode(command.NodeType, command.Position, command.ConnectedPort, command.Args);
            if (command.SequenceToAddTo != null && node.IsSequenceable)
            {
                Asset.AddNodeToSequence(node, command.SequenceToAddTo, command.SequenceToAddTo.Nodes.Count);
            }
            return false;
        }
    }
}
#endif