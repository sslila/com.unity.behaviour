using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class ResourceLoadAPI
    {
        public delegate Object LoadDelegate(string path);
        public static event LoadDelegate LoadCallback;

        public delegate Object[] LoadAllDelegate(string path);
        public static event LoadAllDelegate LoadAllCallback;

        public static T Load<T>(string path) where T : Object
        {
            if (LoadCallback != null)
            {
                return LoadCallback(path) as T;
            }
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }

        public static List<T> LoadAll<T>(string path) where T : Object
        {
            Object[] objs = null;
            if (LoadAllCallback != null)
            {
                objs = LoadAllCallback(path);
            }
#if UNITY_EDITOR
            else
            {
                List<T> loadedAssets = new List<T>();
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta"))
                    {
                        continue;
                    }
                    T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(file);
                    if (asset != null)
                    {
                        loadedAssets.Add(asset);
                    }
                }
                return loadedAssets;
            }
#endif
            if (objs == null)
            {
                return null;
            }

            List<T> list = new List<T>();
            foreach (var obj in objs)
            {
                if (obj is T)
                {
                    list.Add(obj as T);
                }
            }
            return list;
        }
    }
}