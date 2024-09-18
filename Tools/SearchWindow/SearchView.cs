using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace UnityEngine.UIExtras
{
    internal class SearchView : VisualElement
    {
        private const string k_Title = "Root";
        private string m_Title = k_Title;

        public readonly struct Item
        {
            public string Path { get; }
            public Texture2D Icon { get; }
            public string IconName { get; }
            public object Data { get; }
            public Action OnSelected { get; }
            public string Description { get; }
            public string Name => Path.Split('/').Last();
            public bool Enabled { get; }
            
            public int Priority { get; }

            public Item(string path, Texture2D icon = null, object data = null, string description = null, bool enabled = true, Action onSelected = null, int priority = 0)
            {
                Path = path;
                Icon = icon;
                IconName = null;
                Data = data;
                Description = description;
                Enabled = enabled;
                OnSelected = onSelected;
                Priority = priority;
            }

            public Item(string path, string iconName, object data = null, string description = null, bool enabled = true, Action onSelected = null, int priority = 0)
            {
                Path = path;
                Icon = null;
                IconName = iconName;
                Data = data;
                Description = description;
                Enabled = enabled;
                OnSelected = onSelected;
                Priority = priority;
            }
        }
        
        private readonly SearchBar m_SearchField;
        private readonly VisualElement m_ReturnButton;
        private readonly VisualElement m_ReturnIcon;
        readonly private ActionGroup m_ActionGroup;
        private readonly ListView m_ListView;
        public ListView ListView => m_ListView;

        private class TabInfo
        {
            public string Name { get; set; }
            public List<Item> Items { get; set; }
        }
        private readonly List<TabInfo> m_Tabs = new List<TabInfo>();        
        private List<Item> m_Items;
        private TreeNode<Item> m_RootNode;
        private TreeNode<Item> m_CurrentNode;
        private TreeNode<Item> m_SearchNode;
        private Stack<TreeNode<Item>> m_NavigationStack = new ();
        
        public List<Item> Items
        {
            get => m_Items;
            set
            {
                m_Items = value;
                if (m_Items == null)
                {
                    return;
                }
                m_RootNode = new TreeNode<Item>(new Item(m_Title));
                foreach (var item in m_Items)
                {
                    Add(item);
                }
                SortItems(m_RootNode);
                m_NavigationStack.Clear();
                SetCurrentSelectionNode(m_RootNode);                
            }
        }

        public void SetTabItems(string tabName, List<Item> items)
        {
            TabInfo tab = new TabInfo();
            tab.Name = tabName;
            tab.Items = items;
            m_Tabs.Add(tab);
            m_ActionGroup.Add(new ActionButton() { label = tab.Name });

            if (m_Tabs.Count == 1)
            {
                Items = items;
            }
            if (m_Tabs.Count == 2)
            {
                m_ActionGroup.style.display = DisplayStyle.Flex;
            }
        }

        public bool AutoSortItems {  get; set; }

        private void SortItems(TreeNode<Item> node)
        {
            if (node == null)
            {
                return;
            }

            node.Sort((TreeNode<Item> a, TreeNode<Item> b) =>
            {
                // First, check if the items can be sorted by given priority value. 
                int priorityComparison = b.Value.Priority.CompareTo(a.Value.Priority);
                if (priorityComparison != 0 || !AutoSortItems)
                {
                    return priorityComparison;
                }

                // Try sorting by item subcategory amount. 
                if (a.ChildCount != 0 && b.ChildCount == 0)
                {
                    return -1;
                }
                if (a.ChildCount == 0 && b.ChildCount != 0)
                {
                    return 1;
                }
                
                // Try sorting by item name.
                return a.Value.Name.CompareTo(b.Value.Name);
            });

            foreach (var child in node.Children)
            {
                SortItems(child);
            }
        }

        public string Title
        {
            get => m_Title;
            set
            {
                m_Title = value;
                if (m_RootNode != null)
                {
                    m_RootNode.Value = new Item(value);
                }

                m_ReturnButton.Q<Label>().text = m_CurrentNode == null ? value : m_CurrentNode.Value.Name;
            }
        }
        
        public bool ShowIcons
        {
            get => !ClassListContains("SearchView_NoIcons");
            set
            {
                if (value)
                {
                    RemoveFromClassList("SearchView_NoIcons");
                } 
                else if (!ClassListContains("SearchView_NoIcons"))
                {
                    AddToClassList("SearchView_NoIcons");
                }
            }
        }
        
        public event Action<Item> OnSelection;

        public SelectionType SelectionType 
        { 
            get => m_ListView.selectionType; 
            set => m_ListView.selectionType = value; 
        }
        
        public SearchView()
        {
            AddToClassList("SearchView");
#if UNITY_EDITOR
            AddToClassList(UnityEditor.EditorGUIUtility.isProSkin ? "UnityThemeDark" : "UnityThemeLight");
#else
            AddToClassList("UnityThemeDark");
#endif
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/SearchWindow/Assets/SearchViewStyle.uss"));
            VisualTreeAsset visualTree = ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/SearchWindow/Assets/SearchView.uxml");
            visualTree.CloneTree(this);

            m_SearchField = this.Q<SearchBar>();
            m_ReturnButton = this.Q<VisualElement>("ReturnButton");
            m_ReturnButton.Q<IconButton>().clicked += OnNavigationReturn;
            m_ReturnIcon = this.Q("ReturnIcon");

            m_ActionGroup = this.Q<ActionGroup>("SearchTabs");

            m_ListView = this.Q<ListView>("SearchResults");
            m_ListView.selectionType = SelectionType.Single;
            m_ListView.makeItem = () => new SearchViewItem();
            m_ListView.bindItem = (element, index) =>
            {
                if (element is not SearchViewItem searchViewItem)
                {
                    return;
                }
                searchViewItem.Item = m_CurrentNode[index];
                searchViewItem.RegisterCallback<PointerUpEvent>(HandlePointerSelection);
            };
            m_ListView.unbindItem = (element, i) =>
            {
                if (element is not SearchViewItem searchViewItem)
                {
                    return;
                }

                searchViewItem.UnregisterCallback<PointerUpEvent>(HandlePointerSelection);
            };
            
            m_ListView.RegisterCallback<FocusEvent>(evt =>
            {
                if (evt.currentTarget is not ListView lv)
                {
                    return;
                }

                if (!lv.selectedItems.Any())
                {
                    lv.SetSelection(0);
                }
            });
            
            RegisterCallback<PointerDownEvent>(evt => evt.StopImmediatePropagation());

            m_ActionGroup.style.display = DisplayStyle.None;
            m_ActionGroup.selectionChanged += OnTabSelectionChanged;


            RegisterCallback<KeyDownEvent>(HandleKeyboardDownInput, TrickleDown.TrickleDown);
            RegisterCallback<KeyUpEvent>(HandleKeyboardUpInput, TrickleDown.TrickleDown);
            RegisterCallback<PointerDownEvent>(evt => schedule.Execute(FocusSearchField), TrickleDown.TrickleDown);

            Title = k_Title;

            void ItemsChosenHandler(IEnumerable<object> items)
            {
                var itemsList = items.ToList();
                if (!itemsList.Any())
                {
                    return;
                }

                // we only consider single selection
                if (itemsList[0] is not TreeNode<Item> treeViewItem)
                {
                    return;
                }

                OnItemChosen(treeViewItem);
            }

#if UNITY_2022_2_OR_NEWER
            m_ListView.itemsChosen += ItemsChosenHandler;
#else 
            m_ListView.onItemsChosen += ItemsChosenHandler;
#endif

            NotifyValueChangingExtensions.RegisterValueChangingCallback(m_SearchField, OnSearchQueryChanged);
        }

        private void OnTabSelectionChanged(IEnumerable<int> enumerable)
        {
            int index = enumerable.FirstOrDefault();
            Items = m_Tabs[index].Items;
        }

        private void HandlePointerSelection(PointerUpEvent evt)
        {
            if (evt.currentTarget is not SearchViewItem svi)
            {
                return;
            }
            OnItemChosen(svi.Item);
        }

        private void HandleKeyboardDownInput(KeyDownEvent evt)
        {
            TreeNode<Item> selectedItem = m_ListView?.selectedItem as TreeNode<Item>;
            bool CanTraverseToChild = selectedItem?.ChildCount > 0 && string.IsNullOrEmpty(m_SearchField.value);
            bool CanTraverseToParent = m_CurrentNode != m_RootNode && string.IsNullOrEmpty(m_SearchField.value);
            switch (evt.keyCode)
            {
                case KeyCode.RightArrow when CanTraverseToChild:
                    OnItemChosen(selectedItem);
                    break;
                case KeyCode.Backspace when CanTraverseToParent:
                case KeyCode.LeftArrow when CanTraverseToParent:
                    OnNavigationReturn();
                    break;
                case KeyCode.DownArrow:
                    m_ListView.SetSelection(m_ListView.selectedIndex < (m_ListView.itemsSource.Count - 1) ? m_ListView.selectedIndex + 1 : 0);
                    m_ListView.ScrollToItem(m_ListView.selectedIndex);
                    break;
                case KeyCode.UpArrow:
                    m_ListView.SetSelection(m_ListView.selectedIndex > 0 ? m_ListView.selectedIndex - 1 : m_ListView.itemsSource.Count - 1);
                    m_ListView.ScrollToItem(m_ListView.selectedIndex);
                    break;
            }
        }

        private void HandleKeyboardUpInput(KeyUpEvent evt)
        {
            TreeNode<Item> selectedItem = m_ListView?.selectedItem as TreeNode<Item>;
            switch (evt.keyCode)
            {
                case KeyCode.Return:
                    if (selectedItem != null)
                    {
                        evt.StopImmediatePropagation();
                        OnItemChosen(selectedItem);
                    }
                    FocusSearchField();
                    break;
            }
        }

        public void FocusSearchField()
        {
            m_SearchField.Focus();
        }


        /// <summary>
        /// Checks whether the source string contains the search string, while ignoring case and spaces in both strings. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private static bool DoesSourceContainSearch(string source, string search)
        {
            // String interning will not cause GC.
            string sourceWithoutSpaces = source.Replace(" ", string.Empty);
            string toCheckWithoutSpaces = search.Replace(" ", string.Empty);

            /// Uses a character-by-character comparison, so it's efficient for longer strings.
            return sourceWithoutSpaces.IndexOf(toCheckWithoutSpaces, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        private void OnSearchQueryChanged(ChangingEvent<string> changeEvent)
        {
            string newValue = changeEvent.newValue;
            if (newValue == " ")
            {
                m_SearchField.SetValueWithoutNotify("");
                newValue = "";
            }

            if (string.IsNullOrEmpty(m_SearchField.value))
            {
                m_SearchField.placeholder = "Filter";
            } else
            {
                m_SearchField.placeholder = "";
            }

            bool isCurrentNodeSearchNode = m_SearchNode != null && m_CurrentNode == m_SearchNode;
            if (newValue.Length == 0)
            {
                if (isCurrentNodeSearchNode)
                {
                    OnNavigationReturn();
                }
                return;
            }

            if (!isCurrentNodeSearchNode)
            {
                m_NavigationStack.Push(m_CurrentNode);
            }
            List<TreeNode<Item>> searchResults = new List<TreeNode<Item>>();
            m_RootNode?.Traverse(delegate(TreeNode<Item> itemNode)
            {
                if (itemNode.Value.Name != null && DoesSourceContainSearch(itemNode.Value.Name, newValue))
                {
                    searchResults.Add(itemNode);
                }
            });
            searchResults.Remove(m_RootNode);
            m_SearchNode = new TreeNode<Item>(new Item("Search"));
            m_SearchNode.m_Children = searchResults;
            m_SearchNode.Parent = m_CurrentNode;
            SetCurrentSelectionNode(m_SearchNode);
        }

        private void OnItemChosen(TreeNode<Item> node)
        {
            if (node.ChildCount == 0)
            {
                OnSelection?.Invoke(node.Value);
            }
            else
            {
                m_NavigationStack.Push(m_CurrentNode);
                SetCurrentSelectionNode(node);
            }
        }

        private void SetCurrentSelectionNode(TreeNode<Item> node)
        {
            m_CurrentNode = node;
            m_ListView.itemsSource = m_CurrentNode.Children;
            m_ListView.Rebuild();
            m_ListView.ClearSelection();
            m_ReturnButton.Q<Label>().text = m_CurrentNode.Value.Name;
            if (m_NavigationStack.Count == 0)
            {
                m_ReturnButton.SetEnabled(false);
                m_ReturnIcon.style.visibility = Visibility.Hidden;
            }
            else
            {
                m_ReturnButton.SetEnabled(true);
                m_ReturnIcon.style.visibility = Visibility.Visible;
            }
        }

        private void OnNavigationReturn()
        {
            if (m_CurrentNode == null || m_NavigationStack.Count == 0)
            {
                return;
            }

            if (m_NavigationStack.TryPop(out TreeNode<Item> nodeToShow))
            {
                var previousNode = m_CurrentNode;
                if (m_CurrentNode == m_SearchNode)
                {
                    // If we've reached the root, reset the search field and remove the search node.
                    m_SearchField.SetValueWithoutNotify("");
                    m_SearchNode = null;
                }
                SetCurrentSelectionNode(nodeToShow);
                if (previousNode != null && previousNode.Parent != null && m_CurrentNode.Children.Contains(previousNode))
                {
                    m_ListView.SetSelection(m_CurrentNode.Children.IndexOf(previousNode));
                    m_ListView.ScrollToItem(m_ListView.selectedIndex);
                }
            }
        }

        private void Add(Item item)
        {
            if (item.Path.Length == 0)
            {
                return;
            }
            
            string[] pathParts = item.Path.Split('/');
            TreeNode<Item> treeNodeParent = m_RootNode;
            string currentPath = string.Empty;
            for (int i = 0; i < pathParts.Length; ++i)
            {
                if (currentPath.Length == 0)
                {
                    currentPath += pathParts[i];
                }
                else
                {
                    currentPath += "/" + pathParts[i];
                }

                TreeNode<Item> node = (i < pathParts.Length - 1) ? FindNodeByPath(treeNodeParent, currentPath) : null;
                if (node == null)
                {
                    node = treeNodeParent.AddChild(new Item(currentPath));
                }
                if (i == (pathParts.Length - 1))
                {
                    node.Value = item;
                }
                else
                {
                    treeNodeParent = node;
                }
            }
        }

        private static TreeNode<Item> FindNodeByPath(TreeNode<Item> treeNodeParent, string path)
        {
            if (treeNodeParent == null || path.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < treeNodeParent.ChildCount; ++i)
            {
                if (treeNodeParent[i].Value.Path.Equals(path))
                {
                    return treeNodeParent[i];
                }
            }
            return null;
        }
    }
}