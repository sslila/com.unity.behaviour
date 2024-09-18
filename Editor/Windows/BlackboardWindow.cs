using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class BlackboardWindow : EditorWindow
    {
        [SerializeField]
        private BehaviorBlackboardAuthoringAsset m_Asset;
        internal BehaviorBlackboardAuthoringAsset Asset
        {
            get => m_Asset;
            set
            {
                m_Asset = value;
                if (Editor.Asset != value)
                {
                    Editor.Load(value);
                }
                titleContent.text = m_Asset.name + " (Blackboard)";
            }
        }
        
        [SerializeField]
        private int m_DebugAgentId;

        internal BlackboardEditor Editor;
        private Panel m_AppUIPanel;
        
        private const string k_WindowDockedKey = "BlackboardWindowDocked";
        private const string k_WindowXKey = "BlackboardWindowX";
        private const string k_WindowYKey = "BlackboardWindowY";
        private const string k_WindowWidthKey = "BlackboardWindowWidth";
        private const string k_WindowHeightKey = "BlackboardWindowHeight";

        private void OnEnable()
        {
            this.SetAntiAliasing(8);
            titleContent.text = "Blackboard";
            minSize = new Vector2(300, 400);

            Editor = new BlackboardEditor();
            m_AppUIPanel = WindowUtils.CreateAndGetAppUIPanel(Editor, rootVisualElement);
            
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            
            if (m_Asset != null)
            {
                Editor.Load(m_Asset);
            }

            SetWindowTitleFromAsset();
            
            EditorApplication.playModeStateChanged += OnEditorStateChange;
            Editor.OnSave += base.SaveChanges;
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
        
        private void OnLostFocus()
        {
            AutoSaveIfEnabledInEditor();
        }

        private void OnEditorStateChange(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                AutoSaveIfEnabledInEditor();
            }

            if (stateChange == PlayModeStateChange.ExitingPlayMode)
            {
                AutoSaveIfEnabledInEditor();
            }
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
                titleContent.text = m_Asset.name + " (Blackboard)";
            }
        }
        
        internal void OnFocus()
        {
            // If the Blackboard asset is deleted outside of Unity, this will close the window when focused.
            if (!ReferenceEquals(m_Asset, null) && !EditorUtility.IsPersistent(m_Asset))
            {
                Close();
            }

            SetWindowTitleFromAsset();
        }
        
        [InitializeOnLoadMethod]
        private static void RegisterWindowDelegates()
        {
            BlackboardWindowDelegate.openHandler = Open;
        }

        private void AutoSaveIfEnabledInEditor()
        {
            if (Editor is { AutoSaveIsEnabled: true })
            {
                Editor.OnAssetSave();
            }
        }

        internal static void Open(BehaviorBlackboardAuthoringAsset asset)
        {
            BlackboardWindow[] windows = Resources.FindObjectsOfTypeAll<BlackboardWindow>();
            foreach (BlackboardWindow window in windows)
            {
                if (window.Asset != asset)
                {
                    continue;
                }

                window.Show();
                window.Focus();
                return;
            }

            // Create a window using docking if possible.
            bool willBeUndocked = !HasOpenInstances<BlackboardWindow>();
            BlackboardWindow newWindow = CreateWindow<BlackboardWindow>(typeof(BlackboardWindow));
            if (willBeUndocked)
            {
                WindowUtils.ApplyWindowOffsetFromPrefs(newWindow, k_WindowDockedKey, k_WindowXKey, k_WindowYKey, k_WindowWidthKey, k_WindowHeightKey);
            }
            newWindow.titleContent.text = asset.name;
            newWindow.Asset = asset;
            newWindow.Show();
        }
    }
}
