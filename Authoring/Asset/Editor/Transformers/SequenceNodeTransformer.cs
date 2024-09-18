using Unity.Behavior.GraphFramework;
using System;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior
{
    internal class SequenceNodeTransformer : INodeTransformer
    {
        public Type NodeModelType => typeof(SequenceNodeModel);

        public Node CreateNodeFromModel(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel)
        {
            return new SequenceComposite();
        }

        public void ProcessNode(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node)
        {
            SequenceNodeModel sequenceNodeModel = nodeModel as SequenceNodeModel;
			SequenceComposite sequence = node as SequenceComposite;
            foreach (var childNodeModel in sequenceNodeModel.Nodes)
            {
                switch (childNodeModel)
                {
                    case PlaceholderNodeModel:
                        // Ignore Placeholder Nodes in a sequence.
                        break;
                    case BehaviorGraphNodeModel aidnm:
                        var childNode = graphAssetProcessor.GetOrCreateNode(aidnm);
                        if (childNode != null)
                        {
                            sequence.Add(childNode);
                        }
                        break;
                    default:
                        throw new TypeAccessException(nameof(childNodeModel));
                }
            }
        }
    }
}