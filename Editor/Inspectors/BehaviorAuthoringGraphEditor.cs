using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    /// <summary>
    /// A custom editor for inspecting <see cref="BehaviorAuthoringGraph"/> assets. When an asset is selected,
    /// the inspector displays the runtime sub-assets, if any exist, as well as other referenced authoring assets.
    /// </summary>
    [CustomEditor(typeof(BehaviorAuthoringGraph))]
    [CanEditMultipleObjects]
    internal class BehaviorAuthoringGraphEditor : Editor
    {
        private Dictionary<SerializableGUID, string> m_GuidToName = new ();
        
        /// <inheritdoc />
        public override VisualElement CreateInspectorGUI()
        {
            m_GuidToName.Clear();
            foreach (Object targetAsset in serializedObject.targetObjects)
            {
                BehaviorAuthoringGraph authoringGraph = targetAsset as BehaviorAuthoringGraph;
                if (authoringGraph)
                {
                    m_GuidToName[authoringGraph.AssetID] = authoringGraph.name;
                }
            }
            
            var element = new IMGUIContainer(OnInspectorGUI);
            element.TrackSerializedObjectValue(serializedObject, CheckForRename);   
            return element;
        }

        private void CheckForRename(SerializedObject changedObject)
        {
            BehaviorAuthoringGraph authoringGraph = changedObject.targetObject as BehaviorAuthoringGraph;
            SerializableGUID assetID = authoringGraph!.AssetID;
            string newName = authoringGraph.name;
            
            // If the new name is the same as the old name, do nothing.
            if (m_GuidToName.TryGetValue(assetID, out string oldName) && string.Equals(oldName, newName))
            {
                return;
            }
            
            // Asset has new name, so rename runtime graph.
            m_GuidToName[authoringGraph.AssetID] = newName;
            string assetPath = AssetDatabase.GetAssetPath(authoringGraph);
            BehaviorGraph runtimeGraph = AssetDatabase.LoadAssetAtPath<BehaviorGraph>(assetPath);
            if (runtimeGraph)
            {
                runtimeGraph.name = newName;
            }
            BehaviorGraphDebugInfo debugInfo = AssetDatabase.LoadAssetAtPath<BehaviorGraphDebugInfo>(assetPath);
            if (debugInfo)
            {
                debugInfo.name = $"{newName} Debug Info";
            }
            EditorUtility.SetDirty(authoringGraph);
            AssetDatabase.SaveAssetIfDirty(authoringGraph);
        }
        
        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            foreach (Object targetAsset in targets)
            {
                BehaviorAuthoringGraph authoringGraph = targetAsset as BehaviorAuthoringGraph;
                if (!authoringGraph)
                {
                    continue;
                }
                
                EditorGUILayout.LabelField(authoringGraph.name, EditorStyles.boldLabel);
                DrawAssetData(authoringGraph);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                EditorGUILayout.Space();
            }
        }

        private void DrawAssetData(BehaviorAuthoringGraph targetAsset)
        {
            // Display linked authoring graph assets.
            EditorGUILayout.LabelField("Linked Authoring Graph Assets");
            EditorGUI.BeginDisabledGroup(true);
            List<BehaviorAuthoringGraph> referencedAssets = targetAsset.Nodes.OfType<SubgraphNodeModel>()
                .Where(node => node.SubgraphAuthoringAsset)
                .Select(node => node.SubgraphAuthoringAsset)
                .Distinct()
                .ToList();
            if (referencedAssets.Count == 0)
            {
                EditorGUILayout.LabelField("(None)");
            }
            foreach (BehaviorAuthoringGraph asset in referencedAssets)
            {
                EditorGUILayout.ObjectField(asset.name, asset, typeof(BehaviorAuthoringGraph), false);
            }
            EditorGUI.EndDisabledGroup();

            // Display runtime assets
            EditorGUILayout.LabelField("Runtime Assets");
            EditorGUI.BeginDisabledGroup(true);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(targetAsset));
            var runtimeGraph = assets.FirstOrDefault(asset => asset is BehaviorGraph) as BehaviorGraph;
            var debugInfo = assets.FirstOrDefault(asset => asset is BehaviorGraphDebugInfo) as BehaviorGraphDebugInfo;
            EditorGUILayout.ObjectField("Runtime Graph", runtimeGraph, typeof(BehaviorGraph), false);
            EditorGUILayout.ObjectField("Graph Debug Info", debugInfo, typeof(BehaviorGraphDebugInfo), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            // Add a button to delete runtime assets.
            if (GUILayout.Button("Delete Runtime Assets"))
            {
                foreach (Object asset in assets)
                {
                    if (asset is BehaviorGraph or BehaviorGraphDebugInfo)
                    {
                        AssetDatabase.RemoveObjectFromAsset(asset);
                    }
                }
                EditorUtility.SetDirty(targetAsset);
                AssetDatabase.SaveAssetIfDirty(targetAsset);
            }

            if (targetAsset.m_Blackboards.Count > 0)
            {
                EditorGUILayout.LabelField("Blackboard References");
                foreach (var asset in targetAsset.m_Blackboards)
                {
                    EditorGUILayout.ObjectField(asset.name, asset, typeof(BehaviorBlackboardAuthoringAsset), false);
                }
            }
        }
    }
}