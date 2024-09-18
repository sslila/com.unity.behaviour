namespace Unity.Behavior.GraphFramework
{
    internal class DeleteNodeCommand : Command
    {
        public NodeModel NodeModel { get; }

        public DeleteNodeCommand(NodeModel nodeModel, bool markUndo = true) : base(markUndo)
        {
            NodeModel = nodeModel;
        }
    }
}