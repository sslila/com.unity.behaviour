#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(CompositeNodeModel))]
    internal class CompositeNodeUI : BehaviorNodeUI
    {
        public CompositeNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Composite");
            BehaviorGraphNodeModel behaviorGraphNodeModel = nodeModel as BehaviorGraphNodeModel;
            if (behaviorGraphNodeModel == null)
            {
                return;
            }

            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorGraphNodeModel.NodeTypeID);
            InitFromNodeInfo(nodeInfo);            
        }
    }
}
#endif