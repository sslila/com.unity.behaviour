using System;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class GraphPrefsUtility
    {
        public static string PrefsPrefix { get; set; } = "Graph.Framework";

        private static string GetPrefsKey(string key) => $"{PrefsPrefix}.{key}";

        public static bool HasKey(string key, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                return UnityEditor.EditorPrefs.HasKey(GetPrefsKey(key));
            }
#endif
            return PlayerPrefs.HasKey(GetPrefsKey(key));
        }

        public static void DeleteKey(string key, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                UnityEditor.EditorPrefs.DeleteKey(GetPrefsKey(key));
                return;
            }
#endif
            PlayerPrefs.DeleteKey(GetPrefsKey(key));
        }

        public static string GetString(string key, string defaultValue, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                return UnityEditor.EditorPrefs.GetString(GetPrefsKey(key));
            }
#endif
            return PlayerPrefs.GetString(GetPrefsKey(key), defaultValue);
        }

        public static float GetFloat(string key, float defaultValue, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                return UnityEditor.EditorPrefs.GetFloat(GetPrefsKey(key));
            }
#endif
            return PlayerPrefs.GetFloat(GetPrefsKey(key), defaultValue);
        }

        public static int GetInt(string key, int defaultValue, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                return UnityEditor.EditorPrefs.GetInt(GetPrefsKey(key));
            }
#endif
            return PlayerPrefs.GetInt(GetPrefsKey(key), defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                return UnityEditor.EditorPrefs.GetBool(GetPrefsKey(key), defaultValue);
            }
#endif
            // There is no PlayerPrefs.SetBool so we use an int.
            return Convert.ToBoolean(PlayerPrefs.GetInt(GetPrefsKey(key), Convert.ToInt32(defaultValue)));
        }

        public static void SetString(string key, string value, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                UnityEditor.EditorPrefs.SetString(GetPrefsKey(key), value);
                return;
            }
#endif
            PlayerPrefs.SetString(GetPrefsKey(key), value);
        }

        public static void SetFloat(string key, float value, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                UnityEditor.EditorPrefs.SetFloat(GetPrefsKey(key), value);
                return;
            }
#endif
            PlayerPrefs.SetFloat(GetPrefsKey(key), value);
        }

        public static void SetInt(string key, int value, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                UnityEditor.EditorPrefs.SetInt(GetPrefsKey(key), value);
                return;
            }
#endif
            PlayerPrefs.SetInt(GetPrefsKey(key), value);
        }

        public static void SetBool(string key, bool value, bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                UnityEditor.EditorPrefs.SetBool(GetPrefsKey(key), value);
                return;
            }
#endif
            // There is no PlayerPrefs.SetBool so we use an int.
            PlayerPrefs.SetInt(GetPrefsKey(key), Convert.ToInt32(value));
        }

        public static void Save(bool inEditorContext)
        {
#if UNITY_EDITOR
            if (inEditorContext)
            {
                return;
            }
#endif
            PlayerPrefs.Save();
        }
    }
}