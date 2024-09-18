#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    internal class SwapNodeFromSerializedTypeCommand : Command
    {
        public string PlaceholderNodeName;
        public string NewNodeTypeName;
        public Vector2 Position;

        public SwapNodeFromSerializedTypeCommand(string placeholderNodeName, string newNodeTypeName,
            Vector2 position,
            bool markUndo) : base(markUndo)
        {
            PlaceholderNodeName = placeholderNodeName;
            NewNodeTypeName = newNodeTypeName;
            Position = position;
        }
    }
}
#endif