using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class StartNodeTransformer : INodeTransformer
    {
        public Type NodeModelType => typeof(StartNodeModel);

        public Node CreateNodeFromModel(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel)
        {
            return new Start();
        }

        public void ProcessNode(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node)
        {
            StartNodeModel startNodeModel = nodeModel as StartNodeModel;
            Start startNode = node as Start;
            startNode.Repeat = startNodeModel.Repeat;
        }
    }
}
