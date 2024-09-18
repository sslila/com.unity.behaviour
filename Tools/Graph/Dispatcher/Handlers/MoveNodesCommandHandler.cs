#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class MoveNodesCommandHandler : CommandHandler<MoveNodesCommand>
    {        
        public override bool Process(MoveNodesCommand command)
        {
            for (int i = 0; i < command.NodeModels.Count; i++)
            {
                NodeModel nodeModel = command.NodeModels[i];
                nodeModel.Position = command.Positions[i];
                SequenceNodeModel parentSequence = command.ParentSequences[i];

                // If the node is in a sequence, remove it from the sequence.
                if (parentSequence != null && nodeModel.Parents.Contains(parentSequence))
                {
                    RemoveFromParentSequence(nodeModel, parentSequence);
                }
            }
            
            return true;
        }
        
        private void RemoveFromParentSequence(NodeModel node, SequenceNodeModel sequenceNodeModel)
        {
            // Unlink sequence and node.
            sequenceNodeModel.Nodes.Remove(node);
            node.Parents.Remove(sequenceNodeModel);

            if (sequenceNodeModel.Nodes.Count == 0)
            {
                // Delete empty sequences.
                Asset.DeleteNode(sequenceNodeModel);
            }
            else if (sequenceNodeModel.Nodes.Count == 1)
            {
                // If the sequence contains only one remaining action, remove it as well.
                NodeModel lastRemainingNode = sequenceNodeModel.Nodes[0];
                lastRemainingNode.Position = sequenceNodeModel.Position;
                
                // Unlink from sequence
                sequenceNodeModel.Nodes.Remove(lastRemainingNode);
                lastRemainingNode.Parents.Remove(sequenceNodeModel);
                
                // Preserve incoming and outgoing connections.
                lastRemainingNode.TryDefaultInputPortModel(out PortModel inputPort);
                foreach (PortModel parentNodePort in sequenceNodeModel.IncomingConnections)
                {
                    Asset.ConnectEdge(parentNodePort, inputPort);
                }
                lastRemainingNode.TryDefaultOutputPortModel(out PortModel outputPort);
                foreach (PortModel childNodePort in sequenceNodeModel.OutgoingConnections)
                {
                    Asset.ConnectEdge(outputPort, childNodePort);
                }
                
                // Delete the now empty sequence.
                Asset.DeleteNode(sequenceNodeModel);
            }
        }
    }
}
#endif