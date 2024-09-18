#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class CreateNodeCommandHandler : CommandHandler<CreateNodeCommand>
    {
        public override bool Process(CreateNodeCommand command)
        {
            NodeModel newNode = Asset.CreateNode(command.NodeType, command.Position, null, command.Args);

            // Connect default LinkField variables to fields.
            if (DispatcherContext is BehaviorGraphEditor behaviorGraphEditor &&
                newNode is BehaviorGraphNodeModel behaviorGraphNodeModel)
            {
                behaviorGraphEditor.LinkVariablesFromBlackboard(behaviorGraphNodeModel);
                behaviorGraphEditor.LinkRecentlyLinkedFields(behaviorGraphNodeModel);
                behaviorGraphNodeModel.OnValidate();
            }

            if (command.SequenceToAddTo != null && newNode.IsSequenceable)
            {
                Asset.AddNodeToSequence(newNode, command.SequenceToAddTo, command.SequenceToAddTo.Nodes.Count);
                return true;
            }

            if (command.ConnectedPort == null)
            {
                return true;
            }

            // Get the relevant port to connect to.
            PortModel connectedPort = command.ConnectedPort;
            PortModel newNodePort;
            if (connectedPort.IsInputPort)
            {
                newNode.TryDefaultOutputPortModel(out newNodePort);
            }
            else
            {
                newNode.TryDefaultInputPortModel(out newNodePort);
            }

            // If the new node does not have a valid port to connect to, return without connecting.
            if (newNodePort == null)
            {
                return true;
            }

            PortModel outputPortModel = connectedPort.IsOutputPort ? connectedPort : newNodePort;
            PortModel inputPortModel = connectedPort.IsInputPort ? connectedPort : newNodePort;
            BehaviorGraphView graphView = GraphView as BehaviorGraphView;

            graphView!.ConnectPorts(outputPortModel, inputPortModel);

            return true;
        }
    }
}
#endif