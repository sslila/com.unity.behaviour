#if UNITY_EDITOR
using System.Linq;

namespace Unity.Behavior.GraphFramework
{
    internal class AddNodesToSequenceCommandHandler : CommandHandler<AddNodesToSequenceCommand>
    {
        public override bool Process(AddNodesToSequenceCommand command)
        {
            int index = command.StartingIndex;

            command.TargetSequence.TryDefaultOutputPortModel(out PortModel dropTargetOutputPortModel);
            command.TargetSequence.TryDefaultInputPortModel(out PortModel dropTargetInputPortModel);

            // Add nodes to sequence
            foreach (var node in command.NodesToAdd)
            {
                AddConnectionsToSequenceFromNode(command.TargetSequence, node, dropTargetOutputPortModel, dropTargetInputPortModel);
                Asset.RemoveNodeFromSequence(node);
            }

            foreach (var node in command.NodesToAdd)
            {
                Asset.AddNodeToSequence(node, command.TargetSequence, index++);
            }

            // Delete sequences
            foreach (var sequence in command.SequencesToDelete)
            {
                AddConnectionsToSequenceFromNode(command.TargetSequence, sequence, dropTargetOutputPortModel, dropTargetInputPortModel);
                Asset.DeleteNode(sequence);
            }

            return true;
        }

        private void AddConnectionsToSequenceFromNode(SequenceNodeModel sequenceModel, NodeModel node, PortModel sequenceOutputPort, PortModel sequenceInputPort)
        {
            var nodeOutgoingConnections = node.OutgoingConnections.ToList();
            foreach (PortModel connectedPort in nodeOutgoingConnections)
            {
                if (ReferenceEquals(connectedPort.NodeModel, sequenceModel))
                {
                    PortModel nodeOutputPort = node.OutputPortModels.FirstOrDefault();
                    Asset.DeleteEdge(nodeOutputPort, sequenceInputPort);
                    continue;
                }
                if (sequenceModel.IncomingConnections.Contains(connectedPort.NodeModel.OutputPortModels.FirstOrDefault()))
                {
                    continue;
                }
                Asset.ConnectEdge(connectedPort, sequenceOutputPort);
            }

            if (sequenceInputPort.Connections.Count() == 0)
            {
                foreach (PortModel connectedPort in node.IncomingConnections)
                {
                    if (ReferenceEquals(connectedPort.NodeModel, sequenceModel) || sequenceModel.OutgoingConnections.Contains(connectedPort.NodeModel.InputPortModels.FirstOrDefault()))
                    {
                        continue;
                    }
                    Asset.ConnectEdge(connectedPort, sequenceInputPort);
                    break;
                }
            }
        }
    }
}
#endif