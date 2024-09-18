using System.Collections.Generic;
using System.Linq;

namespace Unity.Behavior
{
    internal static class SubgraphUtility
    {
        internal static bool ContainsReferenceTo(this BehaviorAuthoringGraph subgraphAsset, BehaviorAuthoringGraph parentAsset)
        {
            // Null assets can't reference the parent asset.
            if (!subgraphAsset)
            {
                return false;
            }

            // Trivial check: see if the two assets are the same.
            if (subgraphAsset == parentAsset)
            {
                return true;
            }

            // Detect if the subgraph has any references to this node's graph asset.
            bool cycleDetected = false;
            HashSet<BehaviorAuthoringGraph> visitedSubgraphs = new() { subgraphAsset };
            List<BehaviorAuthoringGraph> subgraphsToCheck = new() { subgraphAsset };
            while (subgraphsToCheck.Count != 0)
            {
                var subgraph = subgraphsToCheck[0];
                subgraphsToCheck.Remove(subgraph);

                if (subgraph.Nodes.OfType<SubgraphNodeModel>().Any(node => node.RuntimeSubgraph == parentAsset))
                {
                    cycleDetected = true;
                    break;
                }

                // Queue subgraphs for checking
                foreach (var subgraphNode in subgraph.Nodes.OfType<SubgraphNodeModel>())
                {
                    if (subgraphNode.RuntimeSubgraph && subgraphNode.SubgraphAuthoringAsset && visitedSubgraphs.Add(subgraphNode.SubgraphAuthoringAsset))
                    {
                        subgraphsToCheck.Add(subgraphNode.SubgraphAuthoringAsset);
                    }
                }
            }

            return cycleDetected;
        }
    }
}