#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class ConnectEdgesCommandHandler : CommandHandler<ConnectEdgesCommand>
    {
        public override bool Process(ConnectEdgesCommand command)
        {
            foreach (var portPair in command.PortPairsToConnect)
            {
                Asset.ConnectEdge(portPair.Item1, portPair.Item2);
            }
            return true;
        }
    }
}
#endif