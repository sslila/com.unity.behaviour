#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    internal class CreateNodeFromSerializedTypeCommand : Command
    {
        public string NodeTypeName;
        public Vector2 Position;
        
        public CreateNodeFromSerializedTypeCommand(string nodeTypeName, Vector2 position, bool markUndo) : base(markUndo)
        {
            NodeTypeName = nodeTypeName;
            Position = position;
        }
    }
}
#endif