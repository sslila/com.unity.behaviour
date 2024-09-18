#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    sealed class BehaviorSettingsProvider : SettingsProvider
    {
        private List<SettingsProvider> m_SettingsProviders = new List<SettingsProvider>();
        
        public BehaviorSettingsProvider() : base("Project/Muse Behavior", SettingsScope.Project)
        {
            m_SettingsProviders.Add(BehaviorAssetSettingsProvider.CreateCustomSettingsProvider());
        }

        public override void OnGUI(string search)
        {
            foreach (SettingsProvider provider in m_SettingsProviders)
            {
                EditorGUILayout.LabelField(provider.label, EditorStyles.boldLabel);
                provider.OnGUI(search);
                GUILayout.Space(10);
            }
        }
        
        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new BehaviorSettingsProvider();
    }
}
#endif