#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal static class EventChannelGeneratorUtility
    {
        private static readonly string k_PrefsKeySaveEventPath = "SaveEventPath";
        internal class EventChannelData
        {
            internal string Name { get; set; }
            internal string ClassName { get; set; }
            internal string Description { get; set; }
            internal string Message { get; set; }
            internal string Category { get; set; }
            internal (string, Type)[] Parameters { get; set; }
        }

        internal static bool CreateEventChannelAsset(EventChannelData data)
        {
            string fileName = GeneratorUtils.ToPascalCase(data.Name);
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveEventPath, Application.dataPath, true);
            suggestedSavePath = Util.IsPathRelativeToProjectAssets(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                "Create Event Channel ScriptableObject",
                suggestedSavePath,
                fileName,
                "cs");

            if (path.Length == 0)
            {
                return false;
            }
            GraphPrefsUtility.SetString(k_PrefsKeySaveEventPath, Path.GetDirectoryName(path), true);

            GenerateEventChannelFile(data, path);

            return true;
        }

        internal static void GenerateEventChannelFile(EventChannelData data, string path)
        {
            string typedParams = string.Join(", ",
                data.Parameters.Select(p => $"{GeneratorUtils.GetStringForType(p.Item2)} {p.Item1}"));
            string untypedParams = string.Join(", ", data.Parameters.Select(p => $"{p.Item1}"));

            string eventChannelName = Path.GetFileNameWithoutExtension(path);
            string attributeString = GenerateAttributeString(data);
            data.ClassName = eventChannelName;

            using (StreamWriter outfile = new StreamWriter(path))
            {
                // Imports
                outfile.WriteLine("using System;");
                outfile.WriteLine("using Unity.Behavior.GraphFramework;");
                outfile.WriteLine("using Unity.Behavior;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using Unity.Properties;");

                // Event channel
                outfile.WriteLine();
                outfile.WriteLine("#if UNITY_EDITOR");
                outfile.WriteLine($"[CreateAssetMenu(menuName = \"Behavior/Event Channels/{data.Name}\")]");
                outfile.WriteLine("#endif");
                outfile.WriteLine("[Serializable, GeneratePropertyBag]");
                outfile.WriteLine(attributeString);
                outfile.WriteLine($"public partial class {eventChannelName} : EventChannelBase");
                outfile.WriteLine("{");
                outfile.WriteLine($"    public delegate void {eventChannelName}EventHandler({typedParams});");
                outfile.WriteLine($"    public event {eventChannelName}EventHandler Event; ");
                outfile.WriteLine();
                outfile.WriteLine($"    public void SendEventMessage({typedParams})");
                outfile.WriteLine("    {");
                outfile.WriteLine($"        Event?.Invoke({untypedParams});");
                outfile.WriteLine("    }");
                outfile.WriteLine();

                outfile.WriteLine("    public override void SendEventMessage(BlackboardVariable[] messageData)");
                outfile.WriteLine("    {");
                for (int i = 0; i < data.Parameters.Length; i++)
                {
                    string varName = data.Parameters[i].Item1;
                    string type = GeneratorUtils.GetStringForType(data.Parameters[i].Item2);
                    outfile.WriteLine(
                        $"        BlackboardVariable<{type}> {varName}BlackboardVariable = messageData[{i}] as BlackboardVariable<{type}>;");
                    outfile.WriteLine(
                        $"        var {varName} = {varName}BlackboardVariable != null ? {varName}BlackboardVariable.Value : default({type});");
                    outfile.WriteLine();
                }

                outfile.WriteLine($"        Event?.Invoke({untypedParams});");
                outfile.WriteLine("    }");
                outfile.WriteLine();

                outfile.WriteLine(
                    "    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)");
                outfile.WriteLine("    {");
                outfile.WriteLine($"        {eventChannelName}EventHandler del = ({untypedParams}) =>");
                outfile.WriteLine("        {");
                for (int i = 0; i < data.Parameters.Length; i++)
                {
                    string varName = data.Parameters[i].Item1;
                    string type = GeneratorUtils.GetStringForType(data.Parameters[i].Item2);
                    outfile.WriteLine(
                        $"            BlackboardVariable<{type}> var{i} = vars[{i}] as BlackboardVariable<{type}>;");
                    outfile.WriteLine($"            if(var{i} != null)");
                    outfile.WriteLine($"                var{i}.Value = {varName};");
                    outfile.WriteLine();
                }

                outfile.WriteLine("            callback();");
                outfile.WriteLine("        };");
                outfile.WriteLine("        return del;");
                outfile.WriteLine("    }");
                outfile.WriteLine();

                outfile.WriteLine("    public override void RegisterListener(Delegate del)");
                outfile.WriteLine("    {");
                outfile.WriteLine($"        Event += del as {eventChannelName}EventHandler;");
                outfile.WriteLine("    }");
                outfile.WriteLine();

                outfile.WriteLine("    public override void UnregisterListener(Delegate del)");
                outfile.WriteLine("    {");
                outfile.WriteLine($"        Event -= del as {eventChannelName}EventHandler;");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
                outfile.WriteLine();
            }

            AssetDatabase.Refresh();
        }

        internal static string GenerateAttributeString(EventChannelData data)
        {
            string name = string.IsNullOrEmpty(data.Name) ? "" : $"name: \"{data.Name}\",";
            string message = string.IsNullOrEmpty(data.Message) ? "" : $" message: \"{data.Message}\",";
            string description = string.IsNullOrEmpty(data.Description) ? "" : $" description: \"{data.Description}\",";
            string category = string.IsNullOrEmpty(data.Category) ? "" : $" category: \"{data.Category}\",";

            string attributeString = "[EventChannelDescription(" + name + description + message + category +
                                     $" id: \"{SerializableGUID.Generate()}\")]";
            return attributeString;
        }
    }
}
#endif