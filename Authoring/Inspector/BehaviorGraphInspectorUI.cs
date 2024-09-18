#if UNITY_EDITOR
using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.Behavior
{
    internal class BehaviorGraphInspectorUI : VisualElement
    {
        internal readonly ActionButton EditSubgraphStoryButton;

        private const string k_GraphSubtitle = "Behavior Graph";
        private readonly BehaviorAuthoringGraph m_InspectedGraph;
        
        public BehaviorGraphInspectorUI(BehaviorAuthoringGraph graph)
        {
            m_InspectedGraph = graph;
            AddToClassList("NodeInspectorUI");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/Inspector/Assets/BehaviorInspectorStyleSheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/Inspector/Assets/BehaviorGraphInspectorLayout.uxml").CloneTree(this);
            
            Label titleLabel = this.Q<Label>("Info-Name");
            Label infoDescriptionLabel = this.Q<Label>("Info-Description");
            Label subtitleLabel = this.Q<Label>("Subtitle");
            EditSubgraphStoryButton = this.Q<ActionButton>("EditSubgraphStoryButton");
            TextField graphDescriptionField = this.Q<TextField>("GraphDescription-Field");

            titleLabel.text = m_InspectedGraph.name;
            subtitleLabel.text = k_GraphSubtitle.ToUpper();
            infoDescriptionLabel.text = "This graph can be used in other graphs. Edit how it represents itself in other graphs below.";
            graphDescriptionField.RegisterValueChangingCallback(OnDescriptionChanged);
            if (!string.IsNullOrEmpty(m_InspectedGraph.Description))
            {
                graphDescriptionField.value = m_InspectedGraph.Description;   
            }
        }

        private void OnDescriptionChanged(ChangingEvent<string> evt)
        {
            m_InspectedGraph.Description = evt.newValue;
        }
    }
}
#endif