#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Unity.Behavior.GraphFramework
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement("Blackboard")]
#endif
    internal partial class BlackboardView : VisualElement
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<BlackboardView, UxmlTraits>
        {
            public override string uxmlName => "Blackboard";
        }
#endif

        public Dispatcher Dispatcher { get; internal set; }
        public BlackboardAsset Asset { get; private set; }

        internal bool IsVisible = true;

        private IconButton m_AddButton;
        internal IconButton AddButton
        {
            set
            {
                m_AddButton = value;
                m_AddButton.clicked += OnAddClicked;
            }
        }

        protected readonly VisualElement ViewContent;
        protected readonly ListView VariableListView;

        public delegate SearchMenuBuilder BlackboardMenuCreationCallback();
        internal event BlackboardMenuCreationCallback OnOnBlackboardMenuCreation;

        // Variable information cache used to check for expensive list view update.
        private readonly List<Tuple<string, Type>> m_VariableCache = new List<Tuple<string, Type>>();
        
        public BlackboardView(){}

        public BlackboardView(BlackboardMenuCreationCallback blackboardMenuCreationCallback)
        {
            OnOnBlackboardMenuCreation = blackboardMenuCreationCallback;
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Blackboard/Assets/BlackboardStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Blackboard/Assets/BlackboardLayout.uxml").CloneTree(this);
            ViewContent = this.Q<VisualElement>("BlackboardViewContent");
            VariableListView = this.Q<ListView>("Variables");

            VariableListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
            VariableListView.RegisterCallback<BlurEvent>(OnBlurEvent);

#if UNITY_2023_2_OR_NEWER
            SetupDragAndDropArgs();
#endif
        }

#if UNITY_2023_2_OR_NEWER
        private void SetupDragAndDropArgs()
        {
            VariableListView.canStartDrag += CanStartDragAndDrop;
            VariableListView.setupDragAndDrop += SetupDragAndDrop;
        }

        private StartDragArgs SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            var draggedBlackboardElement = args.draggedElement?.Q<BlackboardVariableElement>();
            if (draggedBlackboardElement == null)
            {
                return args.startDragArgs;
            }
            var startDragArgs = new StartDragArgs(args.startDragArgs.title, DragVisualMode.Move);
            startDragArgs.SetGenericData("VariableModel", draggedBlackboardElement.VariableModel);
            return startDragArgs;
        }

        private bool CanStartDragAndDrop(CanStartDragArgs args)
        {
            return args.draggedElement?.Q<BlackboardVariableElement>() != null;
        }
#endif

        private void OnAttachToPanelEvent(AttachToPanelEvent evt)
        {
            if (!IsInFloatingPanel(out FloatingPanel floatingPanel))
            {
                return;
            }
            
            floatingPanel.Title = "Blackboard";
            
            if (m_AddButton == null)
            {
                m_AddButton = new IconButton();
                m_AddButton.name = "BlackboardAddButton";
                m_AddButton.icon = "plus";
                m_AddButton.quiet = true;
                AppBar appBar = floatingPanel.Q<AppBar>();
                appBar.Add(m_AddButton);
                
                m_AddButton.clicked += OnAddClicked;
            }
        }

        internal void Load(BlackboardAsset asset)
        {
            Asset = asset;
            if (asset == null)
            {
                VariableListView.Clear();
                return;
            }

            // Check for removed variables, which can occur on undo.
            GraphEditor editor = GetFirstAncestorOfType<GraphEditor>();
            VariableListView.Query<BlackboardVariableElement>()
                .Where(elem => asset.Variables.All(variable => variable.ID != elem.VariableModel.ID))
                .ForEach(elem => editor.SendEvent(VariableDeletedEvent.GetPooled(editor, elem.VariableModel)));
            VariableListView.Query<BlackboardVariableElement>()
                .ForEach(elem =>
                {
                    VariableModel matchingVariableInAsset = asset.Variables.FirstOrDefault(variableModel =>
                        variableModel.ID == elem.VariableModel.ID);
                    if (matchingVariableInAsset != null && matchingVariableInAsset.Name != elem.VariableModel.Name)
                    {
                        editor.SendEvent(VariableRenamedEvent.GetPooled(editor, matchingVariableInAsset));
                    }
                });

            UpdateVariableCache();
            InitializeListView();
        }

        internal virtual void InitializeListView()
        {
            UpdateListViewFromAsset(VariableListView, Asset, true);
        }

        protected void UpdateListViewFromAsset(ListView listView, BlackboardAsset asset, bool isEditable)
        {
            listView.makeItem = () => new VisualElement();
            listView.bindItem = delegate(VisualElement element, int i)
            {
                element.Clear();
                if (i >= asset.Variables.Count)
                {
                    return;
                }
                VariableModel variable = asset.Variables[i];
                Type variableUIType = NodeRegistry.GetVariableUIType(variable.GetType());
                BlackboardVariableElement variableUI = variableUIType == null ? new BlackboardVariableElement(this, variable) :
                    Activator.CreateInstance(variableUIType, this, variable) as BlackboardVariableElement;
                variableUI!.IsEditable = isEditable;
                variableUI!.OnNameChanged += (name, variable) =>
                {
                    RenameVariable(variable, name);
                    // Selection is set here as a placeholder, should later be replaced by a better solution
                    int index = Asset.Variables.IndexOf(variable);
                    listView.SetSelection(index);
                };
                // variableUI.IconImage = variable.Type.GetIcon();
                variableUI.VariableType = GetBlackboardVariableTypeName(variable.Type);
                if (variable.ID != BlackboardVariableElement.k_ReservedID)
                {
                    variableUI.tooltip = GetBlackboardVariableTypeName(variable.Type) + " variable";
                }
                variableUI.RegisterCallback<PointerDownEvent>(OnPointerDown);

                element.Add(variableUI);
            };
            listView.itemsSource = asset.Variables;
            listView.Rebuild();
        }

        protected virtual string GetBlackboardVariableTypeName(Type variableType)
        {
            return BlackboardUtils.GetNameForType(variableType);
        }

        private void UpdateVariableCache()
        {
            m_VariableCache.Clear();
            if (Asset != null)
            {
                m_VariableCache.AddRange(Asset.Variables.Select(variable =>
                    new Tuple<string, Type>(variable.Name, variable.Type)));
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.clickCount == 1 && evt.button == 1)
            {
                BlackboardVariableElement element = evt.currentTarget as BlackboardVariableElement;
                int index = Asset.Variables.IndexOf(element!.VariableModel);
                VariableListView.SetSelection(index);
            }
        }

        private bool IsInFloatingPanel(out FloatingPanel floatingPanel)
        {
            floatingPanel = GetFirstAncestorOfType<FloatingPanel>();
            return floatingPanel != null;
        }

        public void FocusOnVariableNameField(VariableModel variable)
        {
            if (!IsVisible || panel == null)
            {
                return;
            }
            if (IsInFloatingPanel(out FloatingPanel floatingPanel))
            {
                floatingPanel.ExpandPanel();
            }
            VariableListView.RefreshItems();
            int index = Asset.Variables.IndexOf(variable);
            VariableListView.schedule.Execute(() =>
            {
                VariableListView.ScrollToItem(index);
                var listElementForIndex = VariableListView.GetRootElementForIndex(index);
                if (listElementForIndex == null)
                {
                    return;
                }
                BlackboardVariableElement blackboardVariableElement = listElementForIndex.Q<BlackboardVariableElement>();
                if (blackboardVariableElement == null)
                {
                    return;
                }
                blackboardVariableElement.Expand();
                blackboardVariableElement.ToggleTitleFields();
            }).ExecuteLater(50);
        }

        protected internal virtual void RefreshFromAsset()
        {
            // As refreshing the variable list incurs a performance hit, we should only refresh it when necessary.
            if (!m_VariableCache.All(variableInfo => Asset.Variables.Any(variable => 
                    variable.Name == variableInfo.Item1 && variable.Type == variableInfo.Item2)))
            {
                VariableListView.RefreshItems();
                UpdateVariableCache();
            }
        }
        
        private void RenameVariable(VariableModel variable, string name)
        {
            Dispatcher.DispatchImmediate(new RenameVariableCommand(variable, name));
        }

        internal void DeleteVariable(VariableModel variable)
        {
            Dispatcher.DispatchImmediate(new DeleteVariableCommand(variable));
        }

        private void OnAddClicked()
        {
            if (IsInFloatingPanel(out FloatingPanel floatingPanel))
            {
                floatingPanel.ExpandPanel();
            }
            
            SearchMenuBuilder builder = CreateBlackboardMenu();
            floatingPanel?.PreventCollapsingThisFrame();
            builder.Show();
        }

        private SearchMenuBuilder CreateBlackboardMenu()
        {
            SearchMenuBuilder builder = OnOnBlackboardMenuCreation?.Invoke();
            if (builder == null)
            {
                return new SearchMenuBuilder();
            }
            builder.Title = "Add Variable";
            builder.OnSelection = OnCreateVariableFromMenu;
            builder.Width = 260;
            builder.Height = 400;
            builder.Parent = m_AddButton;
            builder.ShowIcons = false;
            builder.SortSearchItems = true;
            return builder;
        }

        private void OnCreateVariableFromMenu(SearchView.Item item)
        {
            item.OnSelected?.Invoke();
        }

        private void OnBlurEvent(BlurEvent evt)
        {
            VariableListView.ClearSelection();
        }
    }
}
#endif