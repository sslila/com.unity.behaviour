using System;
using System.Linq;
using Unity.Behavior.GraphFramework;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// Blackboard asset type used by Unity Behavior. The asset contains a collection of Blackboard variables.
    /// </summary>
    [Serializable]
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "Blackboard", menuName = "Behavior/Blackboard")]
#endif
    internal class BehaviorBlackboardAuthoringAsset : BlackboardAsset
    {
        [SerializeField]
        private SerializableCommandBuffer m_CommandBuffer = new SerializableCommandBuffer();        
        public SerializableCommandBuffer CommandBuffer => m_CommandBuffer;
            
        [SerializeReference]
        private RuntimeBlackboardAsset m_RuntimeBlackboardAsset;
        
        private void OnEnable()
        {
            if (m_RuntimeBlackboardAsset == null)
            {
                BuildRuntimeBlackboard();
            }
        }

#if UNITY_EDITOR
        [OnOpenAsset(1)]
        public static bool OnOpenBlackboardAsset(int instanceID, int line)
        {
            BehaviorBlackboardAuthoringAsset asset = EditorUtility.InstanceIDToObject(instanceID) as BehaviorBlackboardAuthoringAsset;
            if (asset == null)
            {
                return false;
            }
            BlackboardWindowDelegate.Open(asset);
            return true; 
        }
#endif
        
        private static RuntimeBlackboardAsset GetOrCreateBlackboardAsset(BehaviorBlackboardAuthoringAsset assetObject)
        {
            RuntimeBlackboardAsset reference;
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(assetObject);
            if (!EditorUtility.IsPersistent(assetObject) || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            reference = AssetDatabase.LoadAllAssetsAtPath(assetPath).
                FirstOrDefault(asset => asset is RuntimeBlackboardAsset) as RuntimeBlackboardAsset;
            if (reference != null)
            {
                return reference;
            }
#endif

            reference = CreateInstance<RuntimeBlackboardAsset>();
            reference.name = assetObject.name;
            reference.AssetID = assetObject.AssetID;
            
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(reference, assetObject);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(assetObject);
#endif
            return reference;
        }

        public RuntimeBlackboardAsset BuildRuntimeBlackboard()
        {
            m_RuntimeBlackboardAsset = GetOrCreateBlackboardAsset(this);
            if (m_RuntimeBlackboardAsset == null)
            {
                return null;
            }

            m_RuntimeBlackboardAsset.name = name;
            m_RuntimeBlackboardAsset.AssetID = AssetID;

            if (m_RuntimeBlackboardAsset.VersionTimestamp == VersionTimestamp)
            { 
                return m_RuntimeBlackboardAsset;
            }
            
            m_RuntimeBlackboardAsset.Blackboard.m_Variables.Clear();
            m_RuntimeBlackboardAsset.m_SharedBlackboardVariableGuidHashset.Clear();
            foreach (VariableModel variable in Variables)
            {
                BlackboardVariable blackboardVariable = BlackboardVariable.CreateForType(variable.Type);
                blackboardVariable.Name = variable.Name;
                blackboardVariable.GUID = variable.ID;
                
                if (typeof(UnityEngine.Object).IsAssignableFrom(variable.Type))
                {
                    UnityEngine.Object unityObject = variable.ObjectValue as UnityEngine.Object;
                    if (unityObject != null)
                    {
                        blackboardVariable.ObjectValue = variable.ObjectValue;
                    }
                }
                else if (variable.ObjectValue != null)
                {
                    blackboardVariable.ObjectValue = variable.ObjectValue;
                }

                if (variable.IsShared)
                {
                    m_RuntimeBlackboardAsset.m_SharedBlackboardVariableGuidHashset.Add(variable.ID);
                }
                
                m_RuntimeBlackboardAsset.Blackboard.m_Variables.Add(blackboardVariable);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif                
            return m_RuntimeBlackboardAsset;
        }
    }
}