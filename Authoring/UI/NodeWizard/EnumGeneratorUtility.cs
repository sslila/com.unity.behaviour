#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    internal static class EnumGeneratorUtility
    {
        private static readonly string k_PrefsKeySaveEnumPath = "SaveEnumPath";
        internal static bool CreateEnumAsset(string name, List<string> members)
        {
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveEnumPath, Application.dataPath, true);
            suggestedSavePath = Util.IsPathRelativeToProjectAssets(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            var path = EditorUtility.SaveFilePanel($"Create Enum '{name}'", suggestedSavePath, name, "cs");

            if (path.Length == 0)
            {
                return false;
            }
            GraphPrefsUtility.SetString(k_PrefsKeySaveEnumPath, Path.GetDirectoryName(path), true);

            using (var outfile = new StreamWriter(path))
            {
                outfile.WriteLine("using System;");
                outfile.WriteLine("using Unity.Behavior;");
                outfile.WriteLine("");
                outfile.WriteLine("[BlackboardEnum]");
                outfile.WriteLine($"public enum {name}");
                outfile.WriteLine("{");
                outfile.WriteLine($"    {string.Join(",\n\t", members.Select(m => m.Replace(" ", string.Empty)).Where(m => !string.IsNullOrEmpty(m)))}");
                outfile.WriteLine("}");
            }
            AssetDatabase.Refresh();
            return true;
        }
    }
}
#endif