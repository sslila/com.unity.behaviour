using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class PasteNodeCommand : DuplicateNodeCommand
    {
        public PasteNodeCommand(IEnumerable<NodeModel> nodeModels, Vector2 position, bool markUndo = true) : base(markUndo)
        {
            NodeModels.AddRange(nodeModels);
            Position = position;
        }
    }
}