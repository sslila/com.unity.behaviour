using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace UnityEngine.UIExtras
{
    internal class SearchWindow
    {
        static bool IsClosing = false;
        
        public static SearchView ShowInPopover(string title, List<SearchView.Item> items, Action<SearchView.Item> onSelection, float width, float height, VisualElement parent, bool showIcons = true, bool closeOnSelection = true, bool sortSearchItems = true)
        {
            IsClosing = false;
            
            SearchView searchView = new SearchView();
            searchView.Title = title;
            Popover popover = Popover.Build(parent, searchView);
            popover.SetArrowVisible(false);
            searchView.AutoSortItems = sortSearchItems;
            searchView.Items = items;
            searchView.OnSelection += (e) =>
            {
                if (IsClosing)
                {
                    return;
                }

                // If the item isn't enabled for selection, do nothing.
                if (!e.Enabled)
                {
                    return;
                }
                
                if (closeOnSelection)
                {
                    IsClosing = true;
                    popover.Dismiss();
                }
                
                onSelection?.Invoke(e);
                parent?.Focus();
            };
            searchView.style.width = width;
            searchView.style.height = height;
            searchView.ShowIcons = showIcons;
            
            popover.Show();
            popover.shown += (popover) =>
            {
                popover.view.schedule.Execute(searchView.FocusSearchField);
            };

            return searchView;
        }
        
        public static SearchView ShowAtPosition(string title, List<SearchView.Item> items, Action<SearchView.Item> onSelection, float x, float y, float width, float height, VisualElement parent, bool showIcons = true, bool sortSearchItems = true)
        {
            IsClosing = false;
            
            SearchView searchView = new SearchView();
            // Hide the SearchView element until the Popover has been shown, to avoid hierarchy visibility issues with the Popover element.
            searchView.Title = title;
            searchView.AutoSortItems = sortSearchItems;
            searchView.style.display = DisplayStyle.None;
            Popover popover = Popover.Build(parent, searchView);
            popover.SetArrowVisible(false);
            popover.SetShouldFlip(false);
            GraphUIUtility.PlacePopupAt(popover, new Vector2(x, y), width, height);

            // Return focus to parent after dismissing the popover.
            popover.dismissed += (popup, type) =>
            {
                parent?.Focus();
            };
            
            searchView.Items = items;
            searchView.OnSelection += (e) =>
            {
                if (IsClosing)
                {
                    return;
                }

                // If the item isn't enabled for selection, do nothing.
                if (!e.Enabled)
                {
                    return;
                }

                IsClosing = true;
                onSelection?.Invoke(e);
                popover.Dismiss();
                parent?.Focus();
            };
            searchView.style.width = width;
            searchView.style.height = height;
            searchView.ShowIcons = showIcons;
            popover.Show();
            popover.shown += (popoverElement) =>
            {
                // Display the SearchView element once the Popover has been shown.
                searchView.style.display = DisplayStyle.Flex;
                
                popoverElement.view.schedule.Execute(searchView.FocusSearchField);
            };
            return searchView;
        }


        public static SearchView Show(string title, List<SearchView.Item> items, Action<SearchView.Item> onSelection, VisualElement parent, float width, float height, bool showIcons = true, bool closeOnSelection = true)
        {
            Rect parentBounds = parent.worldBound;
            if (width == 0.0f)
            {
                width = parent.layout.width + parent.style.paddingLeft.value.value + parent.style.paddingRight.value.value;
            }
            SearchView searchView = ShowInPopover(title, items, onSelection, width, height, parent, showIcons, closeOnSelection);

            return searchView;
        }

        public static SearchView Show(SearchMenuBuilder builder)
        {
            Rect parentBounds = builder.Parent.worldBound;
            if (builder.Width == 0.0f)
            {
                builder.Width = builder.Parent.layout.width + builder.Parent.style.paddingLeft.value.value + builder.Parent.style.paddingRight.value.value;
            }
            if (builder.Height == 0.0f)
            {
                builder.Height = 200.0f;
            }
            List<SearchView.Item> searchItems = builder.Tabs.Count == 0 ? builder.Options : null;
            SearchView searchView = ShowInPopover(builder.Title, searchItems, builder.OnSelection, builder.Width, builder.Height, builder.Parent, builder.ShowIcons, builder.CloseOnSelection, sortSearchItems: builder.SortSearchItems);
            if (searchItems == null && builder.Options?.Count != 0)
            {
                searchView.SetTabItems(builder.DefaultTabName, builder.Options);
            }
            foreach (var tab in builder.Tabs)
            {
                searchView.SetTabItems(tab.Key, tab.Value);
            }

            return searchView;
        }

        public static SearchView ShowAtPosition(SearchMenuBuilder builder, float x, float y)
        {
            Rect parentBounds = builder.Parent.worldBound;
            if (builder.Width == 0.0f)
            {
                builder.Width = builder.Parent.layout.width + builder.Parent.style.paddingLeft.value.value + builder.Parent.style.paddingRight.value.value;
            }
            if (builder.Height == 0.0f)
            {
                builder.Height = 200.0f;
            }
            List<SearchView.Item> searchItems = builder.Tabs.Count == 0 ? builder.Options : null;
            SearchView searchView = ShowAtPosition(builder.Title, searchItems, builder.OnSelection, x, y, builder.Width, builder.Height, builder.Parent, builder.ShowIcons, builder.SortSearchItems);
            if (searchItems == null && builder.Options?.Count != 0)
            {
                searchView.SetTabItems(builder.DefaultTabName, builder.Options);
            }
            foreach (var tab in builder.Tabs)
            {
                searchView.SetTabItems(tab.Key, tab.Value);
            }

            return searchView;
        }
    }
}
