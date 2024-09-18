#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(FloatingPortNodeModel))]
    internal class FloatingPortNodeUI : BehaviorNodeUI
    {
        public FloatingPortNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Modifier");
            
            if (IsFloatingConditionNodePort())
            {
                AddToClassList("Condition");
            }

            IsDeletable = false;

            if (nodeModel != null)
            {
                FloatingPortNodeModel portNodeModel = nodeModel as FloatingPortNodeModel;
                Title = Util.NicifyVariableName(portNodeModel.PortName);
            }

            // Disable the input port from being used manually.
            var inputPort = GetFirstInputPort();
            if (inputPort != null)
            {
                inputPort.pickingMode = PickingMode.Ignore;
            }
        }
        
        private bool IsFloatingConditionNodePort()
        {
            return Model != null && Model.HasIncomingConnections && (Model.IncomingConnections.First().NodeModel is BranchingConditionNodeModel || Model.IncomingConnections.First().NodeModel is SwitchNodeModel);
        }
    }
}
#endif