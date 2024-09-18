#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class NodeModelCreateHelper
    {
        private static readonly string k_PrefsKeySaveNodeModelPath = "SaveNodeModelPath";
        private static readonly string k_PrefsKeySaveNodeUIPath = "SaveNodeUIPath";

        //[MenuItem("Assets/Create/Graph/Node Model and UI", false, 1)]
        private static void CreateNewAsset()
        {
            string suggestedSavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveNodeModelPath, Application.dataPath, true);
            suggestedSavePath = Directory.Exists(suggestedSavePath) ? suggestedSavePath : Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                    "Create Node Model",
                    suggestedSavePath,
                    "NewNodeModel",
                    "cs");

            if (path.Length == 0)
            {
                return;
            }

            GraphPrefsUtility.SetString(k_PrefsKeySaveNodeModelPath, Path.GetDirectoryName(path), true);
            string nodeModelName = Path.GetFileNameWithoutExtension(path);
            using (StreamWriter outfile =
                    new StreamWriter(path))
            {
                outfile.WriteLine("using System;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using Unity.Behavior.GraphFramework;");
                outfile.WriteLine("");
                outfile.WriteLine($"internal class {nodeModelName} : NodeModel");
                outfile.WriteLine("{");
                outfile.WriteLine("    ");
                outfile.WriteLine("}");
            }


            string suggestedNodeUISavePath = GraphPrefsUtility.GetString(k_PrefsKeySaveNodeUIPath, Path.GetDirectoryName(path), true);
            suggestedNodeUISavePath = Directory.Exists(suggestedNodeUISavePath) ? suggestedNodeUISavePath : Application.dataPath;
            string nameWithoutNodeModel = nodeModelName.Replace("NodeModel", "");
            string nodeUIPath = EditorUtility.SaveFilePanel(
                    "Create Node UI",
                    suggestedNodeUISavePath,
                    $"{nameWithoutNodeModel}NodeUI",
                    "cs");

            GraphPrefsUtility.SetString(k_PrefsKeySaveNodeUIPath, Path.GetDirectoryName(nodeUIPath), true);
            string nodeUIName = Path.GetFileNameWithoutExtension(nodeUIPath);

            using (StreamWriter outfile =
                    new StreamWriter(nodeUIPath))
            {
                outfile.WriteLine("using System;");
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using Unity.Behavior.GraphFramework;");
                outfile.WriteLine("");
                outfile.WriteLine($"[Graph.NodeUIAttribute(typeof({nodeModelName}))]");
                outfile.WriteLine($"internal class {nodeUIName} : NodeUI");
                outfile.WriteLine("{");
                outfile.WriteLine($"    public {nodeUIName}(NodeModel nodeModel) : base(nodeModel)");
                outfile.WriteLine("    {");
                outfile.WriteLine("         ");
                outfile.WriteLine("    }");
                outfile.WriteLine("}");
            }

            AssetDatabase.Refresh();

            string relativePathToNodeModel = path.StartsWith(Application.dataPath) ? ("Assets" + path.Substring(Application.dataPath.Length)) : path;
            MonoScript nodeModelScript = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePathToNodeModel, typeof(MonoScript));
            AssetDatabase.OpenAsset(nodeModelScript);


            string relativePathToNodeUI = nodeUIPath.StartsWith(Application.dataPath) ? ("Assets" + nodeUIPath.Substring(Application.dataPath.Length)) : nodeUIPath;
            MonoScript nodeUIScript = (MonoScript)AssetDatabase.LoadAssetAtPath(relativePathToNodeUI, typeof(MonoScript));
            AssetDatabase.OpenAsset(nodeUIScript);
        }
    }
}
#endif