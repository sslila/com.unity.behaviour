using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// RuntimeBlackboardAsset is the runtime version of BehaviorBlackboardAsset.
    /// </summary>
    [Serializable]
    public class RuntimeBlackboardAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField][HideInInspector]
        internal long m_VersionTimestamp;
        internal long VersionTimestamp => m_VersionTimestamp;
        
        [HideInInspector]
        [SerializeField] 
        internal SerializableGUID AssetID;

        [SerializeField]
        private Blackboard m_Blackboard;
        
        /// <summary>
        /// The Blackboard for the RuntimeBlackboardAsset.
        /// </summary>
        public Blackboard Blackboard => m_Blackboard;
        
        [SerializeField]
        private List<SerializableGUID> m_SharedBlackboardVariableGuids = new List<SerializableGUID>();
        
        internal HashSet<SerializableGUID> m_SharedBlackboardVariableGuidHashset = new HashSet<SerializableGUID>();
        
#if UNITY_EDITOR
        private List<BlackboardVariable> m_ValueOnEnterPlaymode = new List<BlackboardVariable>();

        private void Awake()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        }

        private void OnPlaymodeStateChanged(UnityEditor.PlayModeStateChange change)
        {
            if (change == UnityEditor.PlayModeStateChange.EnteredPlayMode)
            {
                Blackboard blackboard = new Blackboard();
                blackboard = blackboard.CopyBlackboard(m_Blackboard, this);
                m_ValueOnEnterPlaymode = blackboard.m_Variables;
            }
            else if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                m_Blackboard.m_Variables = m_ValueOnEnterPlaymode;
            }
        }
#endif
                
        internal bool IsSharedVariable(SerializableGUID guid)
        {
            return m_SharedBlackboardVariableGuidHashset.Contains(guid);
        }
        
        /// <inheritdoc cref="OnBeforeSerialize"/>
        public void OnBeforeSerialize()
        {
            m_SharedBlackboardVariableGuids.Clear();
            foreach (var serializableGuid in m_SharedBlackboardVariableGuidHashset)
            {
                m_SharedBlackboardVariableGuids.Add(serializableGuid);
            }
        }
        
        /// <inheritdoc cref="OnAfterDeserialize"/>
        public void OnAfterDeserialize()
        {
            if (m_SharedBlackboardVariableGuids == null)
            {
               m_SharedBlackboardVariableGuids = new List<SerializableGUID>();
            }
            
            m_SharedBlackboardVariableGuidHashset.Clear();
            for (int i = 0; i < m_SharedBlackboardVariableGuids.Count; i++)
            {
                m_SharedBlackboardVariableGuidHashset.Add(m_SharedBlackboardVariableGuids[i]);
            }
        }
    }
}