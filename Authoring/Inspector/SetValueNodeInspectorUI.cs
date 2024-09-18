#if UNITY_EDITOR
using System.Linq;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(SetValueNodeModel))]
    internal class SetValueNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {

        public SetValueNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }

        public override void Refresh()
        {
            NodeProperties.Clear();
            SetValueNodeModel setValueNodeModel = InspectedNode as SetValueNodeModel;
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(setValueNodeModel.NodeTypeID);

            // Create the variable field.
            CreateField(nodeInfo.Variables[0].Name, nodeInfo.Variables[0].Type);

            // Create the value field.
            VariableModel link = setValueNodeModel.Fields.FirstOrDefault(f => f.FieldName == nodeInfo.Variables[0].Name)?.LinkedVariable;
            VariableInfo variableInfo = nodeInfo.Variables[1];
            if (link == null)
            {
                BaseLinkField field = CreateField(variableInfo.Name, variableInfo.Type);
                field.SetEnabled(false);
                return;
            }
            CreateField(variableInfo.Name, link.Type);
        }
    }
}
#endif