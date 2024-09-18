using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class ContextMenu
    {
        public delegate void OnMenuCallback();

        private VisualElement m_Target;

        public ContextMenu(VisualElement target)
        {
            m_Target = target;
        }

#if UNITY_EDITOR
        // Implementation for editor based applications.

        private UnityEditor.GenericMenu m_Menu = new UnityEditor.GenericMenu();

        private void AddItemEditor(string text, OnMenuCallback callback)
        {
            m_Menu.AddItem(new GUIContent(text), false, () => { callback(); });
        }

        private void AddItemCheckmarkedEditor(string text, OnMenuCallback callback)
        {
            m_Menu.AddItem(new GUIContent(text), true, () => { callback(); });
        }
        
        private void AddItemDisabledEditor(string text)
        {
            m_Menu.AddDisabledItem(new GUIContent(text));
        }

        private void AddSeparatorEditor()
        {
            m_Menu.AddSeparator("");
        }

        private void ShowEditor()
        {
            m_Menu.ShowAsContext();
        }
#endif

        Vector2 MousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && USE_NEW_INPUT_SYSTEM
                Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
#else
                Vector2 mousePos = Input.mousePosition;
#endif
                mousePos.y = Screen.height - mousePos.y;
                Vector2 pos = Vector2.zero;
                if (m_Target.panel.contextType == ContextType.Player)
                {
                    pos = RuntimePanelUtils.ScreenToPanel(m_Target.panel, mousePos);
                }

                return pos;
            }
        }

        Popup<ListView> m_Popup;
        private readonly List<(string entry, OnMenuCallback callback)> m_MenuData = new ();

        public void AddItemCheckmarked(string text, OnMenuCallback callback)
        {
#if UNITY_EDITOR
            if (m_Target.panel.contextType == ContextType.Editor)
            {
                AddItemCheckmarkedEditor(text, callback);
                return;
            }
#endif
            m_MenuData.Add((text, callback));
        }

        public void AddItem(string text, OnMenuCallback callback)
        {
#if UNITY_EDITOR
            if (m_Target.panel.contextType == ContextType.Editor)
            {
                AddItemEditor(text, callback);
                return;
            }
#endif
            m_MenuData.Add((text, callback));
        }
        
        public void AddDisabledItem(string text)
        {
#if UNITY_EDITOR
            if (m_Target.panel.contextType == ContextType.Editor)
            {
                AddItemDisabledEditor(text);
                return;
            }
#endif
            m_MenuData.Add((text, null));
        }

        public void AddSeparator()
        {
#if UNITY_EDITOR
            if (m_Target.panel.contextType == ContextType.Editor)
            {
                AddSeparatorEditor();
                return;
            }
#endif
        }

        public void Show()
        {
#if UNITY_EDITOR
            if (m_Target.panel.contextType == ContextType.Editor)
            {
                ShowEditor();
                return;
            }
#endif
            m_Popup = Popup<ListView>.Show(m_Target, MousePosition);
            m_Popup.Root.fixedItemHeight = 20;
            m_Popup.Root.itemsSource = m_MenuData;
            m_Popup.Root.makeItem = () => new Label();
            m_Popup.Root.bindItem = (element, index) =>
            {
                if (element is not Label label)
                {
                    return;
                    
                }
                label.text = m_MenuData[index].entry;
                label.RegisterCallback<MouseDownEvent>(HandleMouseSelection);
                label.userData = m_MenuData[index].callback;
            };
            m_Popup.Root.unbindItem = (element, index) =>
            {
                element.UnregisterCallback<MouseDownEvent>(HandleMouseSelection);
                element.userData = null;
            };
            
            m_Popup.Root.RefreshItems();
            m_Popup.Root.style.backgroundColor = new Color(0.2470588f, 0.2470588f, 0.2470588f);
            m_Popup.Root.style.fontSize = 12;
            m_Popup.Root.style.unityTextAlign = TextAnchor.MiddleLeft;
            
#if UNITY_2022_2_OR_NEWER
            m_Popup.Root.itemsChosen += OnItemsChosen;
#else
            m_Popup.Root.onItemsChosen += OnItemsChosen;
#endif
        }

        private void HandleMouseSelection(MouseDownEvent evt)
        {
            if (evt.currentTarget is not Label label)
            {
                return;
            }

            if (label.userData is not OnMenuCallback callback)
            {
                return;
            }
            
            callback?.Invoke();
            evt.StopPropagation();
            m_Popup.Close();
        } 

        private void OnItemsChosen(IEnumerable<object> obj)
        {
            int index = m_Popup.Root.selectedIndex;
            if (index < m_MenuData.Count)
            {
                m_MenuData[index].callback?.Invoke();
            }

            m_Popup.Close();
        }
    }
}
