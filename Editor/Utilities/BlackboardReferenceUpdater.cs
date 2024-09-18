using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    internal class BlackboardReferenceUpdater : AssetModificationProcessor
    {
        private static List<BehaviorBlackboardAuthoringAsset> s_ChangedBlackboardAssets = new();
        private static List<(BehaviorAuthoringGraph, string)> s_AssetsToRebuild = new();
        
        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (paths.Length == 0)
                return paths;
            
            // Retrieve all authoring graphs being saved.
            s_ChangedBlackboardAssets.Clear();
            s_ChangedBlackboardAssets.AddRange(paths
                .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(BehaviorBlackboardAuthoringAsset))
                .Select(path => AssetDatabase.LoadAssetAtPath<BehaviorBlackboardAuthoringAsset>(path))
            );
            
            // Get all assets that contain references to the assets being saved.
            s_AssetsToRebuild.Clear();
            foreach (BehaviorAuthoringGraph authoringGraph in BehaviorGraphAssetRegistry.GlobalRegistry.Assets)
            {
                foreach (BehaviorBlackboardAuthoringAsset blackboardAsset in s_ChangedBlackboardAssets)
                {
                    if (authoringGraph.ContainsReferenceTo(blackboardAsset))
                    {
                        s_AssetsToRebuild.Add((authoringGraph, blackboardAsset.name));
                        break;
                    }
                }
            }
            
            // Rebuild all assets that reference the changed assets.
            foreach ((BehaviorAuthoringGraph referencingGraph, string blackboardName) in s_AssetsToRebuild)
            {
                Debug.Log($"Blackboard: \"{blackboardName}\" updated. Rebuilding referencing graph \"{referencingGraph.name}\".");
                referencingGraph.BuildRuntimeGraph();
            }
            
            return paths;
        }
    }
}