using Unity.Behavior.GraphFramework;
using System;

namespace Unity.Behavior
{
    internal class AbortNodeTransformer : INodeTransformer
    {
        public Type NodeModelType => typeof(AbortNodeModel);

        public Node CreateNodeFromModel(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel)
        {
            AbortNodeModel abortNodeModel = nodeModel as AbortNodeModel;
            
            abortNodeModel.NodeType = abortNodeModel.ModelAbortType == AbortNodeModel.AbortType.Restart ? typeof(RestartModifier) : typeof(AbortModifier);
            
            Node node = Activator.CreateInstance(abortNodeModel.NodeType) as Node;

            return node;
        }

        public void ProcessNode(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node)
        {
            if (node is IConditional conditionalNode)
            {
                DefaultNodeTransformer.ProcessNodeConditions(graphAssetProcessor, nodeModel, conditionalNode);   
            }
        }
    }
}