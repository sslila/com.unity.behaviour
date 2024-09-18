#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    internal class CopyNodeCommandHandler : CommandHandler<CopyNodeCommand>
    {
        public override bool Process(CopyNodeCommand command)
        {
            if (!command.NodeModels.Any())
            {
                return true;   
            }

            NodeCopyData copyNodeList = new NodeCopyData
            {
                Nodes = command.NodeModels
            };
            string jsonString = JsonUtility.ToJson(copyNodeList);
            GUIUtility.systemCopyBuffer = jsonString;
            
            return true;
        }
    }
}
#endif