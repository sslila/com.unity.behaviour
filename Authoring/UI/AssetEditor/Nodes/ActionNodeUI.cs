#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(ActionNodeModel))]
    internal class ActionNodeUI : BehaviorNodeUI
    {
        public ActionNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Action");
            BehaviorGraphNodeModel behaviorNodeModel = nodeModel as BehaviorGraphNodeModel;
            if (behaviorNodeModel == null)
            {
                return;
            }
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorNodeModel.NodeTypeID);

            if (nodeInfo.Category == "Action/Conditional")
            {
                AddToClassList("Condition");
                AddToClassList("ShowNodeColor");
            }
            
            InitFromNodeInfo(nodeInfo);
        }

        internal override void InitFromNodeInfo(NodeInfo nodeInfo)
        {
            ReflectionElement reflectionElement = this.Q<ReflectionElement>();
            if (reflectionElement == null)
            {
                reflectionElement = new ReflectionElement();
                Insert(0, reflectionElement);
            }
            reflectionElement.CreateFields(nodeInfo);
            reflectionElement.Node = Model as BehaviorGraphNodeModel;
        }
    }
}
#endif