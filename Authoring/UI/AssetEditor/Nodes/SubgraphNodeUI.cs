#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(SubgraphNodeModel))]
    internal class SubgraphNodeUI : BehaviorNodeUI
    {
        private SubgraphNodeModel m_SubgraphNodeModel => Model as SubgraphNodeModel;

        private string m_SubgraphFieldName => SubgraphNodeModel.k_SubgraphFieldName;
        private readonly List<BaseLinkField> m_StoryFields = new ();
        private readonly BaseLinkField m_LinkedGraphField;
        private readonly VisualElement m_GraphSelectionLine;
        private readonly VisualElement m_StoryLine;

        public SubgraphNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/GraphViewStyles.uss"));
            AddToClassList("Action");
            AddToClassList("ShowNodeColor");

            VisualElement content = this.Q<VisualElement>("Content");
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/SubgraphNodeLayout.uxml").CloneTree(content);
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/SubgraphNodeStylesheet.uss"));
            AddToClassList("Behavior-SubgraphNodeUI");
            content.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);

            m_GraphSelectionLine = this.Q<VisualElement>("GraphSelectionLine");
            m_StoryLine = this.Q<VisualElement>("StoryLine");
            
            // The linked graph field holds a SerializableGUID, but there isn't a LinkField type for that, so use
            // a BaseLinkField for now.
            m_LinkedGraphField = new BaseLinkField { 
                FieldName = m_SubgraphFieldName, 
                LinkVariableType = typeof(BehaviorGraph),
                AllowAssetEmbeds = true
            };
            m_LinkedGraphField.Model = m_SubgraphNodeModel;
            m_LinkedGraphField.OnLinkChanged += _ =>
            {
                m_LinkedGraphField.Dispatcher.DispatchImmediate(new SetNodeVariableLinkCommand(nodeModel, m_LinkedGraphField.FieldName, m_LinkedGraphField.LinkVariableType, m_LinkedGraphField.LinkedVariable, true));
                m_SubgraphNodeModel.SubgraphField.LinkedVariable = m_LinkedGraphField.LinkedVariable;
                m_SubgraphNodeModel.OnValidate();
                m_SubgraphNodeModel.CacheRuntimeGraphId();
            };
            
            m_LinkedGraphField.RegisterCallback<LinkFieldTypeChangeEvent>(_ => UpdateFields());

            PopulateGraphSelectionLine();

            if (m_SubgraphNodeModel is { IsDynamic: false })
            {
                PopulateSubgraphStoryLine();   
            }
        }

        public override void Refresh(bool isDragging)
        {
            m_SubgraphNodeModel.ValidateCachedRuntimeGraph();
            base.Refresh(isDragging);
            UpdateFields();
        }

        private void PopulateGraphSelectionLine()
        {
            m_GraphSelectionLine.Clear();
            m_GraphSelectionLine.EnableInClassList("ShowStory", false);
            m_GraphSelectionLine.Add(new Label("Run"));
            m_GraphSelectionLine.Add(m_LinkedGraphField);
        }

        private void PopulateSubgraphStoryLine()
        {
            var oldFields = this.m_StoryFields.ToList();
            m_StoryFields.Clear();
            m_StoryLine.Clear();
            if (m_SubgraphNodeModel.SubgraphField.LinkedVariable == null || m_SubgraphNodeModel.SubgraphAuthoringAsset == null) // No graph asset assigned.
            {
                m_GraphSelectionLine.EnableInClassList("ShowStory", false);
                m_StoryLine.EnableInClassList("ShowStory", false);
                m_LinkedGraphField.FieldName = "Subgraph";
                return;
            }

            StoryInfo storyInfo = m_SubgraphNodeModel.SubgraphAuthoringAsset.Story;
            string story = storyInfo.Story;
            if (string.IsNullOrEmpty(story)) // Graph asset has no story.
            {
                m_GraphSelectionLine.EnableInClassList("ShowStory", false);
                m_StoryLine.EnableInClassList("ShowStory", false);
                return;
            }

            m_GraphSelectionLine.EnableInClassList("ShowStory", true);
            m_StoryLine.EnableInClassList("ShowStory", true);

            string[] storyWords = story.Split(" ");
            string currentLabelContents = "";
            for (int i = 0; i < storyWords.Length; ++i)
            {
                string word = storyWords[i];
                word = word.TrimStart('[');
                word = word.TrimEnd(']');
                Type variableType = storyInfo.Variables
                    .FirstOrDefault(variable => variable.Name.Equals(word, StringComparison.OrdinalIgnoreCase))?.Type;
                VariableModel variable = m_SubgraphNodeModel.SubgraphAuthoringAsset.Blackboard.Variables.FirstOrDefault(variable =>
                    variable.Name.Equals(word, StringComparison.OrdinalIgnoreCase) && variable.Type == variableType);
                if (variable == null) //ie a non-parameter word
                {
                    if (i != 0 && currentLabelContents.Length != 0)
                    {
                        currentLabelContents += " ";
                    }
                    currentLabelContents += word;
                    continue;
                }

                if (currentLabelContents.Length != 0)
                {
                    Label label = new Label(currentLabelContents);
                    label.AddToClassList("BTLabelExtraSpace");
                    m_StoryLine.Add(label);
                    currentLabelContents = string.Empty;
                }

                // If a story parameter field already exists, use it.
                BaseLinkField linkField = oldFields.FirstOrDefault(field =>
                    field.name == variable.Name && field.LinkVariableType == variable.Type);
                if (linkField == null)
                {
                    linkField = LinkFieldUtility.CreateNodeLinkField(variable.Name, variable!.Type);
                    linkField.FieldName = variable.Name;
                    linkField.Model = m_SubgraphNodeModel;
                    m_StoryFields.Add(linkField);
                }
                else
                {
                    oldFields.Remove(linkField);
                }
                
                m_StoryLine.Add(linkField);
            }
            
            m_StoryLine.Add(new Label(currentLabelContents));
            
            // Remove remaining old fields that haven't been reused.
            oldFields.ForEach(field => field.RemoveFromHierarchy());
        }

        private void UpdateFields()
        {
            TypedVariableModel<BehaviorGraph> linkedAssetVariable = m_LinkedGraphField.LinkedVariable as TypedVariableModel<BehaviorGraph>;

#if UNITY_EDITOR
            BehaviorAuthoringGraph graphAsset = BehaviorGraphAssetRegistry.TryGetAssetFromGraphPath(linkedAssetVariable?.m_Value);
#else
            BehaviorAuthoringGraph graphAsset = null;
#endif

            if (m_SubgraphNodeModel.SubgraphField != null)
            {
                m_LinkedGraphField.LinkedVariable = m_SubgraphNodeModel.SubgraphField.LinkedVariable;
            }
            if (m_LinkedGraphField.LinkedVariable == null)
            {
                m_LinkedGraphField.Q<Label>().text = SubgraphNodeModel.k_SubgraphFieldName;
            }

            if (graphAsset == null)
            {
                return;
            }
            
            BehaviorAuthoringGraph subgraphAsset = BehaviorGraphAssetRegistry.GlobalRegistry.Assets.FirstOrDefault(asset => asset.AssetID == graphAsset.AssetID);
            if (subgraphAsset.ContainsReferenceTo(m_SubgraphNodeModel.Asset as BehaviorAuthoringGraph))
            {
                Debug.LogWarning(
                    $"{subgraphAsset.name} contains a cyclic reference to {m_SubgraphNodeModel.Asset.name} and cannot be assigned to the subgraph node.");

                m_LinkedGraphField.LinkedVariable = null;
                ClearStoryLine();
                return;
            }

            // If there was an asset ID assigned, but no asset was found, warn the user and clear the link field.
            if (graphAsset.AssetID != default && subgraphAsset == null)
            {
                Debug.LogWarning($"Behavior graph asset with ID {graphAsset.AssetID} was not found. Link field will be cleared.", m_SubgraphNodeModel.Asset);
                m_LinkedGraphField.LinkedVariable = null; // This will unset the link field if it was previously set.
                m_LinkedGraphField.Q<Label>().text = SubgraphNodeModel.k_SubgraphFieldName;
            }

            if (!m_SubgraphNodeModel.IsDynamic)
            {
                if (m_SubgraphNodeModel.ShowStaticSubgraphRepresentation)
                {
                    PopulateSubgraphStoryLine();   
                }
                else
                {
                    m_StoryLine.Clear();
                    PopulateGraphSelectionLine();
                }
            }
        }
        
        private void ClearStoryLine()
        {
            // Clear serialized data for story fields
            m_SubgraphNodeModel.m_FieldValues.RemoveAll(field => field.FieldName != m_SubgraphFieldName || field.FieldName != SubgraphNodeModel.k_BlackboardFieldName);
            
            // Remove node UI
            foreach (BaseLinkField field in m_StoryFields)
            {
                field.RemoveFromHierarchy();
            }
            m_StoryFields.Clear();
            m_StoryLine.Clear();
        }
    }
}
#endif