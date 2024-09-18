using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Unity.Behavior.GraphFramework
{
    internal class SearchMenuBuilder
    {
        public delegate void OnOptionSelected();

        List<SearchView.Item> m_Options = new List<SearchView.Item>();
        Dictionary<string, List<SearchView.Item>> m_Tabs = new Dictionary<string, List<SearchView.Item>>();

        public string Title { get; set; } = "Search";
        public float Width { get; set; } = 200.0f;
        public float Height { get; set; } = 300.0f;
        public Action<SearchView.Item> OnSelection { get; set; }
        public VisualElement Parent { get; set; }
        public bool ShowIcons { get; set; } = true;
        public bool CloseOnSelection { get; set; } = true;
        public bool SortSearchItems { get; set; } = true;


        public string DefaultTabName = "Default";
        public List<SearchView.Item> Options => m_Options;
        public Dictionary<string, List<SearchView.Item>> Tabs => m_Tabs;
        public bool HasTabs => m_Tabs?.Count > 0;

        public void Add(string path, OnOptionSelected onOptionSelected, Texture2D icon = null, string description = null, bool enabled = true, int priority = 0, string tab = "")
        {
            SearchView.Item searchItem = new SearchView.Item(path, icon, onOptionSelected, description, enabled, onSelected: null, priority);
            if (string.IsNullOrEmpty(tab))
            {
                m_Options.Add(searchItem);
                return;
            }
            if (!m_Tabs.TryGetValue(tab, out var tabItems))
            {
                tabItems = new List<SearchView.Item>();
                m_Tabs.Add(tab, tabItems);
            }
            tabItems.Add(searchItem);
        }

        public void Add(string path, Action onSelected = null, string iconName = null, string description = null, bool enabled = true, int priority = 0, string tab = "")
        {
            SearchView.Item searchItem = new SearchView.Item(path, iconName, onSelected: onSelected, description: description, enabled: enabled, priority: priority);
            if (string.IsNullOrEmpty(tab))
            {
                m_Options.Add(searchItem);
                return;
            }
            if (!m_Tabs.TryGetValue(tab, out var tabItems))
            {
                tabItems = new List<SearchView.Item>();
                m_Tabs.Add(tab, tabItems);
            }
            tabItems.Add(searchItem);
        }

        public void AddTab(string tabName)
        {
            if (m_Tabs.ContainsKey(tabName))
            {
                return;
            }
            m_Tabs.Add(tabName, new List<SearchView.Item>());
        }

        public void Show()
        {
            SearchWindow.Show(this);
        }

        public void ShowAtPosition(float x, float y)
        {
            SearchWindow.ShowAtPosition(this, x, y);
        }
    }

    internal class SearchMenuBuilderGeneric<Params> : SearchMenuBuilder
    {
        public delegate void OnOptionSelectedWithParams(Params @params);

        public void Add(string path, OnOptionSelectedWithParams onOptionSelected, Texture2D icon=null, string description=null, bool enabled=true, int priority = 0)
        {
            Options.Add(new SearchView.Item(path, icon, onOptionSelected, description, enabled, onSelected: null, priority));
        }
    }
}