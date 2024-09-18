#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    class DeleteNodeCommandHandler : CommandHandler<DeleteNodeCommand>
    {
        public override bool Process(DeleteNodeCommand command)
        {
            Asset.DeleteNode(command.NodeModel);
            return true;
        }
    }
}
#endif