#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class CopyNodeCommandHandler : CommandHandler<CopyNodeCommand>
    {
        public override bool Process(CopyNodeCommand command)
        {
            if (!command.NodeModels.Any())
            {
                return true;   
            }

            NodeCopyData copyData = new NodeCopyData { Nodes = command.NodeModels };
            string jsonString = JsonUtility.ToJson(copyData);
            GUIUtility.systemCopyBuffer = jsonString;

            return true;
        }
    }

    [Serializable]
    internal class NodeCopyData
    {
        [SerializeReference]
        public List<NodeModel> Nodes;
    }
}
#endif