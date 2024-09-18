using System.Collections.Generic;

namespace Unity.Behavior.GraphFramework
{
    internal class CreateNewSequenceOnDropCommand : Command
    {
        public bool InsertAtTop { get; }
        public NodeModel DropTarget { get; }
        public List<NodeModel> NodesToAdd { get; }
        public List<SequenceNodeModel> SequencesToDelete { get; }
        
        public CreateNewSequenceOnDropCommand(bool insertAtTop, NodeModel dropTarget, List<NodeModel> nodesToAdd, List<SequenceNodeModel> sequencesToDelete, bool markUndo=true) : base(markUndo)
        {
            InsertAtTop = insertAtTop;
            DropTarget = dropTarget;
            NodesToAdd = nodesToAdd;
            SequencesToDelete = sequencesToDelete;
        }
    }
}