using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace Unity.Behavior 
{
    internal static class IL2CPPGenericTypesGenerator
    {
        public static readonly string k_PrefsKeySavePath = "IL2CPPSavePath";
        public static readonly string k_DefaultFileName = "BehaviorIL2CPPTypes";

        //[MenuItem("Behavior/Generate IL2CPP Build File")]
        public static void GenerateFile()
        {
            string fileName = k_DefaultFileName;
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySavePath, Application.dataPath, true);
            suggestedSavePath = Util.IsPathRelativeToProjectAssets(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                $"Create IL2CPP Build File",
                suggestedSavePath,
                fileName,
                "cs");

            if (path.Length == 0)
            {
                return;
            }
            
            GraphPrefsUtility.SetString(k_PrefsKeySavePath, Path.GetDirectoryName(path), true);
            CreateAndWriteFile(path);
        }
        
        public static void CreateAndWriteFile(string path)
        {
            string fileString = CreateFileString();
            using (StreamWriter outfile = new StreamWriter(path))
            {
                outfile.WriteLine(fileString);
            }
            
            AssetDatabase.Refresh();
        }

        private static string CreateFileString()
        {
            /* Generate this class:
             * sealed class BehaviorTypes
             * {
             *     private BehaviorTypes() { }
             * 
             *     private TypedVariableModel<int> m_TypedVariableModelint;
             *     ...
             *     private EnumLinkField<CharacterState> m_CharacterStateEnum;
             * }
             */
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using Unity.Behavior.GraphFramework;");
            builder.AppendLine();   
            builder.AppendLine("namespace Unity.Behavior");
            builder.AppendLine("{");
            builder.AppendLine("\tsealed class BehaviorTypes");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\tprivate BehaviorTypes() { }");
            builder.AppendLine();
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            List<Type> types = Util.GetSupportedTypes().ToList();
            foreach (Type variableType in types.Where(type => type.IsVisible))
            {
                string realTypeName = GetRealTypeName(variableType);
                string typeWithoutSpecialCharacters = rgx.Replace(realTypeName, "");
                builder.AppendLine($"\t\tprivate TypedVariableModel<{realTypeName}> m_TypedVariableModel{typeWithoutSpecialCharacters};");
            }
            foreach (Type enumType in types.Where(type => type.IsEnum))
            {
                string typeWithoutSpecialCharacters = rgx.Replace(enumType.FullName, "");
                builder.AppendLine($"\t\tprivate EnumLinkField<{GetRealTypeName(enumType)}> m_{typeWithoutSpecialCharacters}Enum;");
            }
            builder.AppendLine("\t}");
            builder.AppendLine("}");
            return builder.ToString();
        }
        
        public static string GetRealTypeName(Type t)
        {
            if (!t.IsGenericType)
                return t.FullName.Replace('+', '.');

            StringBuilder sb = new StringBuilder();
            sb.Append(t.FullName.Substring(0, t.FullName.IndexOf('`')));
            sb.Append('<');
            bool appendComma = false;
            foreach (Type arg in t.GetGenericArguments())
            {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealTypeName(arg));
                appendComma = true;
            }
            sb.Append('>');
            return sb.ToString();
        }
    }

    internal static class GraphTypesLinkFileGenerator
    {
        public static string k_PrefsKeySavePath => IL2CPPGenericTypesGenerator.k_PrefsKeySavePath;
        public static readonly string k_DefaultFileName = "link";
        public static string k_GraphAssembly => typeof(GraphEditor).Assembly.GetName().Name;
        public static string k_BehaviorAssembly => typeof(BehaviorGraphEditor).Assembly.GetName().Name;
        
        public static IEnumerable<Type> GetUserTypes()
        {
            IEnumerable<Type> nodeModelTypes = TypeCache.GetTypesDerivedFrom<NodeModel>();
            IEnumerable<Type> runtimeNodeTypes = TypeCache.GetTypesDerivedFrom<Node>(); 
            IEnumerable<Type> enumTypes = TypeCache.GetTypesWithAttribute<BlackboardEnumAttribute>();
            IEnumerable<Type> eventChannelTypes =
                TypeCache.GetTypesWithAttribute<EventChannelDescriptionAttribute>();
            
            IEnumerable<Type> allTypes = nodeModelTypes.Concat(runtimeNodeTypes).Concat(enumTypes).Concat(eventChannelTypes);
            bool IsNotPackageAssembly(string assemblyName) => !assemblyName.StartsWith(k_GraphAssembly) && !assemblyName.StartsWith(k_BehaviorAssembly);
            return allTypes.Where(type => IsNotPackageAssembly(type.Assembly.GetName().FullName));
        }
        
        //[MenuItem("Behavior/Generate IL2CPP Link File")]
        public static void GenerateFile()
        {
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySavePath, Application.dataPath, true);
            suggestedSavePath = Util.IsPathRelativeToProjectAssets(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                $"Create Linked Types File",
                suggestedSavePath,
                k_DefaultFileName,
                "xml");

            if (path.Length == 0)
            {
                return;
            }
            
            GraphPrefsUtility.SetString(k_PrefsKeySavePath, Path.GetDirectoryName(path), true);
            CreateAndWriteFile(path);
        }
        
        public static void CreateAndWriteFile(string path)
        {
            using (StreamWriter outfile = new StreamWriter(path))
            {
                outfile.WriteLine(CreateFileString());
            }
            
            AssetDatabase.Refresh();
        }
                
        private static string CreateFileString()
        {
            /* Generate this class:
             * <linker>
             *   <assembly fullname="AssemblyName">
             *     <type fullname="AssemblyName.TypeName"/>
             *     ...
             *   </assembly>
             * </linker>
             */
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<linker>");
            builder.AppendLine($"  <assembly fullname=\"{k_GraphAssembly}\" ignoreIfUnreferenced=\"1\"/>");
            builder.AppendLine($"  <assembly fullname=\"{k_BehaviorAssembly}\" ignoreIfUnreferenced=\"1\"/>");
            
            IEnumerable<Type> types = GetUserTypes();
            IEnumerable<IGrouping<string, Type>> assemblyGroups = types.GroupBy(type => type.Assembly.GetName().Name);
            // For each assembly, write the assembly name and all the types to be preserved in the assembly.
            foreach (IGrouping<string, Type> assemblyGroup in assemblyGroups)
            {
                builder.AppendLine($"  <assembly fullname=\"{assemblyGroup.Key}\">");
                foreach (Type preservedType in assemblyGroup)
                {    
                    builder.AppendLine($"    <type fullname=\"{preservedType.FullName}\" preserve=\"all\"/>");
                }
                builder.AppendLine("  </assembly>");
            }
            builder.AppendLine("</linker>");
            return builder.ToString(); 
        }
    }
    
    internal class OnBuildIL2CPPGenericTypesGenerator : BuildPlayerProcessor, IUnityLinkerProcessor
    {
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            bool isIL2CPPBuild = PlayerSettings.GetScriptingBackend(namedBuildTarget) == ScriptingImplementation.IL2CPP;
            if (isIL2CPPBuild) 
            {
                IL2CPPGenericTypesGenerator.CreateAndWriteFile($"{Application.dataPath}/{IL2CPPGenericTypesGenerator.k_DefaultFileName}.cs");
            }
        }
        
        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            bool isIL2CPPBuild = PlayerSettings.GetScriptingBackend(namedBuildTarget) == ScriptingImplementation.IL2CPP;
            if (isIL2CPPBuild) 
            {
                string path = $"{Application.dataPath}/{GraphTypesLinkFileGenerator.k_DefaultFileName}.xml";
                GraphTypesLinkFileGenerator.CreateAndWriteFile(path);
                return path;
            }
            return string.Empty;
        }
    }
}
