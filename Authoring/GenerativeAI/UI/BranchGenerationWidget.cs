#if ENABLE_MUSE_BEHAVIOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Behavior.GraphFramework;
using JetBrains.Annotations;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Behavior.GenerativeAI
{
    internal class BranchGenerationWidget : VisualElement
    {
        private readonly BehaviorGraphView m_GraphView;
        [CanBeNull] private string m_ConversationId;
        private List<NodeModel> m_GeneratedNodes = new();
        private NodeModel RootNode => m_GeneratedNodes.FirstOrDefault();
        private Vector2 m_WorldPosition;
        private readonly ILanguageModel m_LanguageModel = new MuseChatModel();
        private readonly Button m_CreateButton;
        private AppBar m_WidgetAppBar;
        internal ActionButton CloseButton;
        
        public delegate void BranchGeneratedCallback();
#pragma warning disable 67
        public event BranchGeneratedCallback OnBranchGenerated;
#pragma warning restore 67


        internal BranchGenerationWidget(BehaviorGraphView graphView, Vector2 worldPosition)
        {
            m_WorldPosition = worldPosition;
            m_GraphView = graphView;
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/GenerativeAI/UI/Assets/BranchGenerationWidgetStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/GenerativeAI/UI/Assets/BranchGenerationWidgetLayout.uxml").CloneTree(this);
            AddToClassList("NewBranchModal");
            AddToClassList("BehaviorModal");
            m_WidgetAppBar = this.Q<AppBar>("BranchGenerationWidgetAppBar");
            m_WidgetAppBar.title = "New Branch";
            CloseButton = this.Q<ActionButton>("CloseButton");

            HelpText helpTextLabel = this.Q<HelpText>("HelpText");
            helpTextLabel.Text = "Generate a branch from text using nodes available in your project.";
            helpTextLabel.AddToClassList("ShowHelpText");
            TextArea textField = this.Q<TextArea>("PromptTextField");
#if UNITY_2023_1_OR_NEWER
            textField.placeholder = "Describe your branch here";
#endif
            m_CreateButton = this.Q<Button>("CreateButton");
#if UNITY_EDITOR
            m_CreateButton.clickable.clicked += () => OnCreateClicked(graphView, textField);
            MuseBehaviorUtilities.RegisterSessionStatusChangedCallback(UpdateCreateButtonWithAccess);
            bool hasAccess = MuseBehaviorUtilities.IsSessionUsable;
            m_CreateButton.SetEnabled(hasAccess);
            if (!hasAccess)
            {
                m_CreateButton.tooltip = MuseUtility.k_UserCallToAction;
            }
#endif
        }

#if UNITY_EDITOR
        private void UpdateCreateButtonWithAccess(bool isUsable)
        {
            m_CreateButton.SetEnabled(isUsable);
            if (!isUsable)
            {
                m_CreateButton.tooltip = MuseUtility.k_UserCallToAction;
            }
        }

        private void OnCreateClicked(BehaviorGraphView graphView, TextArea textField)
        {
            string userDescription = textField.value;
            if (string.IsNullOrEmpty(userDescription))
            {
                Debug.LogError("Cannot generate branch without a description of the desired behavior.");
                return;
            }

            List<NodeInfo> nodeInfos = NodeRegistry.NodeInfos;
            GraphAsset asset = graphView.Asset;

            string actions = BranchGeneratorUtility.GetActions(nodeInfos);
            string composites = BranchGeneratorUtility.GetComposites(nodeInfos);
            string modifiers = BranchGeneratorUtility.GetModifiers(nodeInfos);
            string variables = BranchGeneratorUtility.GetBlackboardVariables(asset.Blackboard.Variables);
            string prompt =
                BranchGeneratorUtility.ReplaceInPrompt(actions, composites, modifiers, variables, userDescription);
            m_ConversationId = null; //reset conversation
            m_LanguageModel.Chat(prompt, response =>
            {
                if (response.output.Contains("Unable"))
                {
                    Debug.LogWarning("Unable to generate a branch from the provided description. Please try again.");
                    return;
                }

                m_ConversationId = response.conversationId;
                GenerateBranch(graphView, m_WorldPosition, response.output, nodeInfos);
                MuseBehaviorUtilities.UpdateUsage();
                OnBranchGenerated?.Invoke();
            }, "branch-generation");
        }
        
        private void GenerateBranch(BehaviorGraphView graphView, Vector2 position, string response,
            List<NodeInfo> nodeInfos)
        {
            m_GeneratedNodes = BranchGeneratorUtility.GenerateNodes(graphView.Asset, position, response, nodeInfos);

            if (RootNode != null)
            {
                m_GraphView.ViewState.ViewStateUpdated += AlignNodesAfterNextUIUpdate;
            }

            if (m_GeneratedNodes?.Count > 0)
            {
                CreateCorrectionFrame(graphView);
            }
        }

        private void CreateCorrectionFrame(BehaviorGraphView graphView)
        {
            BehaviorGraphEditor graphEditor = graphView.GetFirstAncestorOfType<BehaviorGraphEditor>();
            VisualElement panelContent = new VisualElement();
            panelContent.name = "BranchGenerationWidget";
            panelContent.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/GenerativeAI/UI/Assets/BranchGenerationWidgetStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/GenerativeAI/UI/Assets/BranchGenerationWidgetLayout.uxml").CloneTree(panelContent);
            panelContent.AddToClassList("HideHelpText");
            
            FloatingPanel frame = FloatingPanel.Create(panelContent, graphView, panelContent.name, FloatingPanel.DefaultPosition.BottomRight, true);
            frame.Title = "Correction?";
            graphEditor.Q<VisualElement>("EditorPanel").Add(frame);

            TextArea textField = panelContent.Q<TextArea>("PromptTextField");
#if UNITY_2023_1_OR_NEWER
            textField.placeholder = "Enter correction here";
#endif
            schedule.Execute(() => { textField.ElementAt(0).Focus(); });

            Button generateButton = panelContent.Q<Button>("CreateButton");
            generateButton.title = "Apply Correction";

            generateButton.clickable.clicked += () => OnCorrectClicked(graphView, frame, textField);
        }

        private void OnCorrectClicked(BehaviorGraphView graphView, FloatingPanel floatingPanel, TextArea textField)
        {
            string correction = textField.value;
            if (string.IsNullOrEmpty(correction))
            {
                Debug.LogError("Please input something to correct.");
                return;
            }

            floatingPanel.Remove();

            string correctionPrompt = ResourceLoadAPI.Load<TextAsset>("Packages/com.unity.behavior/Authoring/GenerativeAI/Assets/Prompts/CorrectionPrompt.txt").text;
            correctionPrompt = correctionPrompt.Replace("{correction}", correction);

            m_LanguageModel.Chat(correctionPrompt, response =>
            {
                if (response.output.Contains("Unable"))
                    return;

                // Delete existing nodes
                foreach (NodeModel node in m_GeneratedNodes)
                    graphView.Asset.DeleteNode(node);

                List<NodeInfo> nodeInfos = NodeRegistry.NodeInfos;
                GenerateBranch(graphView, transform.position, response.output, nodeInfos);
                MuseBehaviorUtilities.UpdateUsage();
            }, "branch-generation-correction", m_ConversationId);
        }
#endif

        private void AlignNodesAfterNextUIUpdate()
        {
            m_GraphView.ViewState.ViewStateUpdated -= AlignNodesAfterNextUIUpdate;
            AlignNodesAndCenter();
        }
        
        private void AlignNodesAndCenter()
        {
            // Nodes have been created in the asset. During the next frame, UI will be created.
            // On the frame *after* that, the VisualElements will have proper layout information
            // we can use to reposition nodes.
            NodeUI rootUI = GetNodeUI(RootNode);

            var nodeUIs = new List<NodeUI>();
            foreach (var node in m_GeneratedNodes)
            {
                var nodeUi = GetNodeUI(node);
                if (nodeUi.IsInSequence)
                    continue;
                nodeUIs.Add(nodeUi);
            }

            // Have to wait a frame for the node to width and height to be initialized.
            m_GraphView.schedule
                .Execute(() =>
                {
                    m_GraphView.ViewState.SetSelected(nodeUIs);
                    GraphUILayoutUtility.AlignSelectedNodesAndAllChildren(m_GraphView);
                    m_GraphView.Background.FrameElement(rootUI);
                    m_GraphView.Asset.SetAssetDirty();
                })
                .ExecuteLater(20);
        }

        private NodeUI GetNodeUI(NodeModel model)
        {
            return m_GraphView.ViewState.Nodes.FirstOrDefault(nodeUI => nodeUI.Model == model);
        }
    }
}
#endif