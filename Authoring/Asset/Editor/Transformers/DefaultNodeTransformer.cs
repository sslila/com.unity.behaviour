using Unity.Behavior.GraphFramework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior
{
    internal class DefaultNodeTransformer : INodeTransformer
    {
        public Type NodeModelType => typeof(BehaviorGraphNodeModel);

        public Node CreateNodeFromModel(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel)
        {
            BehaviorGraphNodeModel behaviorGraphNodeModel = nodeModel as BehaviorGraphNodeModel;
            var node = Activator.CreateInstance(behaviorGraphNodeModel.NodeType) as Node;

            return node;
        }

        public void ProcessNode(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node)
        {
            ProcessNoodeFields(graphAssetProcessor, nodeModel, node);
        }

        public static void ProcessNoodeFields(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node)
        {
            BehaviorGraphNodeModel behaviorGraphNodeModel = nodeModel as BehaviorGraphNodeModel;
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorGraphNodeModel.NodeTypeID);
            int fieldIndex = -1;
            foreach (BehaviorGraphNodeModel.FieldModel fieldModel in behaviorGraphNodeModel.Fields)
            {
                fieldIndex++;

                // Set fields to linked variables or local values (stored in blackboard variables).
                FieldInfo fieldInfo = nodeInfo.Type.GetField(fieldModel.FieldName, GraphAssetProcessor.k_bindingFlags);
                BlackboardVariable variableToAssign = graphAssetProcessor.GetVariableFromFieldModel(fieldModel);
                if (fieldInfo != null && variableToAssign != null)
                {
                    if (fieldInfo.FieldType.IsInstanceOfType(variableToAssign))
                    {
                        fieldInfo.SetValue(node, variableToAssign);
                        continue;
                    }

                    Debug.LogError($"Unhandled variable assignment in {nodeInfo.Name}: Attempting to assign to field \"{fieldModel.FieldName}\" of type \"{fieldInfo.FieldType}\" a variable of type \"{variableToAssign}\"");
                }

                // Set properties to values stored by variables (not to the variables themselves). todo see if we use this anywhere
                PropertyInfo propertyInfo = nodeInfo.Type.GetProperty(fieldModel.FieldName, GraphAssetProcessor.k_bindingFlags);
                if (propertyInfo != null && variableToAssign != null && propertyInfo.PropertyType.IsInstanceOfType(variableToAssign))
                {
                    propertyInfo.SetValue(node, variableToAssign);
                }
            }

            if (node is IConditional conditionalNode)
            {
                ProcessNodeConditions(graphAssetProcessor, nodeModel, conditionalNode);
            }
        }

        public static void ProcessNodeConditions(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, IConditional node)
        {
            IConditionalNodeModel conditionalNodeModel = nodeModel as IConditionalNodeModel;

            // Copy conditions from the node model to the node condition list.
      
            node.RequiresAllConditions = conditionalNodeModel.RequiresAllConditionsTrue;
            node.Conditions ??= new List<Condition>();
            foreach (ConditionModel conditionModel in conditionalNodeModel.ConditionModels)
            {
                node.Conditions.Add(Activator.CreateInstance(conditionModel.ConditionType) as Condition);
            }
                
            // Set LinkField values from the condition models to the runtime condition variables.
            for (int i = 0; i < conditionalNodeModel.ConditionModels.Count; i++)
            {
                foreach (BehaviorGraphNodeModel.FieldModel fieldModel in conditionalNodeModel.ConditionModels[i].Fields)
                {
                    // Set fields to linked variables or local values (stored in blackboard variables).
                    FieldInfo fieldInfo = conditionalNodeModel.ConditionModels[i].ConditionType.Type.GetField(fieldModel.FieldName, GraphAssetProcessor.k_bindingFlags);
                    BlackboardVariable variableToAssign = graphAssetProcessor.GetVariableFromFieldModel(fieldModel);
                    if (fieldInfo != null && variableToAssign != null)
                    {
                        fieldInfo.SetValue(node.Conditions[i], variableToAssign);
                    }
                    else
                    {
                        Debug.LogError($"Unhandled variable assignment in {conditionalNodeModel.ConditionModels[i]}: Attempting to assign to field \"{fieldModel.FieldName}\" of type \"{fieldInfo.FieldType}\" a variable of type \"{variableToAssign}\"");
                    }
                }   
            }
        }
    }
}