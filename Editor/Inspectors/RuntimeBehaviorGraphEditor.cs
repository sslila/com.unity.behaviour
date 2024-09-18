using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// A custom editor for inspecting <see cref="BehaviorGraph"/> graph assets. When an asset is selected,
    /// the inspector will show a simplified text representation of the behavior graph.
    /// </summary>
    [CustomEditor(typeof(BehaviorGraph))]
    internal class RuntimeBehaviorGraphEditor : Editor
    {
        private BehaviorGraph Graph => target as BehaviorGraph;
        private readonly Dictionary<Node, int> m_DisplayID = new ();
        private int m_LastID;

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            if (!Graph || Graph.RootGraph == null)
            {
                EditorGUILayout.LabelField("No graphs to display.");
                return;
            }

            m_DisplayID.Clear();
            for (int i = 0; i < Graph.Graphs.Count; i++)
            {
                BehaviorGraphModule graph = Graph.Graphs[i];
                BehaviorGraphAssetRegistry.TryGetAssetFromId(graph.AuthoringAssetID, out BehaviorAuthoringGraph authoringGraph);
                m_LastID = 0;
                if (i == 0)
                {
                    EditorGUILayout.LabelField($"Root Graph: {authoringGraph?.name} [{i}]");
                }
                else
                {
                    EditorGUILayout.LabelField($"Subgraph: {authoringGraph?.name} [{i}]");
                }
                DisplayNode(graph.Root);
                EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            }
        }

        private void DisplayNode(Node node)
        {
            if (node == null)
            {
                return;
            }

            // If the node has already been traversed, display its existing ID and return.
            bool traversed = m_DisplayID.TryGetValue(node, out int id);
            if (traversed)
            {
                EditorGUILayout.LabelField($"{node.GetType().Name} (#{id}) ...");
                return;
            }

            // Update last ID and assign it to the node.
            id = m_LastID++;
            m_DisplayID[node] = id;

            // If the node is RunSubgraph, display subgraph info.
            if (node is RunSubgraph runSubgraph)
            {
                string subgraphLabel = "null";
                if (runSubgraph.Subgraph != null)
                {
                    BehaviorGraphAssetRegistry.TryGetAssetFromId(runSubgraph.Subgraph.AuthoringAssetID, out var authoringGraph);
                    subgraphLabel = $"{authoringGraph?.name} [{Graph.Graphs.IndexOf(runSubgraph.Subgraph)}]";
                }
                
                EditorGUILayout.LabelField($"{node.GetType().Name} (#{id}) : {subgraphLabel}");
            }
            else
            {
                EditorGUILayout.LabelField($"{node.GetType().Name} (#{id})");
            }

            EditorGUI.indentLevel++;
            switch (node)
            {
                case Action:
                    break;
                case Modifier modifier:
                    DisplayNode(modifier.Child);
                    break;
                case Composite composite:
                    foreach (Node child in composite.Children)
                    {
                        DisplayNode(child);
                    }
                    break;
                case Join join:
                    DisplayNode(join.Child);
                    break;
            }
            EditorGUI.indentLevel--;
        }
    }   
}
