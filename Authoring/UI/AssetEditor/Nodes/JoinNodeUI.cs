#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [NodeUI(typeof(JoinNodeModel))]
    internal class JoinNodeUI : BehaviorNodeUI
    {
        public JoinNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Modifier");
            BehaviorGraphNodeModel behaviorNodeModel = nodeModel as BehaviorGraphNodeModel;
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorNodeModel.NodeTypeID);
            this.AddNodeIcon(nodeInfo);
            Title = nodeInfo.Name;
        }
    }
}
#endif