#if UNITY_EDITOR
using System;

namespace Unity.Behavior.GraphFramework
{
    internal class DeleteNodesAndEdgesCommandHandler : CommandHandler<DeleteNodesAndEdgesCommand>
    {
        public override bool Process(DeleteNodesAndEdgesCommand command)
        {
            foreach (Tuple<PortModel, PortModel> edge in command.EdgesToDelete)
            {
                Asset.DeleteEdge(edge.Item1, edge.Item2);
            }

            // Ensure sequences are deleted first, so that all nested nodes are deleted.
            command.NodesToDelete.Sort(SortSequencesFirst);
            foreach (NodeModel node in command.NodesToDelete)
            {
                // If the node hasn't already been removed, delete it.
                if (Asset.Nodes.Contains(node))
                {
                    Asset.DeleteNode(node);
                }
            }
            
            return true;
        }

        private static int SortSequencesFirst(NodeModel x, NodeModel y)
        {
            return (x is SequenceNodeModel, y is SequenceNodeModel) switch
            {
                (true, true) => 0,
                (true, false) => -1,
                (false, true) => 1,
                (false, false) => 0
            };
        }
    }
}
#endif