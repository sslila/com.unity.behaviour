#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    internal class SwapNodeFromSerializedTypeCommandHandler : CommandHandler<SwapNodeFromSerializedTypeCommand>
    {
        public override bool Process(SwapNodeFromSerializedTypeCommand command)
        {
            //Find the type of the new node
            Type type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => typeof(Node).IsAssignableFrom(t) && t.Name == command.NewNodeTypeName);
            NodeInfo newNodeInfo = type == null ? null : NodeRegistry.GetInfo(type);
            if (type == null || newNodeInfo == null)
            {
                Debug.LogError($"Could not find type {command.NewNodeTypeName}");
                return false;
            }

            //search for all placeholder nodes that have the same name and replace them with the new node
            var nodesToReplaces = new List<NodeModel>();
            foreach (var nodeModel in Asset.Nodes)
            {
                if (nodeModel is PlaceholderNodeModel placeholderActionNodeModel &&
                    placeholderActionNodeModel.Name == command.PlaceholderNodeName)
                {
                    nodesToReplaces.Add(nodeModel);
                }
            }

            foreach (var nodeToReplace in nodesToReplaces)
            {
                var newNodeModel = Asset.CreateNode(newNodeInfo.ModelType, nodeToReplace.Position, null,
                    args: new object[] { newNodeInfo }) as BehaviorGraphNodeModel;
                
                // Connect default LinkField variables to fields.
                if (DispatcherContext is BehaviorGraphEditor behaviorGraphEditor)
                {
                    behaviorGraphEditor.LinkVariablesFromBlackboard(newNodeModel);
                    behaviorGraphEditor.LinkRecentlyLinkedFields(newNodeModel);
                    newNodeModel.OnValidate();
                }
                
                if (IsInSequence(nodeToReplace, out SequenceNodeModel sequenceNodeModel))
                {
                    var index = sequenceNodeModel.Nodes.IndexOf(nodeToReplace);
                    Asset.AddNodeToSequence(newNodeModel, sequenceNodeModel, index);
                }
                else
                {
                    foreach (PortModel portModel in newNodeModel.AllPortModels)
                    {
                        PortModel oldPortModel = nodeToReplace.FindPortModelByName(portModel.Name);
                        if (oldPortModel == null)
                        {
                            continue;
                        }

                        foreach (PortModel connectedPort in oldPortModel.Connections)
                        {
                            if (portModel.IsFloating)
                            {
                                if (connectedPort.NodeModel is FloatingPortNodeModel oldFloatingPortNodeModel && portModel.Connections.Count == 1 && portModel.Connections[0].NodeModel is FloatingPortNodeModel newFloatingPortNodeModel)
                                {
                                    newFloatingPortNodeModel.Position = oldFloatingPortNodeModel.Position;
                                    foreach (PortModel oldFloatingPortModel in oldFloatingPortNodeModel.OutputPortModels)
                                    {
                                        PortModel newFloatingPortModel = newFloatingPortNodeModel.FindPortModelByName(oldFloatingPortModel.Name);
                                        if (newFloatingPortModel == null)
                                        {
                                            continue;
                                        }
                                        foreach (PortModel connectionToOldFloatingPort in oldFloatingPortModel.Connections)
                                        {
                                            Asset.ConnectEdge(newFloatingPortModel, connectionToOldFloatingPort);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Asset.ConnectEdge(portModel, connectedPort);
                            }
                        }
                    }
                }

                Asset.DeleteNode(nodeToReplace);
            }

            return true;
        }
        
        private bool IsInSequence(NodeModel nodeModel, out SequenceNodeModel sequenceNodeModel)
        {
            sequenceNodeModel = null;

            if (nodeModel.Parents.Count == 0)
            {
                return false;
            }

            sequenceNodeModel = nodeModel.Parents[0] as SequenceNodeModel;
            return sequenceNodeModel != null;
        }
    }
}
#endif