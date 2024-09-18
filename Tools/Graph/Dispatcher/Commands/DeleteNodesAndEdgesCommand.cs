using System;
using System.Collections.Generic;

namespace Unity.Behavior.GraphFramework
{
    internal class DeleteNodesAndEdgesCommand : Command
    {
        public List<Tuple<PortModel, PortModel>> EdgesToDelete { get; }
        public List<NodeModel> NodesToDelete { get; }
        
        public DeleteNodesAndEdgesCommand(List<Tuple<PortModel, PortModel>> edgesToDelete, List<NodeModel> nodesToDelete, bool markUndo) : base(markUndo)
        {
            EdgesToDelete = edgesToDelete;
            NodesToDelete = nodesToDelete;
        }
    }
}