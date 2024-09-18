using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class BlackboardAsset : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        public SerializableGUID AssetID = SerializableGUID.Generate();
        
        [SerializeReference, HideInInspector]
        private List<VariableModel> m_Variables = new();
        public List<VariableModel> Variables
        {
            get => m_Variables;
            internal set
            {
                m_Variables = value;
                OnBlackboardChanged.Invoke();
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(GetInstanceID()));
            AssetID = new SerializableGUID(guid);
#endif
        }

        [SerializeField][HideInInspector]
        internal long m_VersionTimestamp;
        public long VersionTimestamp => m_VersionTimestamp;

        /// <summary>
        /// Delegate for blackboard changes.
        /// </summary>
        public delegate void BlackboardChangedCallback();
        
        /// <summary>
        /// Callback used for changes in the blackboard asset.
        /// </summary>
        public event BlackboardChangedCallback OnBlackboardChanged = delegate { };
        
        /// <summary>
        /// Invokes the OnBlackboardChanged callback.
        /// </summary>
        public void InvokeBlackboardChanged() => OnBlackboardChanged.Invoke();
        
        /// <summary>
        /// Delegate for deleted blackboard assets.
        /// </summary>
        public delegate void BlackboardDeletedCallback();
        
        /// <summary>
        /// Callback used for notifying when the asset is deleted.
        /// </summary>
        public event BlackboardDeletedCallback OnBlackboardDeleted = delegate { };
        
        /// <summary>
        /// Invokes the OnBlackboardDeleted callback.
        /// </summary>
        public void InvokeBlackboardDeleted() => OnBlackboardDeleted.Invoke();

        internal virtual void OnValidate()
        {
            Variables.RemoveAll(variable => variable == null);
            foreach (VariableModel variable in Variables)
            {
                variable.OnValidate();
            }
        }

        public void MarkUndo(string description)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, description);
#endif
            // There are still a few lingering non-command changes to asset data preceded by MarkUndo() calls.
            // In order to pick up these changes, set the asset dirty here too.
            SetAssetDirty();
        }
        
        public void SetAssetDirty()
        {
            m_VersionTimestamp = DateTime.Now.Ticks;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void SaveAsset()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            // Note: Using AssetDatabase.SaveAssetIfDirty(this) saves the asset but doesn't pass the path to
            // AssetModificationProcessor.OnWillSaveAssets(), which we use to rebuild graphs which reference this one.
            // Instead, use AssetDatabase.SaveAssets().
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}