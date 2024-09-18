#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class ConnectEdgeCommandHandler : CommandHandler<ConnectEdgeCommand>
    {
        public override bool Process(ConnectEdgeCommand command)
        {
            PortModel outputPortModel = command.SourcePort.IsOutputPort ? command.SourcePort : command.TargetPort;
            PortModel inputPortModel = command.SourcePort.IsInputPort ? command.SourcePort : command.TargetPort;
            BehaviorGraphView graphView = GraphView as BehaviorGraphView;
            
            graphView!.ConnectPorts(outputPortModel, inputPortModel);
            return true;
        }
    }
}
#endif