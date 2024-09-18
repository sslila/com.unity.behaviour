#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class InsertNodeCommandHandler : CommandHandler<InsertNodeCommand>
    {
        public override bool Process(InsertNodeCommand command)
        {
            // Break port connections
            Asset.DeleteEdge(command.ConnectionToBreak.Item1, command.ConnectionToBreak.Item2);

            NodeModel branchNode = Asset.CreateNode(command.InsertedNodeTypeInfo.ModelType, command.Position,
                null, new object[] { command.InsertedNodeTypeInfo });
            
            // Connect new node input port to connected output ports.
            if (branchNode.TryDefaultInputPortModel(out PortModel branchNodeInputPort))
            {
                foreach (PortModel connectToOutputPort in command.ConnectedOutputPorts)
                {
                    Asset.ConnectEdge(connectToOutputPort, branchNodeInputPort);
                }
            }
            
            // Connect new node output port to connected input ports.
            if (branchNode.TryDefaultOutputPortModel(out PortModel branchNodeOutputPort))
            {
                foreach (PortModel inputPort in command.ConnectedInputPorts)
                {
                    Asset.ConnectEdge(branchNodeOutputPort, inputPort);
                }
            }
            
            return true;
        }
    }
}
#endif