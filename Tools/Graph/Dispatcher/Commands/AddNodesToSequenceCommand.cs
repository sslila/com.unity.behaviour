using System.Collections.Generic;

namespace Unity.Behavior.GraphFramework
{
    internal class AddNodesToSequenceCommand : Command
    {
        public SequenceNodeModel TargetSequence { get; }
        public int StartingIndex { get; }
        public List<NodeModel> NodesToAdd { get; }
        public List<SequenceNodeModel> SequencesToDelete { get; }
        
        public AddNodesToSequenceCommand(SequenceNodeModel targetSequence, int startingIndex, List<NodeModel> nodesToAdd, List<SequenceNodeModel> sequencesToDelete, bool markUndo=true) : base(markUndo)
        {
            TargetSequence = targetSequence;
            StartingIndex = startingIndex;
            NodesToAdd = nodesToAdd;
            SequencesToDelete = sequencesToDelete;
        }
    }
}