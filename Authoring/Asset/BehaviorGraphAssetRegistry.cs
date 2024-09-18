using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    /// <summary>
    /// A registry asset which contains references to <see cref="BehaviorAuthoringGraph"/> assets.
    /// </summary>
    [Serializable]
    internal class BehaviorGraphAssetRegistry : ScriptableObject, ISerializationCallbackReceiver
    {
        private const string k_GlobalRegistryPath = "Assets/GlobalAssetRegistry";

        /// <summary>
        /// All <see cref="BehaviorAuthoringGraph"/> assets referenced by the registry instance.
        /// </summary>
        [SerializeReference]
        public List<BehaviorAuthoringGraph> Assets = new ();

        private Dictionary<SerializableGUID, BehaviorAuthoringGraph> m_GuidToAsset = new Dictionary<SerializableGUID, BehaviorAuthoringGraph>();

        private static BehaviorGraphAssetRegistry m_GlobalRegistry;
        
        /// <summary>
        /// An instance of a <see cref="BehaviorGraphAssetRegistry" /> which holds references to all known
        /// assets contained within the project.
        /// </summary>
        public static BehaviorGraphAssetRegistry GlobalRegistry
        {
            get
            {
                if (!m_GlobalRegistry)
                {
                    m_GlobalRegistry = ResourceLoadAPI.Load<BehaviorGraphAssetRegistry>(k_GlobalRegistryPath);
                }
                if (!m_GlobalRegistry)
                {
                    m_GlobalRegistry = CreateInstance<BehaviorGraphAssetRegistry>();
                }

                m_GlobalRegistry.PurgeNullAndDuplicateAssets();
                return m_GlobalRegistry;
            }
        }

        /// <inheritdoc cref="OnEnable"/>
        public void OnEnable()
        {
            if (m_GuidToAsset == null)
            {
                m_GuidToAsset = new Dictionary<SerializableGUID, BehaviorAuthoringGraph>();
            }
            m_GlobalRegistry = this;
#if UNITY_EDITOR
            UpdateGlobalRegistry();
#endif
        }
        
        /// <inheritdoc cref="OnDisable"/>
        public void OnDisable()
        {
            m_GlobalRegistry = null;
        }

        public static void Add(BehaviorAuthoringGraph asset)
        {
            BehaviorGraphAssetRegistry globalRegistry = GlobalRegistry;
            if (!globalRegistry.Assets.Contains(asset))
            {
                globalRegistry.Assets.Add(asset);
                globalRegistry.m_GuidToAsset.Add(asset.AssetID, asset);
            }
        }

        public static bool Remove(BehaviorAuthoringGraph asset)
        {
            BehaviorGraphAssetRegistry globalRegistry = GlobalRegistry;
            if (globalRegistry.Assets.Contains(asset))
            {
                globalRegistry.Assets.Remove(asset);
                globalRegistry.m_GuidToAsset.Remove(asset.AssetID);
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        public static BehaviorAuthoringGraph TryGetAssetFromGraphPath(BehaviorGraph graph)
        {
            string assetPath = AssetDatabase.GetAssetPath(graph);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(assetPath);
        }
#endif

        public static bool TryGetAssetFromId(SerializableGUID id, out BehaviorAuthoringGraph asset)
        {
            return GlobalRegistry.m_GuidToAsset.TryGetValue(id, out asset);
        }

        private void PurgeNullAndDuplicateAssets()
        {
            Assets.RemoveAll(asset => asset == null);
            Assets = new HashSet<BehaviorAuthoringGraph>(Assets).ToList();
            m_GuidToAsset.Clear();
            foreach (BehaviorAuthoringGraph asset in Assets)
            {
                m_GuidToAsset.Add(asset.AssetID, asset);
            }
        }

#if UNITY_EDITOR
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            UpdateGlobalRegistry();
        }

        public static void UpdateGlobalRegistry()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BehaviorAuthoringGraph)}");
            
            // No guids are returned during deserialization, so don't update the registry.
            if (guids.Length == 0)
            {
                return;
            }
            
            BehaviorGraphAssetRegistry globalRegistry = GlobalRegistry;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BehaviorAuthoringGraph asset = AssetDatabase.LoadAssetAtPath<BehaviorAuthoringGraph>(path);
                if (asset != null && !globalRegistry.Assets.Contains(asset))
                {
                    globalRegistry.Assets.Add(asset);
                    globalRegistry.m_GuidToAsset.Add(asset.AssetID, asset);
                }
            }
            EditorUtility.SetDirty(globalRegistry);
        }

        private class AssetRegistryBuildPopulator : BuildPlayerProcessor
        {
            public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
            {
                UpdateGlobalRegistry();
            }
        }
#endif

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            m_GuidToAsset = new Dictionary<SerializableGUID, BehaviorAuthoringGraph>();
            PurgeNullAndDuplicateAssets();
        }
    }
}