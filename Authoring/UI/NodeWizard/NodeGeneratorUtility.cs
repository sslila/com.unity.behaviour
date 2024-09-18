#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Behavior.GraphFramework;
using System.Linq;

namespace Unity.Behavior
{
    internal class NodeGeneratorUtility
    {
        private static readonly string k_PrefsKeySaveNodePath = "SaveNodePath";
        internal enum NodeType
        {
            Action,
            Modifier,
            Composite
        }

        internal class NodeData
        {
            internal NodeType NodeType { get; set; }
            internal string Name { get; set; }
            internal string ClassName { get; set; }
            internal string Description { get; set; }
            internal string Story { get; set; }
            internal string Icon { get; set; }
            internal string Category { get; set; }
            internal Dictionary<string, Type> Variables { get; set; }
            internal List<string> Ports { get; set; }
            internal List<string> OldPorts { get; set; }
        }

        internal static bool Create(NodeData data, string postfix)
        {
            string fileName = GeneratorUtils.ToPascalCase(data.Name + postfix);
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveNodePath, Application.dataPath, true);
            suggestedSavePath = Util.IsPathRelativeToProjectAssets(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                $"Create {GetNodeTypeString(data.NodeType)} Node Script",
                suggestedSavePath,
                fileName,
                "cs");

            if (path.Length == 0)
            {
                return false;
            }

            GenerateNodeFile(data, path);

            string relativePath = path.StartsWith(Application.dataPath)
                ? "Assets" + path.Substring(Application.dataPath.Length)
                : path;
            MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePath, typeof(MonoScript));
            AssetDatabase.OpenAsset(script);
            return true;
        }

        internal static void GenerateNodeFile(NodeData data, string path)
        {
            GraphPrefsUtility.SetString(k_PrefsKeySaveNodePath, Path.GetDirectoryName(path), true);
            string name = Path.GetFileNameWithoutExtension(path);
            data.ClassName = name;
            var content = MakeClassString(data);

            using (StreamWriter outfile = new StreamWriter(path))
            {
                outfile.WriteLine(content);
            }

            AssetDatabase.Refresh();
        }

        internal static List<string> GetNamespaceStrings(Dictionary<string, Type> variables)
        {
            HashSet<string> namespaceList = new HashSet<string>();
            namespaceList.Add("System");
            namespaceList.Add("UnityEngine");
            namespaceList.Add("Unity.Behavior");
            if (variables != null)
            {
                foreach (var variable in variables)
                {
                    if (!string.IsNullOrEmpty(variable.Value.Namespace))
                    {
                        namespaceList.Add(variable.Value.Namespace);
                    }
                    if (typeof(IList).IsAssignableFrom(variable.Value))
                    {
                        namespaceList.Add("System.Collections.Generic");
                    }
                }
            }

            var sortedList = namespaceList.ToList();
            sortedList.Sort();
            return sortedList;
        }


        internal static string MakeClassString(NodeData data)
        {
            var builder = new StringBuilder();

            string attributeString = GenerateAttributeString(data);

            var namespaceStrings = GetNamespaceStrings(data.Variables);
            foreach (var namespaceString in namespaceStrings)
            {
                builder.AppendLine($"using {namespaceString};");
            }
            builder.AppendLine($"using {GetNodeTypeString(data.NodeType)} = Unity.Behavior.{GetNodeTypeString(data.NodeType)};");
            builder.AppendLine($"using Unity.Properties;");
            builder.AppendLine();
            builder.AppendLine("[Serializable, GeneratePropertyBag]");
            builder.AppendLine(attributeString);
            builder.AppendLine($"public partial class {data.ClassName} : {GetNodeTypeString(data.NodeType)}");
            builder.AppendLine("{");
            builder.Append(GeneratorUtils.GenerateVariableFields(data.Variables));
            builder.Append(GeneratePortFields(data));
            builder.AppendLine();
            builder.AppendLine("    protected override Status OnStart()");
            builder.AppendLine("    {");
            builder.AppendLine("        return Status.Running;");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    protected override Status OnUpdate()");
            builder.AppendLine("    {");
            builder.AppendLine("        return Status.Success;");
            builder.AppendLine("    }");
            builder.AppendLine();
            builder.AppendLine("    protected override void OnEnd()");
            builder.AppendLine("    {");
            builder.AppendLine("    }");
            builder.AppendLine("}");
            
            return builder.ToString();
        }
         
        internal static bool Edit(NodeData newData, NodeInfo info)
        {
            if (info.FilePath.Length == 0)
            {
                return false;
            }

            string relativePath;
            string alternateDataPath = Application.dataPath.Replace("/", "\\");
            if (info.FilePath.StartsWith(Application.dataPath))
            {
                relativePath = Path.Combine("Assets", info.FilePath.Substring(Application.dataPath.Length + 1));
            }
            else if (info.FilePath.StartsWith(alternateDataPath))
            {
                relativePath = Path.Combine("Assets", info.FilePath.Substring(alternateDataPath.Length + 1));
            }
            else
            {
                Debug.LogWarning(
                    $"Cannot edit script at {info.FilePath}. Ensure the script is contained within the project's Assets folder.");
                return false;
            }

            MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePath, typeof(MonoScript));
            if (script == null)
            {
                Debug.LogWarning($"Cannot load the script at {relativePath}.");
                return false;
            }

            UpdateNodeFileWithData(newData, info, relativePath);
            AssetDatabase.OpenAsset(script);
            return true;
        }

        internal static void UpdateNodeFileWithData(NodeData newData, NodeInfo info, string relativePath)
        {
            MonoScript script = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePath, typeof(MonoScript));

            string newNameNoSpaces = newData.Name.Replace(" ", string.Empty);
            string oldNameNoSpaces = info.Name.Replace(" ", string.Empty);

            // Update the node name in the script file
            string nameTypePattern = @"\b" + oldNameNoSpaces + @"\b(?!\S)";
            string newScriptText = Regex.Replace(script.text, nameTypePattern, newNameNoSpaces);

            // Updates the NodeDescription attribute string with values from the new data
            newScriptText = UpdateAttributeString(newData, newScriptText);

            if (newData.Ports != null)
            {
                newScriptText = UpdatePorts(newScriptText, newData);
            }

            // Update the node variables and Blackboard variable functions
            Dictionary<string, Type> oldVariables = GetStoryVariablesFromInfo(info);
            newScriptText = UpdateVariables(newScriptText, oldVariables, newData);

            File.WriteAllText(relativePath, newScriptText);
            AssetDatabase.RenameAsset(relativePath, newNameNoSpaces);
            AssetDatabase.Refresh();
        }

        internal static string GenerateAttributeString(NodeData data)
        {
            string name = string.IsNullOrEmpty(data.Name) ? "" : $"name: \"{data.Name}\",";
            string story = string.IsNullOrEmpty(data.Story) ? "" : $" story: \"{data.Story}\",";
            string description = string.IsNullOrEmpty(data.Description) ? "" : $" description: \"{data.Description}\",";
            string category = string.IsNullOrEmpty(data.Category) ? "" : $" category: \"{data.Category}\",";
            string icon = string.IsNullOrEmpty(data.Icon) ? "" : $" icon: \"{data.Icon}\",";

            string attributeString = "[NodeDescription(" + name + description + story + icon + category +
                                     $" id: \"{SerializableGUID.Generate()}\")]";
            return attributeString;
        }

        internal static string UpdateAttributeString(NodeData data, string scriptText)
        {
            string name = string.IsNullOrEmpty(data.Name) ? "" : $"name: \"{data.Name}\",";
            string story = string.IsNullOrEmpty(data.Story) ? "" : $" story: \"{data.Story}\",";
            string description = string.IsNullOrEmpty(data.Description) ? "" : $" description: \"{data.Description}\",";
            string category = string.IsNullOrEmpty(data.Category) ? "" : $" category: \"{data.Category}\",";
            string icon = string.IsNullOrEmpty(data.Icon) ? "" : $" icon: \"{data.Icon}\",";
            
            // Find the existing node description attribute string
            const string descriptionPattern = @"\[NodeDescription\(((\n|.)*?)\)\]";
            Match match = Regex.Match(scriptText, descriptionPattern);
            string nodeDescription = match.Value;

            if (match.Success)
            {
                // Find the existing generated id 
                string idPattern = @"(?<=id: \s*"").*?(?=\s*"")";
                Match idMatch = Regex.Match(match.Groups[1].Value, idPattern);
                string id = idMatch.Success ? idMatch.Value : string.Empty;
                
                string attributeString =
                    "[NodeDescription(" + name + description + story + icon + category + $" id: \"{id}\")]";
                string newScriptText = Regex.Replace(scriptText, Regex.Escape(nodeDescription), attributeString);
                return newScriptText;
            }
            
            // If no description attribute string is found from the existing script
            return scriptText;
        }

        private static Dictionary<string, Type> GetStoryVariablesFromInfo(NodeInfo info)
        {
            Dictionary<string, Type> variables = new Dictionary<string, Type>();
            if (info.Variables != null)
            {
                foreach (VariableInfo variable in info.Variables)
                {
                    if (!info.StoryInfo.StoryVariableNames.Contains(variable.Name))
                    {
                        continue;
                    }
                    
                    Type type = variable.Type;
                    Type genericType = type.GetGenericArguments()?[0];
                    variables.Add(variable.Name, genericType);
                }
            }

            return variables;
        }
        
        internal static string UpdateVariables(string scriptText, Dictionary<string, Type> oldVariables, NodeData data)
        {
            string newString = scriptText;
            foreach (KeyValuePair<string, Type> variable in oldVariables)
            {
                string typeOfObject = GeneratorUtils.GetStringForType(variable.Value);
                string pattern = CreateVariableFieldPattern(typeOfObject, variable.Key);

                newString = Regex.Replace(newString, pattern, string.Empty, RegexOptions.Multiline);
            }

            string newVariables = "$&" + GeneratorUtils.GenerateVariableFields(data.Variables);
            newString = Regex.Replace(newString, GetClassPattern(newString, data), newVariables);

            return newString;
        }

        internal static string CreateVariableFieldPattern(string typeOfObject, string variableName)
        {
            string pattern = $@"\[SerializeReference\]\s*public\s+BlackboardVariable<{Regex.Escape(typeOfObject)}>\s+{Regex.Escape(variableName)};\s*\r?\n?";
            return pattern;
        }

        internal static string UpdatePorts(string scriptText, NodeData data)
        {
            string newString = scriptText;
            if (data.OldPorts != null)
            {
                foreach (string portName in data.OldPorts)
                {
                    string pattern = CreatePortFieldPattern(portName);
                    newString = Regex.Replace(newString, pattern, string.Empty, RegexOptions.Multiline);
                }
            }

            string newPortFields = "$&" + GeneratePortFields(data);
            newString = Regex.Replace(newString, GetClassPattern(newString, data), newPortFields);

            return newString;
        }

        internal static string CreatePortFieldPattern(string portName)
        {
            string pattern = $@"\[SerializeReference\]\s*public\s+Node\s+{Regex.Escape(portName)};\s*\r?\n?";
            return pattern;
        }

        private static string GeneratePortFields(NodeData data)
        {
            StringBuilder functionStringBuilder = new StringBuilder();
            using (StringWriter outfile = new StringWriter(functionStringBuilder))
            {
                if (data.Ports != null)
                {
                    foreach (string portName in data.Ports)
                        outfile.WriteLine(
                            $"    [SerializeReference] public Node {GeneratorUtils.RemoveSpaces(GeneratorUtils.NicifyString(portName))};");
                }
            }

            return functionStringBuilder.ToString();
        }
        
        internal static string GetClassPattern(string text, NodeData data)
        {
            string classNamePattern = $@"\w+\s+class\s+(\w+)\s*:\s*{GetNodeTypeString(data.NodeType)}\s*";
            Match match = Regex.Match(text, classNamePattern);
            if (match.Success)
            {
                string className = match.Groups[1].Value;
                return $@"^[\s\S]*?\bclass\s+{className}\b\s*:\s*\w+\s*{{\r?\n";
            }
            return $@"^[\s\S]*?\bclass\s+{data.Name.Replace(" ", string.Empty)}\b\s*:\s*\w+\s*{{\r?\n";
        }

        private static int FindClassClosingBracket(string script, NodeData data)
        {
            string className = data.Name.Replace(" ", string.Empty);
            string pattern = $@"^[\s\S]*?\bclass\s+{className}\b\s*:\s*\w+\s*{{\r?\n";

            Match match = Regex.Match(script, pattern);
            if (!match.Success)
            {
                return -1;
            }

            int startIndex = match.Groups[1].Index;
            int count = 0;
            for (int i = startIndex; i < script.Length; i++)
                if (script[i] == '{')
                {
                    count++;
                }
                else if (script[i] == '}')
                {
                    count--;
                    if (count == 0)
                    {
                        return i;
                    }
                }

            return -1;
        }

        internal static string GetNodeTypeString(NodeType nodeType)
        {
            return nodeType.ToString();
        }
    }
}
#endif