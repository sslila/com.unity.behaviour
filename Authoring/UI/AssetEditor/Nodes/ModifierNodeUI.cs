#if UNITY_EDITOR

using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [NodeUIAttribute(typeof(ModifierNodeModel))]
    internal class ModifierNodeUI : BehaviorNodeUI
    {
        public ModifierNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Modifier");
            BehaviorGraphNodeModel behaviorNodeModel = nodeModel as BehaviorGraphNodeModel;
            if (behaviorNodeModel == null)
            {
                return;
            }
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorNodeModel.NodeTypeID);
            InitFromNodeInfo(nodeInfo);
        }
    }
}
#endif