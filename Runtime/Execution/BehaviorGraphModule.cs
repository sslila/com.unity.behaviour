using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Behavior.GraphFramework;
using Unity.Properties;
using UnityEngine;
using Status = Unity.Behavior.Node.Status;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace Unity.Behavior
{
    /// <summary>
    /// A directed graph of nodes specifying the order and conditional execution of sequential tasks.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    internal partial class BehaviorGraphModule
    {
        [SerializeField]
        public SerializableGUID AuthoringAssetID;

#if UNITY_EDITOR
        [SerializeReference,DontCreateProperty]
        private BehaviorGraphDebugInfo m_DebugInfo;
        public BehaviorGraphDebugInfo DebugInfo { get => m_DebugInfo; set => m_DebugInfo = value; }
#endif

        /// <summary>
        /// The root node of the graph
        /// </summary>
        [SerializeReference]
        public Node Root;

        /// <summary>
        /// A blackboard containing variables used by the graph.
        /// </summary>
        internal Blackboard Blackboard => BlackboardReference.Blackboard;

        [SerializeReference]
        public BlackboardReference BlackboardReference = new BlackboardReference();
        [SerializeReference]
        public List<BlackboardReference> BlackboardGroupReferences = new List<BlackboardReference>();
        [CreateProperty]
        public List<Node> ProcessedNodes = new List<Node>(4);
        [CreateProperty]
        private List<Node> m_RunningNodes = new List<Node>(4);
        [CreateProperty]
        private Stack<Node> m_NodesToEnd = new Stack<Node>(1);
        [CreateProperty]
        private HashSet<Node> m_EndedNodes = new HashSet<Node>();
        internal bool IsEndingBranch { get; private set; } = false;
        [CreateProperty]
        private bool m_NodesChanged;

        [SerializeField] [HideInInspector]
        private long m_VersionTimestamp;
        public long VersionTimestamp
        {
            get => m_VersionTimestamp;
            set => m_VersionTimestamp = value;
        }

        internal GameObject GameObject { get; set; }
        public delegate void GraphStatusChangeEventHandler(BehaviorGraphModule graph);
        public event GraphStatusChangeEventHandler OnGraphStatusChange;

        /// <summary>
        /// Executes one step of the graph.
        /// </summary>
        public void Tick()
        {
            if (m_NodesChanged)
            {
                RebuildNodeLists();
            }

            for (int i = 0; i < m_RunningNodes.Count; i++)
            {
                Node node = m_RunningNodes[i];
                if (!ProcessedNodes.Contains(node))
                {
                    continue; // node has been removed from processing set. Do not update.
                }

                Status status = node.Update();

                // check for change in status
                if (status != Status.Running)
                {
                    node.SetCurrentStatus(status);

                    m_NodesChanged = true;
                    if (status is Status.Success or Status.Failure)
                    {
                        EndNode(node);
                        node.AwakeParents();
                    }
                }
            }

            if (m_NodesChanged)
            {
                OnGraphStatusChange?.Invoke(this);
            }
        }

        /// <summary>
        /// Starts the specified node. If the node completes, the parent node is notified.
        /// </summary>
        /// <param name="node">The node to start</param>
        /// <returns>Returns the status of the node started.</returns>
        public Status StartNode(Node node)
        {
            node.ResetStatus();
            ProcessedNodes.Add(node);

            Status status = node.Start();
            node.SetCurrentStatus(status);

            if (status is Status.Success or Status.Failure)
            {
                EndNode(node);
                node.AwakeParents();
            }

            m_NodesChanged = true;
            return status;
        }

        /// <summary>
        /// Ends the execution of the specified node.
        /// </summary>
        /// <param name="node">The node for which execution should end</param>
        public void EndNode(Node node)
        {
            m_NodesChanged = true;
            EndBranch(node);
        }

        private void EndBranch(Node branchRoot)
        {
            // Don't end a branch if it has already been ended.
            if (m_EndedNodes.Contains(branchRoot))
            {
                return;
            }

            IsEndingBranch = true;
            m_NodesToEnd.Push(branchRoot);
            while (m_NodesToEnd.Count > 0)
            {
                // Post-order graph traversal
                Node currentNode = m_NodesToEnd.Peek();
                
                // At each node, push the first unprocessed child to the stack.
                Node unprocessedChild = GetFirstUnprocessedChild(currentNode);
                if (unprocessedChild != null)
                {
                    m_NodesToEnd.Push(unprocessedChild);
                    continue;
                }

                // If each child has been visited, pop the node and end it.
                m_NodesToEnd.Pop();
                ProcessedNodes.Remove(currentNode);
                if (currentNode.IsRunning)
                {
                    currentNode.End();
                }
                m_EndedNodes.Add(currentNode);
            }

            m_EndedNodes.Clear();
            IsEndingBranch = false;
            return;

            bool BranchNeedsToBeProcessed(Node node)
            {
                return node is { CurrentStatus: Status.Running or Status.Waiting } && !m_EndedNodes.Contains(node);
            }
            
            // Returns the first unprocessed child of the node. It does not search nodes beyond the immediate children.
            Node GetFirstUnprocessedChild(Node node)
            {
                switch (node)
                {
                    case Composite composite:
                        foreach (Node child in composite.Children)
                        {
                            if (BranchNeedsToBeProcessed(child))
                                return child;
                        }
                        return null;
                    case Modifier modifier:
                        return BranchNeedsToBeProcessed(modifier.Child) ? modifier.Child : null;
                    case Join join:
                        return BranchNeedsToBeProcessed(join.Child) ? join.Child : null;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Resets the state of the graph. Restarts the execution of the root node.
        /// </summary>
        public void Reset()
        {
            m_NodesChanged = false;
            ProcessedNodes.Clear();
            m_RunningNodes.Clear();
        }

        /// <summary>
        /// Awakens a node if it is currently waiting on a child node or branch.
        /// </summary>
        /// <param name="node">The node to awaken.</param>
        internal void AwakeNode(Node node)
        {
            if (ProcessedNodes.Contains(node) && node.CurrentStatus is Status.Waiting or Status.Uninitialized)
            {
                node.SetCurrentStatus(Status.Running);
                m_NodesChanged = true;
            }
        }

        private void RebuildNodeLists()
        {
            // Copy running nodes from processed list to running list
            m_RunningNodes.Clear();
            for (int i = 0; i < ProcessedNodes.Count; i++)
            {
                Node node = ProcessedNodes[i];
                if (node.CurrentStatus is Status.Running)
                {
                    m_RunningNodes.Add(node);
                }
            }

            m_NodesChanged = false;
        }

        private Stack<Node> m_NodeTraversalQueue;
        private HashSet<Node> m_VisitedNodes;

        internal IEnumerable<Node> Nodes()
        {
            m_NodeTraversalQueue ??= new Stack<Node>(4);
            m_VisitedNodes ??= new HashSet<Node>(4);
            m_VisitedNodes.Clear();

            void QueueNode(Node child)
            {
                if (child != null && !m_VisitedNodes.Contains(child))
                {
                    m_VisitedNodes.Add(child);
                    m_NodeTraversalQueue.Push(child);
                }
            }

            m_NodeTraversalQueue.Push(Root);
            m_VisitedNodes.Add(Root);
            while (m_NodeTraversalQueue.Count != 0)
            {
                var current = m_NodeTraversalQueue.Pop();
                switch (current)
                {
                    case Action:
                        break;
                    case Modifier modifier:
                        QueueNode(modifier.Child);
                        break;
                    case Composite composite:
                        for (var index = 0; index < composite.Children.Count; index++)
                        {
                            QueueNode(composite.Children[index]);
                        }
                        break;
                    case Join join:
                        QueueNode(join.Child);
                        break;
                }
            }

            return m_VisitedNodes;
        }

        /// <see cref="Behavior.BlackboardReference.GetVariable{TValue}(string,out Unity.Behavior.BlackboardVariable{TValue})"/>
        public bool GetVariable<TValue>(string variableName, out BlackboardVariable<TValue> variable)
        {
            if (BlackboardReference.GetVariable(variableName, out variable))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.GetVariable(variableName, out variable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <see cref="Behavior.BlackboardReference.GetVariable(string, out BlackboardVariable)"/>
        public bool GetVariable(string variableName, out BlackboardVariable variable)
        {
            if (BlackboardReference.GetVariable(variableName, out variable))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.GetVariable(variableName, out variable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <see cref="Behavior.BlackboardReference.GetVariable(Unity.Behavior.GraphFramework.SerializableGUID,out Unity.Behavior.BlackboardVariable)"/>
        public bool GetVariable(SerializableGUID guid, out BlackboardVariable variable)
        {
            if (BlackboardReference.GetVariable(guid, out variable))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.GetVariable(guid, out variable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <see cref="Behavior.BlackboardReference.GetVariable{TValue}(SerializableGUID, out BlackboardVariable{TValue})"/>
        public bool GetVariable<TValue>(SerializableGUID guid, out BlackboardVariable<TValue> variable)
        {
            if (BlackboardReference.GetVariable(guid, out variable))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.GetVariable(guid, out variable))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="Behavior.BlackboardReference.GetVariableID"/>
        public bool GetVariableID(string variableName, out SerializableGUID id)
        {
            if (BlackboardReference.GetVariableID(variableName, out id))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.GetVariableID(variableName, out id))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <see cref="Behavior.BlackboardReference.SetVariableValue{TValue}(SerializableGUID, TValue)"/>
        public bool SetVariableValue<TValue>(SerializableGUID guid, TValue value)
        {
            if (BlackboardReference.SetVariableValue(guid, value))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.SetVariableValue(guid, value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <see cref="Behavior.BlackboardReference.SetVariableValue{TValue}(string,TValue)"/>
        public bool SetVariableValue<TValue>(string variableName, TValue value)
        {
            if (BlackboardReference.SetVariableValue(variableName, value))
            {
                return true;
            }

            foreach (var blackboardReference in BlackboardGroupReferences)
            {
                if (blackboardReference.SetVariableValue(variableName, value))
                {
                    return true;
                }
            }

            return false;
        }

        public void Serialize()
        {
            HashSet<Node> candidates = new (10);
            foreach (Node node in ProcessedNodes)
            {
                GatherActiveNodes(node, ref candidates);
            }

            foreach (Node node in candidates)
            {
                node.Serialize();
            }
        }

        public void Deserialize()
        {
            HashSet<Node> candidates = new(10);
            foreach (Node node in ProcessedNodes)
            {
                GatherActiveNodes(node, ref candidates);
            }

            foreach (Node node in candidates)
            {
                node.Deserialize();
            }
        }

        private void GatherActiveNodes(Node origin, ref HashSet<Node> outCollection)
        {   
            // If the node is not running or already explored, we don't need to got further.
            if (!origin.IsRunning || outCollection.Contains(origin))
            {
                return;
            }

            outCollection.Add(origin);

            var composite = origin as Composite;
            if (composite != null)
            {
                foreach (var childNode in composite.Children)
                {
                    GatherActiveNodes(childNode, ref outCollection);
                }
                return;
            }

            var modifier = origin as Modifier;
            if (modifier != null)
            {
                GatherActiveNodes(modifier.Child, ref outCollection);
                return;
            }
        }

#if DEBUG && UNITY_EDITOR
        internal bool ShouldDebuggerBreak(SerializableGUID ID)
        {
            return m_DebugInfo != null && m_DebugInfo.IsNodeBreakpointEnabled(ID);
        }
#endif
    }
}