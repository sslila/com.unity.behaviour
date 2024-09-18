#if UNITY_EDITOR

using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class SubGraphStoryEditor : StoryEditor
    {
        private BehaviorAuthoringGraph m_Asset;
        
        public SubGraphStoryEditor()
        {
            name = "StoryEditor";
            RefreshPropertiesUI();

            this.Q<HelpText>("StoryInfoHelpText").Text = "Edit this graphs appearance as a subgraph. Variables will map to ones on the blackboard, or create them if needed.";
          
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            SetAssetLink(m_Asset);
            RefreshPropertiesUI();
        }

        internal void SetAssetLink(BehaviorAuthoringGraph asset)
        {
            StoryInfo storyInfo = asset.Story;
            m_Asset = asset;
            StoryField.Value = storyInfo.Story;
            
            // Update sentence with types from storyInfo
            Sentence.WordTypePairs.Clear();
            foreach (VariableInfo varInfo in storyInfo.Variables)
            {
                Sentence.AddWordType(varInfo.Name, varInfo.Type);
            }
            Sentence.UpdateWordTypeList(storyInfo.Story.Length - 1, storyInfo.Story);
            
            RefreshPropertiesUI();
        }
    }
}
#endif