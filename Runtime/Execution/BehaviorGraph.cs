using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using Unity.Properties;
using UnityEngine;
using Status = Unity.Behavior.Node.Status;

namespace Unity.Behavior
{
    /// <summary>
    /// BehaviorGraph holds all the runtime graph instances linked together into a complete behaviour
    /// defined within a BehaviorAuthoringGraph.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    public partial class BehaviorGraph : ScriptableObject, ISerializationCallbackReceiver
    {
        internal static readonly SerializableGUID k_GraphOwnerID = new SerializableGUID(1, 0);

        /// <summary>
        /// The blackboard reference used for accessing variables.
        /// </summary>
        public BlackboardReference BlackboardReference => RootGraph?.BlackboardReference;
        
        /// <summary>
        /// True if the graph is running, false otherwise.
        /// </summary>
        public bool IsRunning => RootGraph?.Root is { CurrentStatus: Status.Running or Status.Waiting };

        /// <summary>
        /// The set of linked graphs that make up the behaviour.
        /// </summary>
        [SerializeReference]
        internal List<BehaviorGraphModule> Graphs = new();

        /// <summary>
        /// The primary entry point for the behaviour defined by the BehaviorAuthoringGraph.
        /// </summary>
        [SerializeReference]
        internal BehaviorGraphModule RootGraph;

        [SerializeReference, DontCreateProperty]
        internal BehaviorGraphDebugInfo m_DebugInfo;

        /// <summary>
        /// Begins execution of the behavior graph.
        /// </summary>
        public void Start()
        {
            if (RootGraph?.Root != null)
            {
                RootGraph.StartNode(RootGraph.Root);
            }
        }

        /// <summary>
        /// Executes one step of the graph.
        /// </summary>
        public void Tick()
        {
            RootGraph?.Tick();
        }

        /// <summary>
        /// Ends the execution of the behavior graph.
        /// </summary>
        public void End()
        {
            if (RootGraph?.Root != null)
            {
                RootGraph.EndNode(RootGraph.Root);
            }
        }

        /// <summary>
        /// Resets the execution state and restarts the graph.
        /// </summary>
        public void Restart()
        {
            EndAndResetGraphs();
            Start();
        }

        /// <summary>
        /// Ends the current execution of the graph and resets the execution state.
        /// </summary>
        internal void EndAndResetGraphs()
        {
            End();
            foreach (BehaviorGraphModule graphModule in Graphs)
            {
                graphModule.Reset();
            }
        }

        /// <summary>
        /// Raise OnRuntimeSerialize in each BehaviorGraphModule to notify nodes.
        /// </summary>
        internal void SerializeGraphModules()
        {
            for (int i = Graphs.Count - 1; i >= 0; i--)
            {
                Graphs[i].Serialize();
            }
        }

        /// <summary>
        /// Raise OnRuntimeDeserialize in each BehaviorGraphModule to notify nodes.
        /// </summary>
        internal void DeserializeGraphModules()
        {
            for (int i = Graphs.Count - 1; i >= 0; i--)
            {
                Graphs[i].Deserialize();
            }
        }

        /// <inheritdoc cref="OnBeforeSerialize"/>
        public void OnBeforeSerialize()
        {
        }

        /// <inheritdoc cref="OnAfterDeserialize"/>
        public void OnAfterDeserialize()
        {
#if DEBUG && UNITY_EDITOR
            foreach (BehaviorGraphModule graph in Graphs)
            {
                graph.DebugInfo = m_DebugInfo;
            }
#endif
        }
    }
}