using System;
using Unity.Behavior.GraphFramework;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// A base class for behaviour nodes used in the Muse Behavior graph.
    /// </summary>
    [Serializable]
    public abstract class Node
    {
        /// <summary>
        /// Status of a graph node.
        /// </summary>
        public enum Status
        {
            /// <summary> The node has not started.</summary>
            Uninitialized,
            /// <summary> The node is currently running. </summary>
            Running,
            /// <summary> The node has succeeded. </summary>
            Success,
            /// <summary> The node has failed. </summary>
            Failure,
            /// <summary> The node is currently waiting for child nodes to complete. </summary>
            Waiting,
        }

        /// <summary>
        /// The current status of the graph node.
        /// </summary>
        [CreateProperty]
        public Status CurrentStatus { get; protected set; } = Status.Uninitialized;

        // Later we could store additional information here to provide more context to the graph.
        /// <summary>
        /// Log failure to the editor with additional contextual data.
        /// </summary>
        /// <param name="reason">Reason for the failure.</param>
        /// <param name="isError">If true, report the message using LogError instead of LogWarning.</param>
        public void LogFailure(string reason, bool isError = false)
        {
            if (isError)
            {
                Debug.LogError($"{this.GetType().Name}: {reason}", GameObject);
            }
            else
            {
                Debug.LogWarning($"{this.GetType().Name}: {reason}", GameObject);
            }
        }

        /// <summary>
        /// Sets the current status of the node.
        /// </summary>
        /// <param name="status"></param>
        internal void SetCurrentStatus(Status status)
        {
            CurrentStatus = status;
        }

        /// The current active state of the node. Unlike the Status, **IsRunning** is specifically used in graph operations
        /// to determine the node's lifecycle.
        /// </summary>
        [CreateProperty]
        internal bool IsRunning { get; set; } = false;

        /// <summary>
        /// The unique ID assigned to the node.
        /// </summary>
        [SerializeField]
        internal SerializableGUID ID;

        /// <summary>
        /// The BehaviorGraph containing the node instance.
        /// </summary>
        [SerializeReference, DontCreateProperty]
        internal BehaviorGraphModule Graph;

        /// <summary>
        /// The game object associated with the behavior graph.
        /// </summary>
        public GameObject GameObject => Graph.GameObject;

        /// <summary>
        /// The constructor for Node, currently internal to prevent creation of new node base classes.
        /// </summary>
        internal Node()
        { }

        internal Status Start()
        {
            CurrentStatus = Status.Running;
            IsRunning = true;
#if DEBUG && UNITY_EDITOR
            // The user set a breakpoint in the graph editor. Call a break on the debugger.
            if (Graph.ShouldDebuggerBreak(ID)) System.Diagnostics.Debugger.Break();
#endif
            return OnStart();
        }

        internal Status Update()
        {
#if DEBUG && UNITY_EDITOR
            // The user set a breakpoint in the graph editor. Call a break on the debugger.
            if (Graph.ShouldDebuggerBreak(ID)) System.Diagnostics.Debugger.Break();
#endif
            return OnUpdate();
        }

        internal void End()
        {
            IsRunning = false;
#if DEBUG && UNITY_EDITOR
            // The user set a breakpoint in the graph editor. Call a break on the debugger.
            if (Graph.ShouldDebuggerBreak(ID)) System.Diagnostics.Debugger.Break();
#endif
            OnEnd();
        }

        internal void Serialize()
        {
            OnSerialize();
        }

        internal void Deserialize()
        {
            OnDeserialize();
        }

        /// <summary>
        /// OnStart is called when the node starts running.
        /// </summary>
        /// <returns>The status of the node.</returns>
        protected virtual Status OnStart() => Status.Running;

        /// <summary>
        /// OnUpdate is called each frame while the node is running.
        /// </summary>
        /// <returns>The status of the node.</returns>
        protected virtual Status OnUpdate() => Status.Success;

        /// <summary>
        /// OnEnd is called when the node has stopped running.
        /// </summary>
        protected virtual void OnEnd()
        { }

        /// <summary>
        /// AwakeParents is called after the running node has returned a status of Success or Fail.
        /// </summary>
        protected internal virtual void AwakeParents()
        { }

        /// <summary>
        /// Resets the current status of the node.
        /// </summary>
        protected internal virtual void ResetStatus()
        {
            CurrentStatus = Status.Uninitialized;
        }
        
        /// <inheritdoc cref="BehaviorGraphModule.StartNode"/>
        protected Status StartNode(Node node)
        {
            return Graph.StartNode(node);
        }

        /// <inheritdoc cref="BehaviorGraphModule.EndNode"/>
        protected void EndNode(Node node)
        {
            // Catch if the user has called EndNode() recursively.
            if (Graph.IsEndingBranch)
            {
                Debug.LogError($"EndNode() has been called recursively from {this}. This is not allowed."
                               + " Ensure that EndNode() is not called from within OnEnd().");
                return;
            }
            Graph.EndNode(node);
        }

        /// <inheritdoc cref="BehaviorGraphModule.AwakeNode"/>
        protected void AwakeNode(Node node)
        {
            Graph.AwakeNode(node);
        }

        /// <summary>
        /// Adds a parent to this node
        /// </summary>
        /// <param name="parent"></param>
        internal abstract void AddParent(Node parent);

        /// <summary>
        /// Message raised before a graph is serialized. Can be use to prepare data for deserialization.
        /// </summary>
        protected virtual void OnSerialize()
        { }

        /// <summary>
        /// Message raised after a graph is deserialized. 
        /// Can be use to restart waiting nodes or reconstruct complex data that cannot be serialized.
        /// </summary>
        protected virtual void OnDeserialize()
        { }
    }
}