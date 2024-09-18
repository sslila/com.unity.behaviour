using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class CreateCommandHelper
    {
#if UNITY_EDITOR
        private static readonly string k_PrefsKeySaveCommandPath = "SaveCommandPath";
        private static readonly string k_PrefsKeySaveCommandHandlerPath = "SaveCommandHandlerPath";
        //[MenuItem("Assets/Create/Graph/Dispatcher/Command", false, 1)]
        private static void CreateNewAsset()
        {
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveCommandPath, Application.dataPath, true);
            suggestedSavePath = Directory.Exists(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                    "Create Dispatcher Command",
                    suggestedSavePath,
                    "NewCommand",
                    "cs");
        
            if (path.Length == 0)
            {
                return;
            }
            GraphPrefsUtility.SetString(k_PrefsKeySaveCommandPath, Path.GetDirectoryName(path), true);
            string name = Path.GetFileNameWithoutExtension(path);
            using (StreamWriter outfile =
                    new StreamWriter(path))
            {
                outfile.WriteLine("using System;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using Unity.Behavior.GraphFramework;");
                outfile.WriteLine("");
                outfile.WriteLine($"internal class {name} : Command");
                outfile.WriteLine("{");
                outfile.WriteLine("    ");
                outfile.WriteLine("}");
            }


            string suggestedHandlerSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveCommandHandlerPath, Path.GetDirectoryName(path), true);
            suggestedHandlerSavePath = Directory.Exists(suggestedHandlerSavePath) ? suggestedHandlerSavePath : Application.dataPath;
            string handlerPath = EditorUtility.SaveFilePanel(
                    "Create Dispatcher Command Handler",
                    suggestedHandlerSavePath,
                    $"{name}Handler",
                    "cs");

            GraphPrefsUtility.SetString(k_PrefsKeySaveCommandHandlerPath, Path.GetDirectoryName(handlerPath), true);
            using (StreamWriter outfile =
                    new StreamWriter(handlerPath))
            {
                outfile.WriteLine("using System;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using Unity.Behavior.GraphFramework;");
                outfile.WriteLine("");
                outfile.WriteLine($"internal class {name}Handler : CommandHandler<{name}>");
                outfile.WriteLine("{");
                outfile.WriteLine($"    public override bool Process({name} command)");
                outfile.WriteLine("    {");
                outfile.WriteLine("        // Have we processed the command and wish to block further processing?");
                outfile.WriteLine("        return false;");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
            }
        
            AssetDatabase.Refresh();
        
            string relativePathToCommand = path.StartsWith(Application.dataPath) ? ("Assets" + path.Substring(Application.dataPath.Length)) : path;
            MonoScript commandScript = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePathToCommand, typeof(MonoScript));
            AssetDatabase.OpenAsset(commandScript);
        
        
            string relativePathToHandler = handlerPath.StartsWith(Application.dataPath) ? ("Assets" + handlerPath.Substring(Application.dataPath.Length)) : handlerPath;
            MonoScript handlerScript = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePathToHandler, typeof(MonoScript));
            AssetDatabase.OpenAsset(handlerScript);
        }
#endif
    }
}