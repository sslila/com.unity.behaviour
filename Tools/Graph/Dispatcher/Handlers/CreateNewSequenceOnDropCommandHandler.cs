#if UNITY_EDITOR
using System.Linq;

namespace Unity.Behavior.GraphFramework
{
    internal class CreateNewSequenceOnDropCommandHandler : CommandHandler<CreateNewSequenceOnDropCommand>
    {
        public override bool Process(CreateNewSequenceOnDropCommand command)
        {
            // Create new sequence.
            NodeModel dropTarget = command.DropTarget;
            var sequenceModel = (SequenceNodeModel) Asset.CreateNode(typeof(SequenceNodeModel), dropTarget.Position);
            
            // Connect drop target's existing connections to sequence.
            if (sequenceModel.TryDefaultInputPortModel(out PortModel sequenceInputPort) && dropTarget.TryDefaultInputPortModel(out PortModel dropTargetInputPortModel))
            {
                foreach (PortModel connectedPort in dropTargetInputPortModel.Connections)
                {
                    if (command.NodesToAdd.Contains(connectedPort.NodeModel))
                    {
                        continue;
                    }
                    Asset.ConnectEdge(connectedPort, sequenceInputPort);
                }
            }
            if (sequenceModel.TryDefaultOutputPortModel(out PortModel sequenceOutputPort) && dropTarget.TryDefaultOutputPortModel(out PortModel dropTargetOutputPortModel))
            {
                foreach (PortModel connectedPort in dropTargetOutputPortModel.Connections)
                {
                    if (command.NodesToAdd.Contains(connectedPort.NodeModel))
                    {
                        continue;
                    }
                    Asset.ConnectEdge(sequenceOutputPort, connectedPort);
                }
            }
            
            // Add drop target to sequence.
            int index = 0;
            Asset.AddNodeToSequence(dropTarget, sequenceModel, index++);

            // Adjust index for dropped nodes if inserting above the target.
            if (command.InsertAtTop)
            {
                index--;
            }
            
            // Add other nodes to new sequence.
            foreach (var model in command.NodesToAdd)
            {
                foreach (PortModel connectedPort in model.OutgoingConnections)
                {
                    if (ReferenceEquals(connectedPort.NodeModel, sequenceModel) || sequenceModel.IncomingConnections.Contains(connectedPort.NodeModel.OutputPortModels.FirstOrDefault()))
                    {
                        continue; 
                    }
                    Asset.ConnectEdge(connectedPort, sequenceOutputPort);
                }

                if (sequenceInputPort.Connections.Count() == 0)
                {
                    foreach (PortModel connectedPort in model.IncomingConnections)
                    {
                        if (ReferenceEquals(connectedPort.NodeModel, sequenceModel) || sequenceModel.OutgoingConnections.Contains(connectedPort.NodeModel.InputPortModels.FirstOrDefault()))
                        {
                            continue;
                        }
                        Asset.ConnectEdge(connectedPort, sequenceInputPort);
                        break;
                    }
                }

                Asset.AddNodeToSequence(model, sequenceModel, index++);
            }
            
            // Delete sequences
            foreach (var sequence in command.SequencesToDelete)
            {
                Asset.DeleteNode(sequence);
            }
            
            return true;
        }
    }
}
#endif