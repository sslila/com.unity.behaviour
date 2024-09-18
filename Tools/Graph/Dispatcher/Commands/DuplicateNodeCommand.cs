using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class DuplicateNodeCommand : Command
    {
        public List<NodeModel> NodeModels { get; }
        
        public Vector2 Position { get; protected set; }

        protected DuplicateNodeCommand(bool markUndo = true) : base(markUndo)
        {
            NodeModels = new List<NodeModel>();
        }

        public DuplicateNodeCommand(NodeModel nodeModel, Vector2 position, bool markUndo = true) : this(markUndo)
        {
            NodeModels.Add(nodeModel);
            Position = position;
        }

        public DuplicateNodeCommand(IEnumerable<NodeModel> nodeModels, Vector2 position, bool markUndo = true) : this(markUndo)
        {
            NodeModels.AddRange(nodeModels);
            Position = position;
        }
    }
}