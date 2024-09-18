#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    [CustomEditor(typeof(BehaviorGraphAssetRegistry))]
    internal class BehaviorGraphAssetRegistryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviorGraphAssetRegistry registry = (BehaviorGraphAssetRegistry)target;

            EditorGUILayout.LabelField("Assets");
            EditorGUI.BeginDisabledGroup(true);
            foreach (BehaviorAuthoringGraph asset in registry.Assets)
            {
                if (asset)
                {
                    EditorGUILayout.ObjectField(asset.name, asset, typeof(BehaviorGraphAssetRegistry), false);
                }

            }
            EditorGUI.EndDisabledGroup();

            if (registry == BehaviorGraphAssetRegistry.GlobalRegistry)
            {
                if (GUILayout.Button("Refresh Global Registry"))
                {
                    BehaviorGraphAssetRegistry.UpdateGlobalRegistry();
                }
            }
        }
    }
}
#endif