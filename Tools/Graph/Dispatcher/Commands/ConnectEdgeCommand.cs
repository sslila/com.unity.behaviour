namespace Unity.Behavior.GraphFramework
{
    internal class ConnectEdgeCommand : Command
    {
        public PortModel SourcePort { get; }
        public PortModel TargetPort { get; }

        public ConnectEdgeCommand(PortModel sourcePort, PortModel targetPort, bool markUndo = true) : base(markUndo)
        {
            SourcePort = sourcePort;
            TargetPort = targetPort;
        }
    }
}