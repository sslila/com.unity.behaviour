#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior
{
    internal static class ConditionGeneratorUtility
    {
        private static readonly string k_PrefsKeySaveConditionPath = "SaveConditionPath";
        
        internal class ConditionData
        {
            internal string Name { get; set; }
            internal string Description { get; set; }
            internal string Story { get; set; }
            internal string Category { get; set; }
            internal Dictionary<string, Type> Variables { get; set; }
        }
        
        internal static bool CreateConditionAsset(ConditionData data)
        {
            string fileName = GeneratorUtils.ToPascalCase(data.Name) + "Condition";
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveConditionPath, Application.dataPath, true);
            suggestedSavePath = Util.IsPathRelativeToProjectAssets(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                $"Create Condition '{data.Name}'",
                suggestedSavePath,
                fileName,
                "cs");

            if (path.Length == 0)
            {
                return false;
            }
            GraphPrefsUtility.SetString(k_PrefsKeySaveConditionPath, Path.GetDirectoryName(path), true);


            string name = Path.GetFileNameWithoutExtension(path);
            string attributeString = GenerateAttributeString(data);

            using (var outfile = new StreamWriter(path))
            {
                var namespaceStrings = NodeGeneratorUtility.GetNamespaceStrings(data.Variables);
                foreach (var namespaceString in namespaceStrings)
                {
                    outfile.WriteLine($"using {namespaceString};");
                }
                outfile.WriteLine("");
                outfile.WriteLine("[Serializable, Unity.Properties.GeneratePropertyBag]");
                outfile.WriteLine(attributeString);
                outfile.WriteLine($"public partial class {name} : Condition");
                outfile.WriteLine("{");
                outfile.WriteLine(GeneratorUtils.GenerateVariableFields(data.Variables));
                outfile.WriteLine("    public override bool IsTrue()");
                outfile.WriteLine("    {");
                outfile.WriteLine("        return true;");
                outfile.WriteLine("    }");
                outfile.WriteLine("");
                outfile.WriteLine("    public override void OnStart()");
                outfile.WriteLine("    {");
                outfile.WriteLine("    }");
                outfile.WriteLine("");
                outfile.WriteLine("    public override void OnEnd()");
                outfile.WriteLine("    {");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
            }
            AssetDatabase.Refresh();
            string relativePath = path.StartsWith(Application.dataPath)
                ? "Assets" + path.Substring(Application.dataPath.Length)
                : path;
            MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePath, typeof(MonoScript));
            AssetDatabase.OpenAsset(script);
            return true;
        }

        private static string GenerateAttributeString(ConditionData data)
        {
            string name = string.IsNullOrEmpty(data.Name) ? "" : $"name: \"{data.Name}\",";
            string story = string.IsNullOrEmpty(data.Story) ? "" : $" story: \"{data.Story}\",";
            string description = string.IsNullOrEmpty(data.Description) ? "" : $" description: \"{data.Description}\",";
            string category = string.IsNullOrEmpty(data.Category) ? "" : $" category: \"{data.Category}\",";

            string attributeString = "[Condition(" + name + description + story + category +
                                     $" id: \"{SerializableGUID.Generate()}\")]";
            return attributeString;
        }
    }
}
#endif