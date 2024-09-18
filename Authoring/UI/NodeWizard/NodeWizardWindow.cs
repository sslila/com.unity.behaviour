#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior
{
    internal class NodeWizardWindow : ISerializationCallbackReceiver
    {
        protected bool IsInEditMode { get; set; }
     
        protected NodeInfo NodeInfo;
        protected Dictionary<string, Type> VariableSuggestions;
        private List<string> m_VariableNames;
        private List<string> m_VariableTypeNames;

        public void OnBeforeSerialize()
        {
            if (VariableSuggestions == null)
            {
                return;
            }
            m_VariableNames = new List<string>();
            m_VariableTypeNames = new List<string>();
            foreach (KeyValuePair<string, Type> suggestion in VariableSuggestions)
            {
                m_VariableNames.Add(suggestion.Key);
                m_VariableTypeNames.Add(suggestion.Value.AssemblyQualifiedName);
            }
        }

        public void OnAfterDeserialize()
        {
            if (m_VariableNames.Count == 0)
            {
                return;
            }
            VariableSuggestions = new Dictionary<string, Type>();
            for (int i = 0; i < m_VariableNames.Count; ++i)
            {
                Type type = Type.GetType(m_VariableTypeNames[i]);
                if (type != null)
                {
                    VariableSuggestions.Add(m_VariableNames[i], type);   
                }
            }
        }
    }
}

#endif