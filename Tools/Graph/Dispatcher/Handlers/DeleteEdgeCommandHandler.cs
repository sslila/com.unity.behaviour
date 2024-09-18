#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class DeleteEdgeCommandHandler : CommandHandler<DeleteEdgeCommand>
    {
        public override bool Process(DeleteEdgeCommand command)
        {
            Asset.DeleteEdge(command.StartPort, command.EndPort);
            return true;
        }
    }
}
#endif