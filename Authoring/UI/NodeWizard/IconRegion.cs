#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class IconRegion : VisualElement
    {
        private const string k_ResourceFolder = "Resources/";
        private const string k_IconHelpText = "You can set an icon for your node. The file needs to be under a 'Resources' folder to be displayed correctly.";
        
        internal string IconPath { get; set; }
        internal ObjectField IconField => m_IconField;
        
        private readonly ObjectField m_IconField;
        private readonly HelpBox m_IconHelpBox;

        internal System.Action OnIconFieldChangedCallback;
        
        internal IconRegion()
        {
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/IconRegionLayout.uxml").CloneTree(this);
            
            m_IconField = this.Q<ObjectField>("IconField");
            m_IconField.objectType = typeof(Texture2D);
            m_IconField.RegisterValueChangedCallback(OnIconFieldChanged);
            
            m_IconHelpBox = this.Q<HelpBox>("IconHelpBox");
            m_IconHelpBox.text = k_IconHelpText;
        }
        
        private void OnIconFieldChanged(ChangeEvent<UnityEngine.Object> changeEvent)
        {
            string currentPath = AssetDatabase.GetAssetPath(changeEvent.newValue);
            IconPath = IsFilePathValid(currentPath) ? GetIconFilePath(currentPath) : "";
            OnIconFieldChangedCallback?.Invoke();
        }
        
        internal static string GetIconFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
            int startIndex = path.LastIndexOf(k_ResourceFolder, StringComparison.Ordinal) + k_ResourceFolder.Length;
            return Path.ChangeExtension(path[startIndex..], null);
        }
        
        private bool IsFilePathValid(string path)
        {
            // if the path is empty, we consider it "valid", but we need to remove possible warning text
            if (string.IsNullOrWhiteSpace(path))
            {
                m_IconHelpBox.text = k_IconHelpText;
                m_IconHelpBox.messageType = HelpBoxMessageType.None;
                return true;
            }
            if (path.Contains(k_ResourceFolder + "unity_builtin_extra"))
            {
                m_IconHelpBox.text = "Built in assets cannot be added as an icon. The file needs to be located under a 'Resources' folder to be displayed with the node.";
                m_IconHelpBox.messageType = HelpBoxMessageType.Warning;
                return false;
            }
            if (!path.Contains(k_ResourceFolder))
            {
                m_IconHelpBox.text = "Icon file needs to be located under a 'Resources' folder to be displayed with the node.";
                m_IconHelpBox.messageType = HelpBoxMessageType.Warning;
                return false;
            }
            
            m_IconHelpBox.text = k_IconHelpText;
            m_IconHelpBox.messageType = HelpBoxMessageType.None;

            return true;
        }
    }
}
#endif