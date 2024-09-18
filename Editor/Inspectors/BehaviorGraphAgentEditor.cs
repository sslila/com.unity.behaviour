using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Behavior
{
    [CustomEditor(typeof(BehaviorGraphAgent))]
    [CanEditMultipleObjects]
    internal class BehaviorGraphAgentEditor : Editor
    {
        private readonly List<BehaviorGraphAgent> m_TargetAgents = new ();
        private bool m_ShowBlackboard = true;
        private readonly Dictionary<SerializableGUID, bool> m_ListVariableFoldoutStates = new Dictionary<SerializableGUID, bool>();
        private readonly Dictionary<SerializableGUID, VariableModel> m_VariableGUIDToVariableModel = new Dictionary<SerializableGUID, VariableModel>();
        private long m_MappedBlackboardVersion = 0;
        private BehaviorGraph m_SharedGraph;
        private BehaviorGraph SharedGraph
        {
            get => m_SharedGraph;
            set
            {
                if (ReferenceEquals(m_SharedGraph, value)) return;
                m_SharedGraph = value;
                if (m_SharedGraph == null)
                {
                    SharedAuthoringGraph = null;
                    return;
                }
                string assetPath = AssetDatabase.GetAssetPath(m_SharedGraph);
                if (string.IsNullOrEmpty(assetPath))
                {
                    return;
                }
                SharedAuthoringGraph = AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(assetPath);
            }
        }

        private BehaviorAuthoringGraph m_SharedAuthoringGraph;
        private BehaviorAuthoringGraph SharedAuthoringGraph
        {
            get => m_SharedAuthoringGraph;
            set
            {
                m_SharedAuthoringGraph = value;
                UpdateVariableModelMap();
            }
        }

        private void UpdateVariableModelMap()
        {
            if (SharedAuthoringGraph == null)
            {
                m_VariableGUIDToVariableModel.Clear();
                return;
            }

            if (m_VariableGUIDToVariableModel.Count > 0 && m_MappedBlackboardVersion == SharedAuthoringGraph.VersionTimestamp)
            {
                return;
            }
            
            m_VariableGUIDToVariableModel.Clear();
            m_MappedBlackboardVersion = SharedAuthoringGraph.VersionTimestamp;
            foreach (var variableModel in SharedAuthoringGraph.Blackboard.Variables)
            {
                // Debug.LogError($"UpdateVariableModelMap: {variableModel.Type}");
                m_VariableGUIDToVariableModel.Add(variableModel.ID, variableModel);
            }
        }

        // Note: Target.Graph is set to a non-persistent copy when the agent is has been initialized at runtime.
        private bool HasRuntimeGraphAssetBeenDeleted(BehaviorGraphAgent agent) => 
            !ReferenceEquals(agent.Graph, null) && !EditorUtility.IsPersistent(agent.Graph);

        private void OnEnable()
        {
            // Update the target agents and check for deleted runtime graph assets.
            m_TargetAgents.Clear();
            foreach (UnityEngine.Object objTarget in targets)
            {
                var targetAgent = objTarget as BehaviorGraphAgent;
                m_TargetAgents.Add(targetAgent);
                UpdateBehaviorGraphIfNeeded(targetAgent);
            }
        }

        private void FindSharedGraph()
        {
            // Use the first target to check for mixed values
            BehaviorGraphAgent firstTarget = m_TargetAgents[0];
            bool targetsShareGraph = true;
            foreach (var targetAgent in m_TargetAgents)
            {
                if (!HaveSameGraph(targetAgent, firstTarget))
                {
                    targetsShareGraph = false;
                    break;
                }
            }
            SharedGraph = targetsShareGraph ? firstTarget.Graph : null;
        }

        DataType GetVariableDataCopy<DataType>(BlackboardVariable<DataType> blackboardVariable)
        {
            if (blackboardVariable.ObjectValue is ValueType)
            {
                return blackboardVariable.Value;
            }
            return Util.GetVariableValueCopy(blackboardVariable.Value);
        }

        public override void OnInspectorGUI()
        {
            FindSharedGraph();
            // Draw the graph field. If a new runtime graph is assigned, set the graph on the target and mark it dirty.
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = SharedGraph == null && targets.Length > 1;
            var graph = EditorGUILayout.ObjectField("Behavior Graph", SharedGraph, typeof(BehaviorGraph), false) as BehaviorGraph;
            if (EditorGUI.EndChangeCheck())
            {
                AssignGraphToAgents(graph);
                SharedGraph = graph;
            }
            DetectAssetDragDrop();

#if NETCODE_FOR_GAMEOBJECTS
            EditorGUI.BeginChangeCheck();
            BehaviorGraphAgent firstTarget = m_TargetAgents[0];
            bool netcodeRunOnlyOnOwner = EditorGUILayout.Toggle("Netcode: Run only on Owner", firstTarget.NetcodeRunOnlyOnOwner);

            if (EditorGUI.EndChangeCheck())
            {
                foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
                {
                    targetAgent.NetcodeRunOnlyOnOwner = netcodeRunOnlyOnOwner;
                    EditorUtility.SetDirty(targetAgent);
                }
            }
#endif

            // Update overrides list before drawing the blackboard.
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                if (targetAgent.BlackboardReference != null)
                {
                    targetAgent.SynchronizeOverridesWithBlackboard();
                }
            }
            
            // Draw a blackboard only if all agents share the same graph and a blackboard for it exists.  
            if (SharedGraph != null && SharedGraph.BlackboardReference?.Blackboard != null && SharedGraph.BlackboardReference?.Blackboard != null)
            {
                UpdateVariableModelMap();
                DrawBlackboard(SharedGraph.BlackboardReference.Blackboard.Variables);
            }
        }

        private void AssignGraphToAgents(BehaviorGraph graph)
        {
            // Assign the new graph to all targets.
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                if (graph == targetAgent.Graph)
                {
                    continue;
                }
                targetAgent.Graph = graph;
                EditorUtility.SetDirty(targetAgent);
            }

            // If the application is playing, initialize and start the targets, creating instances of the graph.
            if (Application.isPlaying)
            {
                BehaviorGraphAgent firstTarget = m_TargetAgents[0];
                firstTarget.StartCoroutine(InitializeAndStartTargets());
            }
        }

        private bool DetectAssetDragDrop()
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            EventType eventType = Event.current.type;
            if (lastRect.Contains(Event.current.mousePosition) &&
                (eventType == EventType.DragUpdated || eventType == EventType.DragPerform))
            {
                if (DragAndDrop.objectReferences.Length == 1 && typeof(BehaviorAuthoringGraph).IsAssignableFrom(DragAndDrop.objectReferences[0].GetType()))
                {
                    BehaviorAuthoringGraph authoringGraph = (BehaviorAuthoringGraph)DragAndDrop.objectReferences[0];
                    Event.current.Use();
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        var runtimeGraph = BehaviorAuthoringGraph.GetOrCreateGraph(authoringGraph);
                        if (runtimeGraph?.RootGraph == null)
                        {
                            authoringGraph.BuildRuntimeGraph();
                        }
                        AssignGraphToAgents(runtimeGraph);
                        SharedGraph = runtimeGraph;
                        return true;
                    }
                }       
            }
            return false;
        }

        private bool IsVariablePublic(BlackboardVariable variable)
        {
            if (m_VariableGUIDToVariableModel.TryGetValue(variable.GUID, out VariableModel variableModel))
            {
                return variableModel.IsExposed;
            }
            // Couldn't find the variable model! If the shared authoring graph is null, continue as if it's public.
            return SharedAuthoringGraph == null;
        }

        private void DrawBlackboard(IEnumerable<BlackboardVariable> variables)
        {
            m_ShowBlackboard = EditorGUILayout.Foldout(m_ShowBlackboard, "Blackboard Variables");
            if (!m_ShowBlackboard)
            {
                return;
            }

            EditorGUI.indentLevel++;
            foreach (BlackboardVariable variable in variables)
            {
                if (!IsVariablePublic(variable))
                {
                    continue;
                }
                EditorGUI.showMixedValue = false;
                bool isOverride = false;
                BlackboardVariable firstTargetVariable = null;
                foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
                {
                    BlackboardVariable targetVariable = GetTargetVariable(targetAgent, variable.GUID);
                    firstTargetVariable ??= targetVariable;
                    if (targetVariable == null)
                    {
                        Debug.LogError($"Variable {variable.Name} not found in blackboard of {targetAgent}.", targetAgent);
                        continue;
                    }

                    if (targetVariable.Name != variable.Name)
                    {
                        // The variable override exists, but its name may not be current if renamed in the asset.
                        targetVariable.Name = variable.Name;
                        EditorUtility.SetDirty(targetAgent);
                    }

                    // If any target's variable value differs from the first target's, show a mixed value indicator.
                    EditorGUI.showMixedValue |= !firstTargetVariable.ValueEquals(targetVariable);
                    
                    if (targetAgent.m_BlackboardOverrides.ContainsKey(variable.GUID))
                    {
                        // Is the variable we're checking the graph owner and is it set to the target agent?
                        bool isGraphOwnerVariableOverriden = variable.GUID == BehaviorGraph.k_GraphOwnerID && ReferenceEquals(targetAgent.m_BlackboardOverrides[variable.GUID].ObjectValue, targetAgent.gameObject);
                        isOverride = !isGraphOwnerVariableOverriden;
                    }
                }

                DrawFieldForBlackboardVariable(firstTargetVariable, isOverride);
            }
            EditorGUI.indentLevel--;
        }

        private void DrawFieldForBlackboardVariable(BlackboardVariable variable, bool isOverride)
        {
            string varName = isOverride ? $"{variable.Name} (Override)" : variable.Name;
            GUIContent label = isOverride ? new GUIContent(varName, "The value of this variable has been changed from the value set on the graph asset.") : new GUIContent(varName);
            Type type = variable.Type;
            
            if (type == typeof(float) && variable is BlackboardVariable<float> floatVariable)
            {
                float value = floatVariable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.FloatField(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<float>) && variable is BlackboardVariable<List<float>> floatListVariable)
            {
                List<float> value = GetVariableDataCopy(floatListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.FloatField(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(double) && variable is BlackboardVariable<double> doubleVariable)
            {
                double value = doubleVariable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.DoubleField(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<double>) && variable is BlackboardVariable<List<double>> doubleListVariable)
            {
                List<double> value = GetVariableDataCopy(doubleListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.DoubleField(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(int) && variable is BlackboardVariable<int> intVariable)
            {
                int value = intVariable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.IntField(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<int>) && variable is BlackboardVariable<List<int>> intListVariable)
            {
                List<int> value = GetVariableDataCopy(intListVariable);
                EditorGUI.BeginChangeCheck();
                
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.IntField(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(bool) && variable is BlackboardVariable<bool> boolVariable)
            {
                bool value = boolVariable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.Toggle(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<bool>) && variable is BlackboardVariable<List<bool>> boolListVariable)
            {
                List<bool> value = GetVariableDataCopy(boolListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.Toggle(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(string) && variable is BlackboardVariable<string> stringVariable)
            {
                string value = stringVariable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.TextField(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<string>) && variable is BlackboardVariable<List<string>> stringListVariable)
            {
                List<string> value = GetVariableDataCopy(stringListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.TextField(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                }; 
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(Color) && variable is BlackboardVariable<Color> colorVariable)
            {
                Color value = colorVariable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.ColorField(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<Color>) && variable is BlackboardVariable<List<Color>> colorListVariable)
            {
                List<Color> value = GetVariableDataCopy(colorListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.ColorField(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(Vector4) && variable is BlackboardVariable<Vector4> vec4Variable)
            {
                Vector4 value = vec4Variable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.Vector4Field(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<Vector4>) && variable is BlackboardVariable<List<Vector4>> vec4ListVariable)
            {
                List<Vector4> value = GetVariableDataCopy(vec4ListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.Vector4Field(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(Vector3) && variable is BlackboardVariable<Vector3> vec3Variable)
            {
                Vector3 value = vec3Variable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.Vector3Field(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<Vector3>) && variable is BlackboardVariable<List<Vector3>> vec3ListVariable)
            {
                List<Vector3> value = GetVariableDataCopy(vec3ListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.Vector3Field(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(Vector2) && variable is BlackboardVariable<Vector2> vec2Variable)
            {
                Vector2 value = vec2Variable.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.Vector2Field(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<Vector4>) && variable is BlackboardVariable<List<Vector4>> vec2ListVariable)
            {
                List<Vector4> value = GetVariableDataCopy(vec2ListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.Vector2Field(rect, $"Element {index}", value[index]);
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(GameObject) && variable is BlackboardVariable<GameObject> gameObjectVar)
            {
                GameObject value = gameObjectVar.Value;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.ObjectField(label, value, typeof(GameObject),true) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<GameObject>) && variable is BlackboardVariable<List<GameObject>> gameObjectListVariable)
            {
                List<GameObject> value = GetVariableDataCopy(gameObjectListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.ObjectField(rect, $"Element {index}", value[index], typeof(GameObject),true) as GameObject;
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type.IsSubclassOf(typeof(ScriptableObject)))
            {
                ScriptableObject value = (ScriptableObject)variable.ObjectValue;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.ObjectField(label, value, type, false) as ScriptableObject;
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateTypeAndUpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type == typeof(List<ScriptableObject>) && variable is BlackboardVariable<List<ScriptableObject>> scriptableObjectListVariable)
            {
                List<ScriptableObject> value = GetVariableDataCopy(scriptableObjectListVariable);
                EditorGUI.BeginChangeCheck();
                ReorderableList reorderableList = CreateVariableListElement(value, variable, varName);
                reorderableList.drawElementCallback = (rect, index, _, _) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    value[index] = EditorGUI.ObjectField(rect,$"Element {index}", value[index], typeof(GameObject),true) as ScriptableObject;
                    ShowContextMenuForVariable(variable.GUID, isOverride);
                };
                if (m_ListVariableFoldoutStates[variable.GUID])
                {
                    reorderableList.DoLayoutList();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID);
                }
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)) || type == typeof(UnityEngine.Object))
            {
                UnityEngine.Object value = (UnityEngine.Object)variable.ObjectValue;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.ObjectField(label, value, type, true);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID, type);
                }
            }
            else if (typeof(Enum).IsAssignableFrom(type))
            {
                var value = (Enum)variable.ObjectValue;
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.EnumPopup(label, value);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateValueIfChanged(value, variable.GUID, type);
                }
            }
            ShowContextMenuForVariable(variable.GUID, isOverride);
        }

        private ReorderableList CreateVariableListElement<T>(List<T> value, BlackboardVariable variable, string name)
        {
            m_ListVariableFoldoutStates.TryAdd(variable.GUID, false);
            m_ListVariableFoldoutStates[variable.GUID] = EditorGUILayout.Foldout(m_ListVariableFoldoutStates[variable.GUID], name);
            ReorderableList reorderableList = new ReorderableList(value, typeof(int))
            {
                draggable = false,
                displayAdd = true,
                displayRemove = true,
                elementHeight = EditorGUIUtility.singleLineHeight,
                headerHeight = 0
            };
            reorderableList.onAddCallback = _ =>
            {
                value.Add(default);
            };
            reorderableList.onRemoveCallback = _ =>
            {
                value.RemoveAt(value.Count-1);
            };

            return reorderableList;
        }

        // This method will update the value stored in the Blackboard if its changed (comparing using value), using the generic type of the value given.
        // This should be used for all value types whose data type matches their variable type.
        private void UpdateValueIfChanged<DataType>(DataType currentValue, SerializableGUID varID) 
        {
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                BlackboardVariable<DataType> targetVariable = (BlackboardVariable<DataType>)GetTargetVariable(targetAgent, varID);
                if (EqualityComparer<DataType>.Default.Equals(currentValue, targetVariable.Value))
                {
                    continue; // this one
                }
                SetBlackboardVariableValue(targetAgent, targetVariable, currentValue);
            }
        }
        
        // This method will update the value stored in the Blackboard if its changed (comparing using value), using the generic type of the value given.
        // This can only be used for list types.
        private void UpdateValueIfChanged<DataType>(List<DataType> currentValue, SerializableGUID varID) 
        {
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                BlackboardVariable<List<DataType>> targetVariable = (BlackboardVariable<List<DataType>>)GetTargetVariable(targetAgent, varID);
                // TODO: Check if LINQ usage here is generating allocs.
                if (currentValue != null && currentValue.SequenceEqual(targetVariable.Value))
                {
                    continue; 
                }
                SetBlackboardVariableValue(targetAgent, targetVariable, currentValue);
            }
        }

        // This method will update the value stored in the Blackboard if its changed (comparing using value), using an explicit given type.
        // This should be used for all value types whose data type doesn't match their variable type, as is the case for Enums which Unity EnumField casts to Enum.
        private void UpdateValueIfChanged<DataType>(DataType currentValue, SerializableGUID varID, Type type)
        {
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {            
                BlackboardVariable targetVariable = GetTargetVariable(targetAgent, varID);
                if (EqualityComparer<DataType>.Default.Equals(currentValue, (DataType)targetVariable.ObjectValue) ||
                             !type.IsInstanceOfType(currentValue))
                {
                    continue;
                }
                SetBlackboardVariableValue(targetAgent, targetVariable, currentValue);
            }
        }

        // This method will update the value stored in the Blackboard if its changed (comparing using reference check), using the variable's type.
        // This should be used for Objects.
        private void ValidateTypeAndUpdateValueIfChanged(object currentValue, SerializableGUID varID)
        {
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                BlackboardVariable targetVariable = GetTargetVariable(targetAgent, varID);
                if (ReferenceEquals(targetVariable.ObjectValue, currentValue) || !targetVariable.Type.IsInstanceOfType(currentValue))
                {
                    continue;
                }
                SetBlackboardVariableValue(targetAgent, targetVariable, currentValue);
            }
        }
        
        private void UpdateBehaviorGraphIfNeeded(BehaviorGraphAgent targetAgent)
        {
            if (Application.isPlaying)
            {
                return; // Don't update the graph if the application is playing, as the graph instance is a copy.
            }
            
            if (!HasRuntimeGraphAssetBeenDeleted(targetAgent))
            {
                return; // Don't make changes if the agent references a persistent asset.
            }

            if (targetAgent.Graph == null)
            {
                return;
            }

            // If the graph isn't enabled, the asset contains no data and the asset link cannot be updated.
            // Likewise, if the asset reference is null, the asset has been deleted and the link cannot be updated.
            BehaviorGraphAssetRegistry.TryGetAssetFromId(targetAgent.Graph.RootGraph.AuthoringAssetID, out BehaviorAuthoringGraph asset);
            if (ReferenceEquals(asset, null))
            {
                Debug.LogWarning($"Behavior graph reference lost on {targetAgent}.", targetAgent);
                targetAgent.Graph = null;
                EditorUtility.SetDirty(targetAgent);
                return;
            }
             
            // Destroy the temporary runtime graph instance.
            DestroyImmediate(targetAgent.Graph);
            
            // Try to update the reference through the authoring asset.
            // Note: asset.GetOrCreateGraph() would create a new runtime graph if one does not exist, 
            // which is not desirable here. If no runtime graph exists, null should be assigned.
            string assetPath = AssetDatabase.GetAssetPath(asset);
            targetAgent.Graph = AssetDatabase.LoadAssetAtPath<BehaviorGraph>(assetPath);
            EditorUtility.SetDirty(targetAgent);
        }
        
        private bool HaveSameGraph(BehaviorGraphAgent agent, BehaviorGraphAgent otherAgent)
        {
            // The two share the same assigned asset.
            if (ReferenceEquals(agent.Graph, otherAgent.Graph))
                return true;
                
            // The two have assigned instances that are copies of a shared asset.
            if (agent.Graph && otherAgent.Graph
                            && agent.Graph.RootGraph.AuthoringAssetID == otherAgent.Graph.RootGraph.AuthoringAssetID
                            && agent.Graph.RootGraph.VersionTimestamp == otherAgent.Graph.RootGraph.VersionTimestamp) 
                return true;

            return false;
        }
        
        private IEnumerator InitializeAndStartTargets()
        {
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                targetAgent.Init();
            }
            yield return 0; // Wait one frame before starting so users can set variable values.
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                targetAgent.Start();
            }
        }

        private BlackboardVariable GetTargetVariable(BehaviorGraphAgent agent, SerializableGUID variableID)
        {
            // If the application is playing, use the runtime graph's blackboard.
            agent.Graph.BlackboardReference.GetVariable(variableID, out BlackboardVariable runtimeVariableInstance);
            if (Application.isPlaying)
            {
                return runtimeVariableInstance;
            }

            // Otherwise, use the override variable if available.
            if (agent.m_BlackboardOverrides.TryGetValue(variableID, out BlackboardVariable overrideVariable))
            {
                return overrideVariable;
            }
            return runtimeVariableInstance;
        }

        private void SetBlackboardVariableValue<DataType>(BehaviorGraphAgent agent, BlackboardVariable refVariable, DataType newValue)
        {
            if (Application.isPlaying)
            {
                refVariable.ObjectValue = newValue;
                return;
            }

            if (agent.m_BlackboardOverrides.TryGetValue(refVariable.GUID, out BlackboardVariable overrideVariable))
            {
                overrideVariable.ObjectValue = newValue;
            } else
            {
                // Debug.LogError($"Add Overrride Create variable: {refVariable.Type}");
                overrideVariable = BlackboardVariable.CreateForType(refVariable.Type);
                overrideVariable.ObjectValue = newValue;
                overrideVariable.GUID = refVariable.GUID;
                overrideVariable.Name = refVariable.Name;
                agent.m_BlackboardOverrides.Add(refVariable.GUID, overrideVariable);
            }
            EditorUtility.SetDirty(agent);
        }

        public void ShowContextMenuForVariable(SerializableGUID guid, bool isOverride)
        {
            if (!isOverride) return;

            var lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.ContextClick)
            {
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Revert Variable"), false, () => ResetVariable(guid));
                    menu.ShowAsContext();

                }
            }
        }

        private void ResetVariable(SerializableGUID guid)
        {
            foreach (BehaviorGraphAgent targetAgent in m_TargetAgents)
            {
                if (targetAgent.m_BlackboardOverrides.ContainsKey(guid))
                {
                    if (guid == BehaviorGraph.k_GraphOwnerID)
                    {
                        targetAgent.m_BlackboardOverrides[guid].ObjectValue = targetAgent.gameObject;
                    }
                    else
                    {
                        targetAgent.m_BlackboardOverrides.Remove(guid);
                    }
                    EditorUtility.SetDirty(targetAgent);
                }
            }
        }
    }
}