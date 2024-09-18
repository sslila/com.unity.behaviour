namespace Unity.Behavior.GraphFramework
{
    internal class DeleteEdgeCommand : Command
    {
        public PortModel StartPort { get; }
        public PortModel EndPort { get; }

        public DeleteEdgeCommand(PortModel start, PortModel end, bool markUndo = true) : base(markUndo)
        {
            StartPort = start;
            EndPort = end;
        }
    }
}