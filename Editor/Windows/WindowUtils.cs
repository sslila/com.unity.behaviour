using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal static class WindowUtils
    {
        public static Panel CreateAndGetAppUIPanel(VisualElement editor, VisualElement rootVisualElement)
        {
            // Place the App UI stylesheet at the root level of the window.
            rootVisualElement.styleSheets.Add(ResourceLoadAPI.Load<ThemeStyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/AppUITheme.tss"));
            var appUIPanel = new Panel
            {
                scale = "small"
            };
            appUIPanel.AddToClassList("unity-editor");
            appUIPanel.Add(editor);
            appUIPanel.popupContainer.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/PopupStylesheet.uss"));
            rootVisualElement.Add(appUIPanel);

            // Handle theme switching.
            if (EditorGUIUtility.isProSkin)
            {
                appUIPanel.theme = "editor-dark";
                rootVisualElement.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/AppUIDarkTheme.uss"));
            }
            else
            {
                appUIPanel.theme = "editor-light";
                rootVisualElement.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/AppUILightTheme.uss"));
            }

            return appUIPanel;
        }

        public static void ApplyWindowOffsetFromPrefs(EditorWindow window, string windowDockedKey, string windowXKey, string windowYKey, string windowWidthKey, string windowHeightKey)
        {
            // If the window was last docked, offset the position to account for the window tab for easy docking.
            float offset = EditorPrefs.GetBool(windowDockedKey, false) ? 20f : 0f;

            // Create the window and restore the last used window position.
            window.position = new Rect(
                EditorPrefs.GetFloat(windowXKey, 0) + offset,
                EditorPrefs.GetFloat(windowYKey, 0) + offset,
                EditorPrefs.GetFloat(windowWidthKey, 1200),
                EditorPrefs.GetFloat(windowHeightKey, 800));
        }
    }
}
