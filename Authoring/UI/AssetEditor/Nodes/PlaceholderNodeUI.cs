#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [NodeUI(typeof(PlaceholderNodeModel))]
    internal class PlaceholderNodeUI : BehaviorNodeUI
    {
        public PlaceholderNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Placeholder");
            PlaceholderNodeModel placeholderNodeModel = nodeModel as PlaceholderNodeModel;
            switch (placeholderNodeModel.PlaceholderType)
            {
                case PlaceholderNodeModel.PlaceholderNodeType.Action:
                    AddToClassList("Action");
                    break;
                case PlaceholderNodeModel.PlaceholderNodeType.Modifier:
                    AddToClassList("Modifier");
                    break;
                case PlaceholderNodeModel.PlaceholderNodeType.Composite:
                    AddToClassList("Composite");
                    break;
            }

            if (placeholderNodeModel.PlaceholderType != PlaceholderNodeModel.PlaceholderNodeType.Action && !String.IsNullOrEmpty(placeholderNodeModel.Story))
            {
                AddToClassList("TwoLineNode");
            }

            Title = placeholderNodeModel.Name;
            tooltip = placeholderNodeModel.Name;

            NodeValueContainer.Add(CreatePlaceholderNodeContent(this, placeholderNodeModel.Story, placeholderNodeModel.Variables));
        }

        private VisualElement CreatePlaceholderNodeContent(BehaviorNodeUI nodeUI, string story, List<VariableInfo> variables)
        {
            var container = new VisualElement();
            container.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/LinkFieldStyles.uss"));
            container.AddToClassList("PlaceholderNodeContent");

            var storyContainer = new VisualElement();
            storyContainer.AddToClassList("PlaceholderStoryContainer");
            string[] words = story.Split(' ');
            if (words?.Length > 0)
            {
                container.Add(storyContainer);
            }
            for (int i = 0; i < words.Length; ++i)
            {
                string word = words[i];
                word = word.TrimStart('[');
                word = word.TrimEnd(']');
                var foundMatch = variables != null && variables.Any(variable =>
                    string.Equals(word, variable.Name, StringComparison.OrdinalIgnoreCase));
                if (foundMatch)
                {
                    var label = new Label(word);
                    label.AddToClassList("Linked");
                    label.AddToClassList("LinkedLabel");
                    storyContainer.Add(label);
                }
                else
                {
                    Label label = new Label(word);
                    storyContainer.Add(label);
                }
            }

#if UNITY_EDITOR
            // Add an appui button to create the action
            var button = new Unity.AppUI.UI.Button();
            button.title = "Create";
            container.Add(button);
            button.clickable.clicked += () =>
            {
                var graphView = container.GetFirstAncestorOfType<BehaviorGraphView>();
                if (nodeUI.Model is PlaceholderNodeModel placeholderNodeModel)
                {
                    if (placeholderNodeModel.PlaceholderType == PlaceholderNodeModel.PlaceholderNodeType.Action)
                    {
                        graphView?.ShowActionNodeWizard(nodeUI.Position, placeholderNodeModel);
                    }
                    else if (placeholderNodeModel.PlaceholderType == PlaceholderNodeModel.PlaceholderNodeType.Modifier)
                    {
                        graphView?.ShowModifierNodeWizard(nodeUI.Position, placeholderNodeModel);
                    }
                    else if (placeholderNodeModel.PlaceholderType == PlaceholderNodeModel.PlaceholderNodeType.Composite)
                    {
                        graphView?.ShowSequencingNodeWizard(nodeUI.Position, placeholderNodeModel);
                    }
                }
            };
#endif
            return container;
        }
    }
}
#endif