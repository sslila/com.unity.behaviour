using System;
using System.Reflection;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class EventNodeTransformer : INodeTransformer
    {
        public Type NodeModelType => typeof(EventNodeModel);

        public Node CreateNodeFromModel(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel)
        {
            EventNodeModel eventNodeModel = nodeModel as EventNodeModel;

            return Activator.CreateInstance(eventNodeModel.NodeType) as Node;
        }

        public void ProcessNode(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node)
        {
            EventNodeModel eventNodeModel = nodeModel as EventNodeModel;
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(eventNodeModel.NodeTypeID);
            int fieldIndex = -1;
            foreach (BehaviorGraphNodeModel.FieldModel fieldModel in eventNodeModel.Fields)
            {
                fieldIndex++;

                BlackboardVariable variableToAssign = graphAssetProcessor.GetVariableFromFieldModel(fieldModel);
                if (fieldIndex == 0)
                {
                    // The first field keeps a reference to the BlackboardVariable holding the channel.
                    FieldInfo fieldInfo = nodeInfo.Type.GetField(fieldModel.FieldName, GraphAssetProcessor.k_bindingFlags);
                    if (typeof(EventChannelBase).IsAssignableFrom(variableToAssign.Type))
                    {
                        fieldInfo!.SetValue(node, variableToAssign);
                    }
                }
                else
                {
                    // Event nodes keep an array of variables holding the values for the message.
                    if (variableToAssign != null)
                    {
                        // For event nodes, the value isn't tied to a node field, but a variable exists, for which a link is created.
                        var field = nodeInfo.Type.GetField("MessageVariables", BindingFlags.Instance | BindingFlags.NonPublic);
                        object messageVariablesValueObject = field.GetValue(node);
                        if (messageVariablesValueObject is Array messageVariablesArray)
                        {
                            messageVariablesArray.SetValue(variableToAssign, fieldIndex - 1);
                        }
                    }
                }
            }

            ProcessStartOnEventNode(node, eventNodeModel);
        }

        private void ProcessStartOnEventNode(Node node, EventNodeModel model)
        {
            var startOnEventNode = node as StartOnEvent;
            if (startOnEventNode != null)
            {
                var startOnEventModel = model as StartOnEventModel;
                startOnEventNode.Mode = startOnEventModel.TriggerBehavior;
            }
        }
    }
}