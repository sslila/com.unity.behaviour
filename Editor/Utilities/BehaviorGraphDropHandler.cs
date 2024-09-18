using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    static internal class BehaviorGraphDropHandler
    {
        [InitializeOnLoadMethod]
        public static void HookUpDragAndDropHandlers()
        {
            DragAndDrop.AddDropHandler(InspectorDropHandler);
            DragAndDrop.AddDropHandler(HierarchyDropHandler);
        }

        static bool IsValidDragItem()
        {
            return DragAndDrop.objectReferences.Length == 1 && typeof(BehaviorAuthoringGraph).IsAssignableFrom(DragAndDrop.objectReferences[0].GetType());
        }

        static void HandleDropOnObject(GameObject gameObject)
        {
            BehaviorAuthoringGraph behaviorAuthoringGraph = DragAndDrop.objectReferences[0] as BehaviorAuthoringGraph;
            var behaviorGraphAgent = gameObject.GetComponent<BehaviorGraphAgent>();
            if (behaviorGraphAgent == null)
            {
                behaviorGraphAgent = gameObject.AddComponent<BehaviorGraphAgent>();
                EditorUtility.SetDirty(gameObject);
            }
            var runtimeGraph = BehaviorAuthoringGraph.GetOrCreateGraph(behaviorAuthoringGraph);
            if (runtimeGraph.RootGraph == null)
            {
                behaviorAuthoringGraph.BuildRuntimeGraph();
            }
            behaviorGraphAgent.Graph = runtimeGraph;
            if (Application.isPlaying)
            {
                behaviorGraphAgent.StartCoroutine(InitializeAndStartGraph(behaviorGraphAgent));
            }
        }

        static DragAndDropVisualMode InspectorDropHandler(Object[] targets, bool perform)
        {
            if (!IsValidDragItem())
            {
                return DragAndDropVisualMode.None;
            }

            DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;
            foreach (var target in targets)
            {
                if (target is GameObject gameObject)
                {
                    if (!perform)
                    {
                        return DragAndDropVisualMode.Generic;
                    }

                    // Drag performed. Add the graph to the agents.
                    visualMode = DragAndDropVisualMode.Generic;
                    HandleDropOnObject(gameObject);
                }
            }
            return visualMode;
        }

        static IEnumerator InitializeAndStartGraph(BehaviorGraphAgent agent)
        {
            agent.Init();
            yield return 0; // Wait one frame before starting so users can set variable values.
            agent.Start();
        }

        static DragAndDropVisualMode HierarchyDropHandler(int dropTargetInstanceID, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            GameObject targetGameObject = EditorUtility.InstanceIDToObject(dropTargetInstanceID) as GameObject;
            if (!dropMode.HasFlag(HierarchyDropFlags.DropUpon) || !IsValidDragItem() || targetGameObject == null)
            {
                return DragAndDropVisualMode.None;
            }
            if (perform)
            {
                HandleDropOnObject(targetGameObject);
            }
            return DragAndDropVisualMode.Generic;
        }

    }
}