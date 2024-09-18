using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    internal class GraphAssetSubgraphUpdater : AssetModificationProcessor
    {
        private static List<BehaviorAuthoringGraph> s_ChangedGraphAssets = new();
        private static List<(BehaviorAuthoringGraph, string)> s_AssetsToRebuild = new();
        
        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (paths.Length == 0)
                return paths;
            
            // Retrieve all authoring graphs being saved.
            s_ChangedGraphAssets.Clear();
            s_ChangedGraphAssets.AddRange(paths
                .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(BehaviorAuthoringGraph))
                .Select(path => AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(path))
            );
            
            // Get all assets that contain references to the assets being saved.
            s_AssetsToRebuild.Clear();
            foreach (BehaviorAuthoringGraph asset in BehaviorGraphAssetRegistry.GlobalRegistry.Assets)
            {
                foreach (BehaviorAuthoringGraph changedGraph in s_ChangedGraphAssets)
                {
                    if (!ReferenceEquals(asset, changedGraph) && asset.ContainsReferenceTo(changedGraph))
                    {
                        s_AssetsToRebuild.Add((asset, changedGraph.name));
                        break;
                    }
                }
            }
            
            // Rebuild all assets that reference the changed assets.
            foreach ((BehaviorAuthoringGraph referencingGraph, string subgraphName) in s_AssetsToRebuild)
            {
                Debug.Log($"Behavior: Graph \"{subgraphName}\" updated. Rebuilding referencing graph \"{referencingGraph.name}\".");
                referencingGraph.BuildRuntimeGraph();
            }
            
            return paths;
        }
    }
}