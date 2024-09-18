using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class MoveNodesCommand : Command
    {
        public List<NodeModel> NodeModels { get; }
        public List<Vector2> Positions { get; }
        public List<SequenceNodeModel> ParentSequences { get; }

        public MoveNodesCommand(List<NodeModel> models, List<Vector2> positions, List<SequenceNodeModel> parentSequences, bool markUndo = true) : base(markUndo)
        {
            NodeModels = models;
            Positions = positions;
            ParentSequences = parentSequences;
        }
    }
}