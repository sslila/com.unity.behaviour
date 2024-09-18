#if UNITY_EDITOR
using System;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    internal class CreateNodeFromSerializedTypeCommandHandler : CommandHandler<CreateNodeFromSerializedTypeCommand>
    {
        public override bool Process(CreateNodeFromSerializedTypeCommand command)
        {
            Type type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(t => typeof(Node).IsAssignableFrom(t) && t.Name == command.NodeTypeName);
            NodeInfo nodeInfo = type == null ? null : NodeRegistry.GetInfo(type);
            if (type == null || nodeInfo == null)
            {
                Debug.LogError($"Could not find type {command.NodeTypeName}");
                return false;
            }

            // Create node.
            NodeModel newNode = Asset.CreateNode(nodeInfo.ModelType, command.Position, null, new object[] { nodeInfo });

            // Connect default LinkField variables to fields.
            if (DispatcherContext is BehaviorGraphEditor behaviorGraphEditor &&
                newNode is BehaviorGraphNodeModel behaviorGraphNodeModel)
            {
                behaviorGraphEditor.LinkVariablesFromBlackboard(behaviorGraphNodeModel);
                behaviorGraphEditor.LinkRecentlyLinkedFields(behaviorGraphNodeModel);
                behaviorGraphNodeModel.OnValidate();
            }
            
            return true;
        }
    }
}
#endif