#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.UIExtras;
using Button = Unity.AppUI.UI.Button;
using ContextMenu = Unity.Behavior.GraphFramework.ContextMenu;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.Behavior
{
    internal class ConditionInspectorElement : VisualElement
    {
        private Button m_AssignButton;
        private ListView m_ConditionListView;
        private readonly IConditionalNodeModel m_NodeModel;

        public ConditionInspectorElement(IConditionalNodeModel nodeModel)
        {
            m_NodeModel = nodeModel;
            if (m_NodeModel != null)
            {
                CreateElement();   
            }
        }

        private void CreateElement()
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/Inspector/Assets/ConditionInspectorElementStylesheet.uss"));
            
            Add(CreateTruncateOptionField());
            
            Divider divider = new Divider();
            divider.size = Size.S;
            Add(divider);
            
            m_AssignButton = new Button();
            m_AssignButton.name = "AssignConditionButton";
            m_AssignButton.title = "Assign Condition";
            Add(m_AssignButton);
            m_AssignButton.clicked += OnAssignButtonClicked;

            m_ConditionListView = new ListView();
            m_ConditionListView.itemsSource = m_NodeModel.ConditionModels;
            m_ConditionListView.makeItem = () => new VisualElement();
            m_ConditionListView.selectionType = SelectionType.None;
            m_ConditionListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            m_ConditionListView.bindItem = (element, i) =>
            {
                element.Clear();
                element.Add(CreateConditionFieldElement(i));
            };
            Add(m_ConditionListView);
        }

        private VisualElement CreateTruncateOptionField()
        {
            VisualElement truncateOptionElement = new VisualElement();
            truncateOptionElement.name = "TruncateOptionField";
            truncateOptionElement.AddToClassList("ToggleOptionField");
            truncateOptionElement.Add(new Label("Truncate Node"));
            truncateOptionElement.tooltip = "Collapse the node to a compact size if multiple conditions are added";
            Toggle toggle = new Toggle();
            truncateOptionElement.Add(toggle);
            
            toggle.value = m_NodeModel.ShouldTruncateNodeUI;
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_NodeModel.ShouldTruncateNodeUI = evt.newValue;
                BehaviorGraphNodeModel node = m_NodeModel as BehaviorGraphNodeModel;
                node?.Asset.SetAssetDirty();
            });
            return truncateOptionElement;
        }

        private VisualElement CreateConditionFieldElement(int index)
        {
            VisualElement conditionContainerElement = new VisualElement();
            conditionContainerElement.name = "ConditionContainerElement";
            ConditionModel condition = m_NodeModel.ConditionModels[index];
            NodeConditionElement element = new NodeConditionElement(m_NodeModel.ConditionModels[index]);
            element.RegisterCallback<GeometryChangedEvent>(ApplyWrapClassOnChildElements);
            element.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.clickCount == 1 && evt.button == 1)
                {
                    ContextMenu menu = new ContextMenu(this);
                    menu.AddItem("Delete", () =>
                    {
                        GraphEditor editor = GetFirstAncestorOfType<GraphEditor>();
                        editor.Dispatcher.DispatchImmediate(new RemoveConditionFromNodeCommand(m_NodeModel, condition, true));
                        m_ConditionListView.RefreshItems();
                    });
#if UNITY_EDITOR
                    menu.AddItem("Edit Script", () =>
                    {
                        // Open the script found through the condition info path.
                        ConditionInfo info = ConditionUtility.GetInfoForConditionType(m_NodeModel.ConditionModels[index].ConditionType);
                        if (string.IsNullOrEmpty(info.FilePath))
                        {
                            Debug.LogWarning($"File path to the script asset of {info.Name} can not be found.");
                        }
                        else
                        {
                            string path = info.FilePath.Replace("\\", "/");
                            string relativePath = path.StartsWith(Application.dataPath)
                                ? "Assets" + path.Substring(Application.dataPath.Length)
                                : path;
                            CodeEditor.CodeEditor.Editor.CurrentCodeEditor.OpenProject(relativePath);   
                        }
                    });
#endif
                    menu.Show();
                }
            });
            element.AddToClassList("ConditionElement");
            conditionContainerElement.Add(element);

            return conditionContainerElement;
        }

        private void OnAssignButtonClicked()
        {
            List<SearchView.Item> searchItems = new List<SearchView.Item>();
            List<Condition> conditionList = ConditionUtility.GetConditions();
            searchItems.Add(new SearchView.Item(path: "Create new Condition...", data: null, priority: 1));

            foreach (Condition condition in conditionList)
            {
                ConditionInfo info = ConditionUtility.GetInfoForConditionType(condition.GetType());
                searchItems.Add(new SearchView.Item(
                    path: info.Path,
                    data: condition ));
            }
            
            SearchWindow.Show("Add Condition", searchItems, OnConditionSelected, m_AssignButton,  200, 244, false);
        }

        private void OnConditionSelected(SearchView.Item obj)
        {
            if (obj.Data == null)
            {
                ShowConditionWizard();
            }
            else
            {
                GraphEditor editor = GetFirstAncestorOfType<GraphEditor>();
                editor.Dispatcher.DispatchImmediate(new AddConditionToNodeCommand(m_NodeModel, obj.Data as Condition, true));
                m_ConditionListView.RefreshItems();
            }
        }
        
        private void ShowConditionWizard()
        {
#if UNITY_EDITOR
            GraphEditor editor = GetFirstAncestorOfType<GraphEditor>();
            BehaviorGraphView view = editor.Q<BehaviorGraphView>();
            BehaviorGraphNodeModel model = m_NodeModel as BehaviorGraphNodeModel;
            
            // Only this nees to use the new command, all the old stays the same as it was..
            ConditionWizardWindow.GetAndShowWindow(view, Util.GetVariableSuggestions(model?.Asset), DeferAddConditionToNode);
#endif
        }
        
        private void DeferAddConditionToNode(string conditionType)
        {
            BehaviorGraphNodeModel node = m_NodeModel as BehaviorGraphNodeModel;
            (node?.Asset as BehaviorAuthoringGraph).CommandBuffer.SerializeDeferredCommand(new AddConditionFromSerializedCommand(node.ID, conditionType, true)); 
            m_ConditionListView.RefreshItems();
        }
        
        private void ApplyWrapClassOnChildElements(GeometryChangedEvent evt)
        {
            if (evt.target is not VisualElement container)
            {
                return;
            }
            
            float containerWidth = container.resolvedStyle.width;
            float currentRowWidth = 0;
            VisualElement previousChild = null;

            for (int i = 0; i < container.childCount; i++)
            {
                VisualElement child = container[i];
                float itemWidth = child.resolvedStyle.width + child.resolvedStyle.marginRight;

                // Check if the visual element fits in the current row
                if (currentRowWidth + itemWidth > containerWidth)
                {
                    child.AddToClassList("Wrapped");
                }
                else
                {
                    child.RemoveFromClassList("Wrapped");
                }

                if (previousChild != null)
                {
                    currentRowWidth += itemWidth + previousChild.resolvedStyle.marginRight;
                }
                else
                {
                    currentRowWidth += itemWidth;
                }
                previousChild = child;
            }
        }
    }
}
#endif