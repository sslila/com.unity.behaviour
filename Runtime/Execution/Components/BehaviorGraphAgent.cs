using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GenericSerializeReference;
using Unity.Behavior.GraphFramework;
#if NETCODE_FOR_GAMEOBJECTS
using Unity.Netcode;
#endif
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// Muse Behavior agent component.
    /// </summary>
    [AddComponentMenu("AI/Behavior Agent")]
#if NETCODE_FOR_GAMEOBJECTS
    public class BehaviorGraphAgent : NetworkBehaviour, ISerializationCallbackReceiver
#else
    public class BehaviorGraphAgent : MonoBehaviour, ISerializationCallbackReceiver
#endif
    {
        [SerializeReference]
        private BehaviorGraph m_Graph;

        /// <summary>
        /// The graph of behaviours to be executed by the agent.
        /// </summary>
        public BehaviorGraph Graph
        {
            get => m_Graph;
            set
            {
                m_Graph = value;
                OnAssignBehaviorGraph();
            }
        }
        
        /// <summary>
        /// The blackboard associated with the agent's graph.
        /// </summary>
        public BlackboardReference BlackboardReference => m_Graph ? m_Graph.BlackboardReference : null;
     
        /// <summary>
        /// UnityEngine.Object references, serialized separately from other variable types.
        /// </summary>
        [SerializeReference, HideInInspector]
        private List<BlackboardVariable> m_BlackboardVariableOverridesList = new ();
        internal Dictionary<SerializableGUID, BlackboardVariable> m_BlackboardOverrides = new ();

        private bool m_IsInitialised = false;
#if NETCODE_FOR_GAMEOBJECTS
        public bool NetcodeRunOnlyOnOwner = false;
        public override void OnNetworkSpawn()
        {
            if (!IsOwner && NetcodeRunOnlyOnOwner)
            {
                enabled = false;
                return;
            }
        }
#endif
        private void Awake()
        {
            Init();
        }

        private void OnAssignBehaviorGraph()
        {
            if (!m_Graph)
            {
                return;
            }
            
            AssignGameObjectToGraphModules();
            
            // If the graph or blackboard are null, we can't sync the overrides, so return.
            if (m_Graph.BlackboardReference?.Blackboard == null)
            {
                m_BlackboardOverrides.Clear();
                return;
            }
            
            // Update overrides to match blackboard.
            SynchronizeOverridesWithBlackboard();
            
            // Automatically assign graph owner variable to this game object.
            SerializableGUID graphOwnerID = BehaviorGraph.k_GraphOwnerID;
            if (m_BlackboardOverrides.TryGetValue(graphOwnerID, out BlackboardVariable ownerVariableOverride))
            {
                // An override already exists, so set its value to this GameObject.
                ownerVariableOverride.ObjectValue = gameObject;
                // Set the blackboard owner variable value to this GameObject.
                if (BlackboardReference.GetVariable(graphOwnerID, out BlackboardVariable ownerVariable))
                {
                    ownerVariable.ObjectValue = gameObject;   
                }
            }
            else if (BlackboardReference.GetVariable(graphOwnerID, out BlackboardVariable ownerVariable))
            {
                // No override exists, but a blackboard variable for the graph owner exists, so add an override.
                m_BlackboardOverrides.Add(graphOwnerID, new BlackboardVariableGameObject(gameObject)
                {
                    GUID = graphOwnerID, 
                    Name = ownerVariable.Name
                });
            }
        }
        
        internal void SynchronizeOverridesWithBlackboard()
        {
            // A new instance of a runtime graph has been assigned. Remove any out-of-date variable overrides.
            foreach ((SerializableGUID guid, _) in m_BlackboardOverrides.ToList())
            {
                if (m_Graph.BlackboardReference.Blackboard.Variables.Any(bbVariable => bbVariable.GUID == guid))
                {
                    continue;
                }
                m_BlackboardOverrides.Remove(guid);
            }
            
            // Ensure override names are up to date.
            foreach (BlackboardVariable variable in m_Graph.BlackboardReference.Blackboard.Variables)
            {
#if UNITY_EDITOR
                // This strange case sometimes happens when the inspector is open during a domain reload.
                if (variable == null) continue;
#endif
                if (m_BlackboardOverrides.TryGetValue(variable.GUID, out var overrideVariable))
                {
                    // An override already exists, so update its name if necessary.
                    if (overrideVariable.Name != variable.Name)
                    {
                        overrideVariable.Name = variable.Name;
                    }
                }
            }

            // Ensure Self variable is set.
            SerializableGUID graphOwnerID = BehaviorGraph.k_GraphOwnerID;
            if (m_BlackboardOverrides.TryGetValue(graphOwnerID, out BlackboardVariable ownerVariableOverride))
            {
                if (ownerVariableOverride.ObjectValue != null)
                {
                    return;
                }

                ownerVariableOverride.ObjectValue = gameObject;
                // Set the blackboard owner variable value to this GameObject.
                if (BlackboardReference.GetVariable(graphOwnerID, out BlackboardVariable ownerVariable))
                {
                    ownerVariable.ObjectValue = gameObject;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the agent's behavior graph.
        /// </summary>
        public void Init()
        {
            if (m_Graph == null )
            {
                return;
            }

            if (m_IsInitialised)
            {
                AssignGameObjectToGraphModules();
                return;
            }
            
            m_Graph = ScriptableObject.Instantiate(m_Graph);
            AssignGameObjectToGraphModules();
            InitChannelsAndMetadata();
            m_IsInitialised = true;
        }

        private void AssignGameObjectToGraphModules()
        {
            m_Graph.RootGraph.GameObject = gameObject;
            foreach (var graphModule in m_Graph.Graphs)
            {
                graphModule.GameObject = gameObject;   
            }
        }

        /// <summary>
        /// Gets a variable associated with the specified name and value type. For values of type subclassed from
        /// UnityEngine.Object, use the non-generic method.
        /// </summary>
        /// <param name="variableName">The name of the variable</param> 
        /// <param name="variable">The blackboard variable matching the name and value type</param>
        /// <typeparam name="TValue">The type of value stored by the variable</typeparam>
        /// <returns>Returns true if a variable matching the name and type is found. Returns false otherwise.</returns>
        public bool GetVariable<TValue>(string variableName, out BlackboardVariable<TValue> variable)
        {
            if (m_Graph.RootGraph.GetVariable(variableName, out variable))
            {
                return true;
            }

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.GetVariable(variableName, out variable))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets the variable associated with the specified name.
        /// </summary>
        /// <param name="variableName">The name of the variable</param> 
        /// <param name="variable">Contains the value associated with the specified name, if the named variable is found;
        /// otherwise, the default value is assigned.</param>
        /// <returns>Returns true if a variable matching the name and type is found. Returns false otherwise.</returns>
        public bool GetVariable(string variableName, out BlackboardVariable variable)
        {
            if (m_Graph.RootGraph.GetVariable(variableName, out variable))
            {
                return true;
            }

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.GetVariable(variableName, out variable))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets a variable associated with the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the variable to get</param>
        /// <param name="variable">The variable associated with the specified GUID.</param>
        /// <returns>Returns true if a variable with a matching GUID was found and false otherwise.</returns>
        public bool GetVariable(SerializableGUID guid, out BlackboardVariable variable)
        {
            if (m_Graph.RootGraph.GetVariable(guid, out variable))
            {
                return true;
            }

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.GetVariable(guid, out variable))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Gets a variable associated with the specified GUID and value type.
        /// </summary>
        /// <param name="guid">The GUID of the variable to get</param>
        /// <param name="variable">The variable associated with the specified GUID.</param>
        /// <typeparam name="TValue">The value type of the variable</typeparam>
        /// <returns>Returns true if a variable with a matching GUID and type was found and false otherwise.</returns>
        public bool GetVariable<TValue>(SerializableGUID guid, out BlackboardVariable<TValue> variable)
        {
            if (m_Graph.RootGraph.GetVariable(guid, out variable))
            {
                return true;
            }

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.GetVariable(guid, out variable))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <inheritdoc cref="Behavior.BlackboardReference.GetVariableID"/>
        public bool GetVariableID(string variableName, out SerializableGUID id)
        {
            if (m_Graph.RootGraph.GetVariableID(variableName, out id))
            {
                return true;
            }

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.GetVariableID(variableName, out id))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Sets the value of a blackboard variable matching the specified name and value type.
        /// </summary>
        /// <param name="variableName">The name of the variable</param>
        /// <param name="value">The value to assign to the variable</param>
        /// <typeparam name="TValue">The type of value stored by the variable</typeparam>
        /// <returns>Returns true if a variable matching the name and type is found and set. Returns false otherwise.</returns>
        public bool SetVariableValue<TValue>(string variableName, TValue value)
        {
            m_Graph.RootGraph?.SetVariableValue(variableName, value);

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.SetVariableValue(variableName, value))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Sets the value of the variable associated with the specified GUID.
        /// </summary>
        /// <param name="guid">The guid associated with the variable</param>
        /// <param name="value">The value to assign to the variable</param>
        /// <typeparam name="TValue">The value type of the variable</typeparam>
        /// <returns>Returns true if the value was set successfully and false otherwise.</returns>
        public bool SetVariableValue<TValue>(SerializableGUID guid, TValue value)
        {
            m_Graph.RootGraph?.SetVariableValue(guid, value);

            foreach (var behaviorGraphModule in m_Graph.Graphs)
            {
                if (behaviorGraphModule.SetVariableValue(guid, value))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Deserializes data on to the associated BehaviorGraph.
        /// </summary>
        /// <param name="serialized">Serialized data.</param>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="resolver">Object resolver to use.</param>
        /// <typeparam name="TSerializedFormat">Type of serialized data.</typeparam>
        public void Deserialize<TSerializedFormat>(TSerializedFormat serialized, RuntimeSerializationUtility.IBehaviorSerializer<TSerializedFormat> serializer, RuntimeSerializationUtility.IUnityObjectResolver<string> resolver)
        {
            m_Graph = ScriptableObject.CreateInstance<BehaviorGraph>();
            serializer.Deserialize(serialized, m_Graph, resolver);
            InitChannelsAndMetadata(applyOverride: false);
            m_Graph.DeserializeGraphModules();
        }

        /// <summary>
        /// Serializes the associated BehaviorGraph to data of TSerializedFormat type.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="resolver">Object resolver to use.</param>
        /// <typeparam name="TSerializedFormat">Type of serialized output.</typeparam>
        /// <returns>Serialized data.</returns>
        public TSerializedFormat Serialize<TSerializedFormat>(RuntimeSerializationUtility.IBehaviorSerializer<TSerializedFormat> serializer, RuntimeSerializationUtility.IUnityObjectResolver<string> resolver)
        {
            m_Graph.SerializeGraphModules();
            return serializer.Serialize(m_Graph, resolver);
        }

        private void InitChannelsAndMetadata(bool applyOverride = true)
        {
            if (applyOverride)
            {
                ApplyBlackboardOverrides();
            }
            
            // Initialize default event channels for unassigned channel variables.
            foreach (BehaviorGraphModule graph in Graph.Graphs)
            {
                foreach (BlackboardVariable variable in graph.Blackboard.Variables)
                {
                    if (typeof(EventChannelBase).IsAssignableFrom(variable.Type) && variable.ObjectValue == null)
                    {
                        ScriptableObject channel = ScriptableObject.CreateInstance(variable.Type);
                        channel.name = $"Default {variable.Name} Channel";
                        variable.ObjectValue = channel;
                    }
                }

                foreach (Node node in graph.Nodes())
                {
                    node.Graph = graph;
                }
            }
            BlackboardReference.Blackboard.CreateMetadata();
        }

        /// <summary>
        /// Begins execution of the agent's behavior graph.
        /// </summary>
        public void Start()
        {
#if NETCODE_FOR_GAMEOBJECTS
            if (!IsOwner && NetcodeRunOnlyOnOwner) return;
#endif
            if (m_Graph)
            {
                m_Graph.Start();
            }
        }

        /// <summary>
        /// Ends the execution of the agent's behavior graph.
        /// </summary>
        public void End()
        {
#if NETCODE_FOR_GAMEOBJECTS
            if (!IsOwner && NetcodeRunOnlyOnOwner) return;
#endif
            if (m_Graph)
            {
                m_Graph.End();
            }
        }
        
        /// <summary>
        /// Restarts the execution of the agent's behavior graph.
        /// </summary>
        public void Restart()
        {
#if NETCODE_FOR_GAMEOBJECTS
            if (!IsOwner && NetcodeRunOnlyOnOwner) return;
#endif
            if (m_Graph)
            {
                m_Graph.Restart();
            }
        }

        private void Update()
        {
            if (!m_IsInitialised)
            {
                return;
            }
            
#if NETCODE_FOR_GAMEOBJECTS
            if (!IsOwner && NetcodeRunOnlyOnOwner) return;
#endif
            if (m_Graph)
            {
                m_Graph.Tick();
            }
        }

#if NETCODE_FOR_GAMEOBJECTS
        public override void OnDestroy()
        {
            base.OnDestroy();
#else
        private void OnDestroy()
        {
#endif
            if (m_Graph)
            {
                m_Graph.EndAndResetGraphs();
            }
        }
        
        /// <summary>
        /// Applies the agent's variable overrides to the blackboard.
        /// </summary>
        private void ApplyBlackboardOverrides()
        {
            foreach (var varOverride in m_BlackboardOverrides)
            {
                if (varOverride.Key == BehaviorGraph.k_GraphOwnerID && varOverride.Value is BlackboardVariable<GameObject> gameObjectBlackboardVariable && gameObjectBlackboardVariable.Value == null)
                {
                    gameObjectBlackboardVariable.Value = gameObject;
                }
                if (BlackboardReference != null && BlackboardReference.GetVariable(varOverride.Key, out BlackboardVariable var))
                {
                    var.ObjectValue = varOverride.Value.ObjectValue;
                }

                foreach (var graphModule in Graph.Graphs)
                {
                    if (graphModule.Blackboard != null && graphModule.BlackboardReference.GetVariable(varOverride.Key, out BlackboardVariable subGraphVariable))
                    {
                        subGraphVariable.ObjectValue = varOverride.Value.ObjectValue;
                    }
                    foreach (var blackboardReference in graphModule.BlackboardGroupReferences)
                    {
                        if (blackboardReference.GetVariable(varOverride.Key, out BlackboardVariable blackboardReferenceVar))
                        {
                            blackboardReferenceVar.ObjectValue = varOverride.Value.ObjectValue;
                        }
                    }
                }
            }
        }
        
        /// <inheritdoc cref="OnBeforeSerialize"/>
        public void OnBeforeSerialize()
        {
            if (!m_Graph)
            {
                return;
            }

            m_BlackboardVariableOverridesList.Clear();
            foreach (BlackboardVariable variable in m_BlackboardOverrides.Values)
            {
                m_BlackboardVariableOverridesList.Add(variable);
            }
        }

        /// <inheritdoc cref="OnAfterDeserialize"/>
        public void OnAfterDeserialize()
        {
            if (!Graph)
            {
                return;
            }
            
            m_BlackboardOverrides = new Dictionary<SerializableGUID, BlackboardVariable>();
            foreach (BlackboardVariable variable in m_BlackboardVariableOverridesList)
            {
                // Debug.LogError($"Add Overrride Create variable: {variable.Type}, {variable}");
                m_BlackboardOverrides.Add(variable.GUID, variable);
            }
        }
    }
}