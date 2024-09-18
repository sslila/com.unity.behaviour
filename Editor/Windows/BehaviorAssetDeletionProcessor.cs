using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    internal class BehaviorAssetDeletionProcessor : AssetModificationProcessor
    {
        // If an authoring graph or blackboard asset is deleted within Unity, this will close any editor window associated with the asset.
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(BehaviorAuthoringGraph) && AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(BehaviorBlackboardAuthoringAsset))
            {
                return AssetDeleteResult.DidNotDelete;
            }
            
            // Close any matching Behavior Graph Windows.
            BehaviorAuthoringGraph graph = AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(path);
            foreach (BehaviorWindow window in Resources.FindObjectsOfTypeAll<BehaviorWindow>())
            {
                if (window.Asset == graph)
                {
                    window.Close();
                }
            }
            // Close any matching Blackboard Windows.
            BehaviorBlackboardAuthoringAsset blackboardAuthoring = AssetDatabase.LoadAssetAtPath<BehaviorBlackboardAuthoringAsset>(path);
            foreach (BlackboardWindow window in Resources.FindObjectsOfTypeAll<BlackboardWindow>())
            {
                if (window.Asset == blackboardAuthoring)
                {
                    window.Close();
                }
            }
            
            // Update any Behavior Graph Windows which have a reference to the deleted Blackboard asset.
            blackboardAuthoring?.InvokeBlackboardDeleted();
            
            return AssetDeleteResult.DidNotDelete;
        }
    }
}