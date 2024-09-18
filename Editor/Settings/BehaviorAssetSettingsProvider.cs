#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEditor;

namespace Unity.Behavior
{
    sealed class BehaviorAssetSettingsProvider : SettingsProvider
    {
        private const string k_PrefsKeyGraphOwnerName = "DefaultGraphOwnerName"; 
        
        public BehaviorAssetSettingsProvider() : base("Project/Muse Behavior/Asset Settings", SettingsScope.Project) { }

        public override void OnGUI(string search)
        {
            string defaultGraphOwnerName = GraphPrefsUtility.GetString(k_PrefsKeyGraphOwnerName, BehaviorGraphEditor.k_DefaultGraphOwnerName, true);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Graph Owner Name");
            defaultGraphOwnerName = EditorGUILayout.TextField(string.Empty, defaultGraphOwnerName);
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                GraphPrefsUtility.SetString(k_PrefsKeyGraphOwnerName, defaultGraphOwnerName, true);
            }
        }

        private static BehaviorAssetSettingsProvider m_Instance;
        internal static BehaviorAssetSettingsProvider Instance
        {
            get
            {
                m_Instance ??= new BehaviorAssetSettingsProvider();
                return m_Instance;
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new BehaviorAssetSettingsProvider();
    }
}
#endif