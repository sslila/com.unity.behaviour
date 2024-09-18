#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal abstract class DuplicateNodeBaseCommandHandler<T> : CommandHandler<T> where T : DuplicateNodeCommand
    {
        internal Vector2 PastePosition;
        
        public override bool Process(T command)
        {
            if (!command.NodeModels.Any())
                return true;
            
            List<NodeModel> nodeModelDuplicates = command.NodeModels.Select(Duplicate).ToList();
            AdjustPosition(nodeModelDuplicates);
            
            foreach (NodeModel nodeModelDuplicate in nodeModelDuplicates)
            {
                Asset.CreateNodePortsForNode(nodeModelDuplicate);
            }
            AddEdgeConnections(command.NodeModels, nodeModelDuplicates);
            
            GraphView.schedule.Execute(() => { 
                GraphView.ViewState.SetSelected(nodeModelDuplicates);
            });
            return true;
        }

        private void AdjustPosition(List<NodeModel> nodeModelDuplicates)
        {
            var relativePositions = GetRelativePositions(nodeModelDuplicates);
            for (int i = 0; i < nodeModelDuplicates.Count; i++)
            {
                Vector2 newPosition = new Vector2((PastePosition.x + relativePositions[i].x),(PastePosition.y + relativePositions[i].y));
                nodeModelDuplicates[i].Position = newPosition;
            }
        }
        
        private List<Vector2> GetRelativePositions(List<NodeModel> nodeModelDuplicates)
        {
            var centerPosition = GraphUILayoutUtility.GetCenterPointOfNodes(nodeModelDuplicates);
            var elementPositions = nodeModelDuplicates.Select(element => element.Position).ToList();
            var relativePositions = new List<Vector2>();
            foreach (Vector2 position in elementPositions)
            {
                var relativePosition = position - centerPosition;
                relativePositions.Add(relativePosition);
            }
            return relativePositions;
        }
        
        private void AddEdgeConnections(List<NodeModel> originalModels, List<NodeModel> nodeModelDuplicates)
        {
            for (int i = 0; i < nodeModelDuplicates.Count; i++)
            {
                List<PortModel> allOriginalOutputs = originalModels[i].OutputPortModels.ToList();
                List<PortModel> allDuplicateOutputs = nodeModelDuplicates[i].OutputPortModels.ToList();
                for (int j = 0; j < allOriginalOutputs.Count; j++)
                {
                    foreach (PortModel connection in allOriginalOutputs[j].Connections)
                    {
                        PortModel outputPortModel = allDuplicateOutputs[j];
                        var originalConnection = connection;
                        if (connection.NodeModel is FloatingPortNodeModel floatingPortNodeModel && floatingPortNodeModel.HasOutgoingConnections)
                        {
                            originalConnection = floatingPortNodeModel.OutgoingConnections.First();
                            outputPortModel = outputPortModel.Connections.First().NodeModel.OutputPortModels.First();
                        }
                        if (!originalModels.Contains(originalConnection.NodeModel) || originalConnection.IsOutputPort) 
                        {
                            continue;
                        }
                        int index = originalModels.FindIndex(a => a == originalConnection.NodeModel);
                        if (nodeModelDuplicates[index].TryDefaultInputPortModel(out PortModel inputPortModel))
                        {
                            Asset.ConnectEdge(outputPortModel, inputPortModel);
                        }
                    }
                }
            }
        }

        private NodeModel Duplicate(NodeModel nodeModel)
        {
            NodeModel nodeModelDuplicate = InvokeDuplicationConstructor(nodeModel, Asset);
            nodeModelDuplicate.OnDefineNode();
            Asset.Nodes.Add(nodeModelDuplicate);

            // todo: this is not ideal, but I couldn't find a better way ensure adding children
            if (nodeModel is SequenceNodeModel sequenceNodeModel)
            {
                if (nodeModelDuplicate is not SequenceNodeModel sequenceNodeModelDuplicate)
                {
                    throw new InvalidCastException("The duplicated sequence did not result in a sequence. This should never happen!");
                }
                
                foreach (NodeModel nodeModelChild in sequenceNodeModel.Nodes)
                {
                    NodeModel nodeModelChildDuplicate = InvokeDuplicationConstructor(nodeModelChild, Asset);
                    nodeModelChildDuplicate.OnDefineNode();
                    nodeModelChildDuplicate.Parents.Clear();
                    nodeModelChildDuplicate.Parents.Add(nodeModelDuplicate);
                    
                    sequenceNodeModelDuplicate.Nodes.Add(nodeModelChildDuplicate);
                    Asset.Nodes.Add(nodeModelChildDuplicate);
                }
            }

            return nodeModelDuplicate;
        }

        private static NodeModel InvokeDuplicationConstructor(NodeModel nodeModelOriginal, GraphAsset asset)
        {
            var nodeModelType = nodeModelOriginal.GetType();
            var ctor = nodeModelType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                new[] { nodeModelType, asset.GetType() }, null);
                
            return ctor.Invoke(new object[] { nodeModelOriginal, asset }) as NodeModel;
            
            // ALTERNATIVE: using Activator, but I'm not sure about the CultureInfo argument...
            // return Activator.CreateInstance(nodeModelType, BindingFlags.Public | BindingFlags.Instance, null, new object[] { nodeModelOriginal }, CultureInfo.InvariantCulture) as NodeModel;
        }
    }
}
#endif