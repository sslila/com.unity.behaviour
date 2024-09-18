using Unity.Behavior.GraphFramework;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior
{
    internal class BehaviorGraphDebugInfo : ScriptableObject, ISerializationCallbackReceiver
    {
        private HashSet<SerializableGUID> m_CodeBreakPoints = new HashSet<SerializableGUID>();
        
        [SerializeField]
        [HideInInspector]
        private List<SerializableGUID> m_CodeBreakPointsList = new List<SerializableGUID>();

        public void OnAfterDeserialize()
        {
            m_CodeBreakPoints = new HashSet<SerializableGUID>(m_CodeBreakPointsList);
        }

        public void OnBeforeSerialize()
        {
            m_CodeBreakPointsList = new List<SerializableGUID>(m_CodeBreakPoints);
        }

        internal bool IsNodeBreakpointEnabled(SerializableGUID nodeID)
        {
            return m_CodeBreakPoints.Contains(nodeID);
        }

#if UNITY_EDITOR
        internal void ToggleNodeBreakpoint(SerializableGUID nodeID)
        {
            if (m_CodeBreakPoints.Contains(nodeID))
            {
                m_CodeBreakPoints.Remove(nodeID);
            }
            else
            {
                m_CodeBreakPoints.Add(nodeID);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}