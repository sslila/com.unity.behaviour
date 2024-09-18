using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.Behavior.GenerativeAI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class BehaviorWindow : EditorWindow
    {
        [SerializeField]
        private BehaviorAuthoringGraph m_Asset;
        internal BehaviorAuthoringGraph Asset
        {
            get => m_Asset;
            set
            {
                m_Asset = value;
                if (m_Editor.Asset != value)
                {
                    m_Editor.Load(value);
                }
                titleContent.text = m_Asset.name;
            }
        }

        [SerializeField]
        private int m_DebugAgentId;
        internal BehaviorGraphEditor m_Editor;
        private Panel m_AppUIPanel;

        private const string k_WindowDockedKey = "WindowDocked";
        private const string k_WindowXKey = "WindowX";
        private const string k_WindowYKey = "WindowY";
        private const string k_WindowWidthKey = "WindowWidth";
        private const string k_WindowHeightKey = "WindowHeight";

        private void OnEnable()
        {
            this.SetAntiAliasing(8);
            titleContent.text = "Behavior";
            minSize = new Vector2(1000, 600);

            m_Editor = new BehaviorGraphEditor();

            // Move panel creation to WindowUtils
            //need a specific panel from Muse.AppUI namespace to support Muse Common pop up.
            m_AppUIPanel = WindowUtils.CreateAndGetAppUIPanel(m_Editor, rootVisualElement);
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            if (m_Asset != null)
            {
                m_Editor.Load(m_Asset);
            }

            SetWindowTitleFromAsset();

            // Unsaved changes message, which will be displayed on window exit when editor auto-save is not enabled.
            saveChangesMessage = "You've made changes to the graph. Do you wish to apply and save them to the runtime asset?";

            EditorApplication.playModeStateChanged += OnEditorStateChange;
            m_Editor.OnSave += base.SaveChanges;
            m_Editor.DebugAgentSelected += agentID => { m_DebugAgentId = agentID;};
            m_Editor.SetActiveGraphToDebugAgent(m_DebugAgentId);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnEditorStateChange;
            AutoSaveIfEnabledInEditor();
            SetWindowPosition();
            rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (m_AppUIPanel == null)
            {
                return;
            }
            m_AppUIPanel.forceUseTooltipSystem = (change == PlayModeStateChange.EnteredPlayMode);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Note: The window undocks when closed, so we store the value when geometry changes.
            EditorPrefs.SetBool(k_WindowDockedKey, docked);
            SetWindowPosition();
        }

        private void SetWindowPosition()
        {
            EditorPrefs.SetFloat(k_WindowXKey, position.x);
            EditorPrefs.SetFloat(k_WindowYKey, position.y);
            EditorPrefs.SetFloat(k_WindowWidthKey, position.width);
            EditorPrefs.SetFloat(k_WindowHeightKey, position.height);
        }

        private void SetWindowTitleFromAsset()
        {
            if (m_Asset != null)
            {
                titleContent.text = m_Asset.name;
            }
        }

        internal void OnFocus()
        {
            // If the authoring graph is deleted outside of Unity, this will close the window when focused.
            if (!ReferenceEquals(m_Asset, null) && !EditorUtility.IsPersistent(m_Asset))
            {
                Close();
            }

            SetWindowTitleFromAsset();
            UpdateGraphEditor();
        }

        private void OnLostFocus()
        {
            AutoSaveIfEnabledInEditor();
            UpdateGraphEditor();
        }

        private void UpdateGraphEditor()
        {
            if (m_Editor == null)
            {
                return;
            }

            // Reload the editor if any graph or blackboard assets which the graph is depending on has changed.
            if (m_Editor.HasBlackboardDependencyChanged() || m_Editor.HasGraphDependencyChanged())
            {
                m_Editor.Load(Asset);
            }
        }

        private void OnEditorStateChange(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.EnteredEditMode)
            {
                m_Editor.BehaviorGraphView.ResetNodesUI();
            }
            if (stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                m_Editor.SetActiveGraphToDebugAgent(m_DebugAgentId);
                AutoSaveIfEnabledInEditor();
            }

            if (stateChange == PlayModeStateChange.ExitingPlayMode)
            {
                AutoSaveIfEnabledInEditor();
            }
        }

        internal static void ShowSaveIndicator(BehaviorAuthoringGraph asset)
        {
            BehaviorWindow[] windows = Resources.FindObjectsOfTypeAll<BehaviorWindow>();
            foreach (BehaviorWindow window in windows)
            {
                if (window.Asset != asset)
                {
                    continue;
                }
                window.hasUnsavedChanges = true;
            }
        }

        internal static void Open(BehaviorAuthoringGraph asset)
        {
            BehaviorWindow[] windows = Resources.FindObjectsOfTypeAll<BehaviorWindow>();
            foreach (BehaviorWindow window in windows)
            {
                if (window.Asset == asset)
                {
                    window.Show();
                    window.Focus();
                    return;
                }
            }

            // Create a window using, docking if possible..
            bool willBeUndocked = !HasOpenInstances<BehaviorWindow>();
            BehaviorWindow newWindow = CreateWindow<BehaviorWindow>(typeof(BehaviorWindow));
            if (willBeUndocked)
            {
                WindowUtils.ApplyWindowOffsetFromPrefs(newWindow, k_WindowDockedKey, k_WindowXKey, k_WindowYKey, k_WindowWidthKey, k_WindowHeightKey);
            }
            newWindow.titleContent.text = asset.name;
            newWindow.Asset = asset;
            newWindow.m_Editor.IsAssetVersionUpToDate();
            newWindow.Show();
        }

        private void AutoSaveIfEnabledInEditor()
        {
            if (m_Editor is { AutoSaveIsEnabled: true })
            {
                m_Editor.OnAssetSave();
            }
        }

        [InitializeOnLoadMethod]
        private static void RegisterWindowDelegates()
        {
            BehaviorWindowDelegate.openHandler = Open;
            BehaviorWindowDelegate.showSaveIndicatorHandler = ShowSaveIndicator;
        }
    }
}
