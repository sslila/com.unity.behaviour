#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class ConnectEdgeCommandHandler : CommandHandler<ConnectEdgeCommand>
    {
        public override bool Process(ConnectEdgeCommand command)
        {
            Asset.ConnectEdge(command.SourcePort, command.TargetPort);
            return true;
        }
    }
}
#endif