#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class GraphUILayoutUtility
    {
        private const int k_VerticalSpacing = 50;
        private const int k_HorizontalSpacing = 30;
        
        internal class NodePositionData
        {
            public readonly NodeUI Node;
            public List<NodePositionData> Children = new ();
            public float X;
            public float Y;
            public readonly float Height;
            public readonly float Width;
            public float SubgraphWidth;

            public NodePositionData()
            {
                Node = null;
                X = Y = Height = Width = SubgraphWidth = 0f;
            }

            public NodePositionData(NodeUI nodeUI)
            {
                Node = nodeUI;
                X = nodeUI.transform.position.x;
                Y = nodeUI.transform.position.y;
                Height = nodeUI.layout.height;
                Width = nodeUI.layout.width;
                SubgraphWidth = Width;
            }
        }

        public static void AlignSelectedNodesImmediateChildren(GraphView graphView)
        {
            graphView.Asset.MarkUndo("Align Child Nodes");
            var nodePositions = ComputeChildNodePositions(graphView.ViewState.Selected);
            ScheduleNodeMovement(graphView, nodePositions);
        }

        public static void AlignSelectedNodesAndAllChildren(GraphView graphView)
        {
            graphView.Asset.MarkUndo("Align Subgraph");
            var nodePositions = ComputeSubgraphNodePositions(graphView.ViewState.Selected);
            ScheduleNodeMovement(graphView, nodePositions);
        }

        public static void ScheduleNodeMovement(GraphView graphView, IEnumerable<KeyValuePair<NodeUI, Vector2>> nodesWithEndPositions)
        {
            // Schedule move
            float accumulated = 0f;
            long msPerIteration = 10;
            long duration = 200; //ms
            var nodeData = nodesWithEndPositions.Select(nodeAndEndPos => new Tuple<NodeModel, Vector2, Vector2>(
                nodeAndEndPos.Key.Model, nodeAndEndPos.Key.transform.position, nodeAndEndPos.Value)).ToList();

            // Animate moving of nodes into position.
            graphView.schedule.Execute((t) =>
            {
                accumulated += t.deltaTime;
                float fractionOfTimePassed = accumulated / duration;
                foreach ((NodeModel node, Vector2 startPos, Vector2 endPos) in nodeData)
                {
                    node.Position = Vector2.Lerp(startPos, endPos, Math.Min(1.0f, fractionOfTimePassed));
                }

                graphView.Asset.SetAssetDirty(false);
                graphView.Asset.HasOutstandingChanges = true;
            }).Every(msPerIteration).ForDuration(duration);

            // Ensure final position.
            graphView.schedule.Execute((t) =>
            {
                foreach ((NodeModel node, _, Vector2 endPos) in nodeData)
                {
                    node.Position = endPos;
                }

                graphView.Asset.SetAssetDirty();
            }).ExecuteLater(duration);
        }

        public static IEnumerable<KeyValuePair<NodeUI, Vector2>> ComputeChildNodePositions(GraphElement rootNode)
        {
            return ComputeChildNodePositions(new[] { rootNode });
        }

        public static IEnumerable<KeyValuePair<NodeUI, Vector2>> ComputeChildNodePositions(IEnumerable<GraphElement> nodesToCompute)
        {
            nodesToCompute = SortForAlignment(nodesToCompute);
            IEnumerable<NodeUI> SortForAlignment(IEnumerable<GraphElement> elements)
            {
                List<NodeUI> sortedList = new();
                foreach (GraphElement element in elements)
                {
                    if (element is NodeUI nodeUI)
                    {
                        Group sequenceUI = element!.GetFirstAncestorOfType<Group>();
                        GraphElement elementToAdd = sequenceUI ?? nodeUI;
                        if (!sortedList.Contains(elementToAdd))
                        {
                            sortedList.Add(elementToAdd as NodeUI);
                        }
                    }
                }
                sortedList.Sort((NodeUI nodeA, NodeUI nodeB) =>
                {
                    if (IsAnAncestor(nodeA, nodeB))
                    {
                        return 1;
                    }
                    else if (IsAnAncestor(nodeB, nodeA))
                    {
                        return -1;
                    }
                    return 0;
                });
                return sortedList;
            }

            Dictionary<NodeUI, Vector2> nodeStartAndEndPositions = new ();
            foreach (NodeUI node in nodesToCompute)
            {
                float nodeHeight = node.layout.height;
                Vector2 nodeCenter = GetComputedOrCurrentNodePosition(node, nodeStartAndEndPositions) + new Vector2(0.0f, node.layout.height / 2);

                // Get child nodes and order by horizontal position
                List<NodeUI> childNodes = node.GetChildNodeUIs().ToList();
                childNodes.Sort((c1, c2) => GetComputedOrCurrentNodePosition(c1, nodeStartAndEndPositions).x.CompareTo(GetComputedOrCurrentNodePosition(c2, nodeStartAndEndPositions).x));

                float totalWidthOfChildren = childNodes.Sum(child => child.GetParentNodeUIs().Count() > 1 ? 0 : child.layout.width)
                                             + Math.Max(0, childNodes.Count - 1) * k_HorizontalSpacing;
                float nextXPosition = nodeCenter.x - 0.5f * totalWidthOfChildren;
                float nextYPosition = nodeCenter.y + nodeHeight / 2 + k_VerticalSpacing;
                
                foreach (NodeUI childNodeUI in childNodes)
                {
                    List<NodeUI> parents = childNodeUI.GetParentNodeUIs().ToList();
                    Vector2 endPosition;
                    if (parents.Count <= 1)
                    {
                        endPosition = new Vector2(nextXPosition + childNodeUI.layout.width * 0.5f, nextYPosition);
                        nextXPosition += childNodeUI.layout.width + k_HorizontalSpacing;
                    }
                    else
                    {
                        // If the node has multiple parents, we want to center it under all of them.
                        float minParentX = parents.Min(p => GetComputedOrCurrentNodePosition(p, nodeStartAndEndPositions).x);
                        float parentsSpan = parents.Max(p => GetComputedOrCurrentNodePosition(p, nodeStartAndEndPositions).x + p.layout.width) - minParentX;
                        float positionX = minParentX + parentsSpan / 2 - childNodeUI.layout.width / 2;
                        float positionY = k_VerticalSpacing + parents.Max(parentUI => GetComputedOrCurrentNodePosition(parentUI, nodeStartAndEndPositions).y + parentUI.layout.height);

                        endPosition = new Vector2(positionX + childNodeUI.layout.width * 0.5f, positionY);
                        nextXPosition = positionX + childNodeUI.layout.width + k_HorizontalSpacing;
                    }

                    nodeStartAndEndPositions.Add(childNodeUI, endPosition);
                }
            }
            return nodeStartAndEndPositions;
        }

        internal static Vector2 GetComputedOrCurrentNodePosition(NodeUI node, Dictionary<NodeUI, Vector2> computedPositions)
        {
            if (computedPositions.TryGetValue(node, out Vector2 position))
                return position;
            return node.transform.position;
        }

        public static IEnumerable<KeyValuePair<NodeUI, Vector2>> ComputeSubgraphNodePositions(NodeUI node)
        {
            return ComputeSubgraphNodePositions(new[] { node });
        }

        internal static IEnumerable<KeyValuePair<NodeUI, Vector2>> ComputeSubgraphNodePositions(IEnumerable<GraphElement> nodesToCompute)
        {
            nodesToCompute = SortForAlignment(nodesToCompute);
            IEnumerable<NodeUI> SortForAlignment(IEnumerable<GraphElement> elements)
            {
                List<NodeUI> sortedList = new();
                foreach (GraphElement element in elements)
                {
                    if (element is NodeUI nodeUI)
                    {
                        Group sequenceUI = element!.GetFirstAncestorOfType<Group>();
                        GraphElement elementToAdd = sequenceUI ?? nodeUI;
                        if (!sortedList.Contains(elementToAdd))
                        {
                            sortedList.Add(elementToAdd as NodeUI);
                        }
                    }
                }
                for (int i = 0; i < sortedList.Count; ++i)
                {
                    NodeUI element = sortedList[i];
                    foreach (NodeUI otherElement in sortedList)
                    {
                        if (IsAnAncestor(element, otherElement))
                        {
                            sortedList.Remove(element);
                            --i;
                            break;
                        }
                    }
                }
                return sortedList;
            }
            List<List<NodePositionData>> nodePositionDataByDepth = new ();
            Queue<Tuple<NodeUI, int>> nodeTraversalQueue = new ();
            Dictionary<NodeUI, int> nodeToDepth = new ();
            Dictionary<NodeUI, NodePositionData> nodeToPositionData = new ();

            foreach (NodeUI rootUI in nodesToCompute)
            {
                // Downward traversal: assign maximum depth/horizon for each node.
                int maxDepth = 0;
                nodeTraversalQueue.Enqueue(new Tuple<NodeUI, int>(rootUI, 0));
                while (nodeTraversalQueue.TryDequeue(out Tuple<NodeUI, int> nodeHorizonTuple))
                {
                    (NodeUI node, int depth) = nodeHorizonTuple;
                    maxDepth = Math.Max(maxDepth, depth);

                    if (nodeToDepth.TryGetValue(node, out int assignedDepth))
                    {
                        if (assignedDepth >= depth) // Already visited at deeper horizon
                            continue;

                        // New horizon is deeper; remove from previously assigned horizon.
                        nodePositionDataByDepth[assignedDepth].RemoveAll(n => n.Node == node);
                    }

                    // Add to horizon
                    NodePositionData nodeData = new NodePositionData(node);
                    nodeToDepth[node] = depth;
                    nodeToPositionData[node] = nodeData;
                    if (nodePositionDataByDepth.Count > depth)
                    {
                        List<NodePositionData> nodes = nodePositionDataByDepth[depth];
                        if (nodes.All(n => n.Node != node)) // Add only if it hasn't been added before
                            nodes.Add(nodeData);
                    }
                    else
                    {
                        nodePositionDataByDepth.Add(new List<NodePositionData> { nodeData });
                    }

                    // Queue children
                    foreach (NodeUI child in node.GetChildNodeUIs())
                    {
                        nodeTraversalQueue.Enqueue(new Tuple<NodeUI, int>(child, depth + 1));
                    }
                }

                // Downward -> set y; set child/parent links in virtual tree 
                float nextY = rootUI.transform.position.y;
                for (int horizon = 0; horizon <= maxDepth; horizon++)
                {
                    List<NodePositionData> horizonNodes = nodePositionDataByDepth[horizon];

                    float maxHeight = 0f;
                    foreach (NodePositionData nodeData in horizonNodes)
                    {
                        maxHeight = Mathf.Max(maxHeight, nodeData.Height);
                        nodeData.Y = nextY;

                        // Set links to children with only this parent
                        nodeData.Children = nodeData.Node.GetChildNodeUIs()
                            .OrderBy(c => nodeToPositionData[c].X)
                            .Where(c => c.GetParentNodeUIs().Count() == 1)
                            .Select(c => nodeToPositionData[c]).ToList();

                        if (horizon == 0)
                        {
                            continue; // Don't set parent links for root horizon.
                        }

                        // Set links to parents, creating virtual parent for nodes with >1 parents
                        List<NodeUI> graphParents = nodeData.Node.GetParentNodeUIs().Where(p => nodeToDepth.ContainsKey(p)).ToList();
                        graphParents.Sort((p1, p2) => nodeToPositionData[p1].X.CompareTo(nodeToPositionData[p2].X));
                        int numParents = graphParents.Count;
                        if (numParents > 1) // multiple parent nodes
                        {
                            // Find the parents' common ancestor.
                            NodeUI commonAncestor = FindCommonAncestorOfParents(nodeData.Node, nodeToDepth);
                            int leftParentIndex = numParents % 2 == 0 ? numParents / 2 - 1 : (numParents - 1) / 2 - 1;
                            // Find the path from the middle-left parent to the ancestor. This is what we'll insert along.
                            List<NodeUI> path = FindRightMostPathToAncestor(graphParents[leftParentIndex], commonAncestor, nodeToDepth, nodeToPositionData);
                            int pathIndex = 0;
                            NodeUI currentPathNode = path[pathIndex];

                            // For all horizons between the ancestor and multi-parent node, insert a virtual parent.
                            // start: one horizon up from current; end: one down from ancestor
                            int commonAncestorDepth = nodeToDepth[commonAncestor];
                            NodePositionData lastNodePosition = nodeData;
                            for (int i = horizon - 1; i > commonAncestorDepth; i--)
                            {
                                NodePositionData nextPositionData = new NodePositionData() { X = lastNodePosition.X };
                                nextPositionData.Children.Add(lastNodePosition);

                                // if the current path parent is on this horizon, place the virtual parent to its right
                                if (i == nodeToDepth[currentPathNode])
                                {
                                    nextPositionData.X = nodeToPositionData[currentPathNode].X + 1;
                                    pathIndex++;
                                    currentPathNode = path[pathIndex];
                                }

                                nodePositionDataByDepth[i].Add(nextPositionData);
                                lastNodePosition = nextPositionData;
                            }

                            NodePositionData ancestorData = nodeToPositionData[commonAncestor];
                            // insert into ancestor's child list in order by x coordinate
                            int index = ancestorData.Children.IndexOf(
                                ancestorData.Children.FirstOrDefault(c => c.X > lastNodePosition.X));
                            if (index == -1)
                            {
                                ancestorData.Children.Add(lastNodePosition);
                            }
                            else
                            {
                                ancestorData.Children.Insert(index, lastNodePosition);
                            }
                        }
                    }
                    nextY += k_VerticalSpacing + maxHeight;
                }

                // Upward -> calculate width needed under each node
                for (int horizon = maxDepth; horizon >= 0; horizon--)
                {
                    foreach (NodePositionData nodeData in nodePositionDataByDepth[horizon])
                    {
                        if (nodeData.Node != null) // virtual nodes don't propagate width requirements
                        {
                            nodeData.SubgraphWidth = Mathf.Max(nodeData.Width,
                                nodeData.Children.Sum(c => c.SubgraphWidth + k_HorizontalSpacing) - k_HorizontalSpacing);
                        }
                    }
                }

                // Downward -> for each node, set its children's x positions
                for (int horizon = 0; horizon < maxDepth; horizon++)
                {
                    foreach (NodePositionData nodeData in nodePositionDataByDepth[horizon])
                    {
                        float nextX = nodeData.X - (nodeData.Children.Sum(c => c.SubgraphWidth + k_HorizontalSpacing) - k_HorizontalSpacing) / 2;
                        foreach (NodePositionData childData in nodeData.Children)
                        {
                            childData.X = nextX + childData.SubgraphWidth / 2;
                            nextX += childData.SubgraphWidth + k_HorizontalSpacing;
                        }
                    }
                }
            }
            
            // Return start position, end position data.
            return nodePositionDataByDepth.SelectMany(nodeData => nodeData)
                .Where(nodeData => nodeData.Node != null) // filter out virtual tree nodes
                .Select(nodeData =>
                {
                    float x = nodeData.X;
                    return new KeyValuePair<NodeUI, Vector2>(nodeData.Node, new Vector2(x, nodeData.Y));
                });
        }
        
        private static NodeUI FindCommonAncestorOfParents(NodeUI node, Dictionary<NodeUI, int> nodeDepths)
        {
            Queue<NodeUI> ancestorTraversalQueue = new Queue<NodeUI>();
            HashSet<NodeUI> commonAncestors = new HashSet<NodeUI>(nodeDepths.Keys);
            HashSet<NodeUI> parentAncestors = new HashSet<NodeUI>();
            
            foreach (NodeUI parent in node.GetParentNodeUIs().Where(nodeDepths.ContainsKey))
            {
                parentAncestors.Clear();
                ancestorTraversalQueue.Enqueue(parent);
                while (ancestorTraversalQueue.TryDequeue(out NodeUI ancestor))
                {
                    if (parentAncestors.Contains(ancestor))
                        continue;
                    
                    parentAncestors.Add(ancestor);
                    foreach (NodeUI ancestorParent in ancestor.GetParentNodeUIs().Where(nodeDepths.ContainsKey))
                    {
                        ancestorTraversalQueue.Enqueue(ancestorParent);
                    }
                }

                commonAncestors.IntersectWith(parentAncestors);
            }
            
            NodeUI deepestCommonAncestor = commonAncestors.Aggregate((a1, a2) => nodeDepths[a1] > nodeDepths[a2] ? a1 : a2);
            return deepestCommonAncestor;
        }

        private static List<NodeUI> FindRightMostPathToAncestor(NodeUI node, NodeUI ancestor, Dictionary<NodeUI, int> nodeDepths, Dictionary<NodeUI, NodePositionData> positionData)
        {
            List<NodeUI> path = new List<NodeUI>{ node }; // start with the node
            
            NodeUI current = node;
            List<NodeUI> parents = current.GetParentNodeUIs().Where(nodeDepths.ContainsKey).ToList();
            while (current != ancestor && parents.Count > 0)
            {
                NodeUI rightMostParent = parents.Aggregate((p1, p2) => positionData[p1].X > positionData[p2].X ? p1 : p2);
                path.Add(rightMostParent);
                current = rightMostParent;
                parents = current.GetParentNodeUIs().Where(nodeDepths.ContainsKey).ToList();
            }

            return path;
        }

        internal static bool IsAnAncestor(NodeUI node, NodeUI targetNode)
        {
            if (ReferenceEquals(node, targetNode))
            {
                return false;
            }
            NodeModel targetModel = targetNode.Model;

            Stack<NodeModel> nodeQueue = new Stack<NodeModel>(4);
            nodeQueue.Push(node.Model);
            while (nodeQueue.Count != 0)
            {
                NodeModel currentNodeModel = nodeQueue.Pop();
                if (ReferenceEquals(currentNodeModel, targetModel))
                {
                    return true;
                }

                foreach (PortModel connectedPort in currentNodeModel.IncomingConnections)
                {
                    nodeQueue.Push(connectedPort.NodeModel);
                }
            }
            return false;
        }
        
        internal static Vector2 GetCenterPointOfNodes(List<NodeModel> nodes)
        {
            if (nodes.Count == 1)
            {
                return nodes[0].Position;
            }
            
            float minX = nodes.Min(n => n.Position.x);
            float maxX = nodes.Max(n => n.Position.x);
            float minY = nodes.Min(n => n.Position.y);
            float maxY = nodes.Max(n => n.Position.y);
            Vector2 centerPosition = new Vector2((minX + maxX)/2, (minY+ maxY)/2);
            return centerPosition;
        }

    }
}
#endif