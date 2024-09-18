#if UNITY_EDITOR
using System;
using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using ContextMenu = Unity.Behavior.GraphFramework.ContextMenu;

namespace Unity.Behavior
{
    internal class BehaviorGraphBlackboardView : BlackboardView
    {
        internal BehaviorAuthoringGraph GraphAsset;
        private VisualElement m_BlackboardAssetsContainer;

        internal BehaviorGraphBlackboardView(BlackboardMenuCreationCallback menuCreationCallback) : base(menuCreationCallback) { }
        
        internal override void InitializeListView()
        {
            UpdateListViewFromAsset(VariableListView, Asset, true);
            if (GraphAsset == null)
            {
                return;
            }

            m_BlackboardAssetsContainer?.Clear();
            if (GraphAsset.m_Blackboards.Count == 0)
            {
                return;
            }

            CreateBlackboardAssetsSection();
            
            // Initializing blackboards for each added Blackboard asset group.
            foreach (BehaviorBlackboardAuthoringAsset blackboardAsset in GraphAsset.m_Blackboards)
            {
                BlackboardAssetElement element = CreateAndGetBlackboardAssetElement(blackboardAsset);
                m_BlackboardAssetsContainer?.Add(element);
                UpdateListViewFromAsset(element.Variables, blackboardAsset, false);

                element.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.clickCount == 1 && evt.button == 1)
                    {
                        ContextMenu menu = new ContextMenu(this);
                        menu.AddItem("Delete Blackboard", () => Dispatcher.DispatchImmediate(new RemoveBlackboardAssetFromGraphCommand(GraphAsset, blackboardAsset, true)));
                        menu.Show();
                    }
                });
                
                // Register change callbacks.
                blackboardAsset.OnBlackboardChanged += InitializeListView;
                blackboardAsset.OnBlackboardDeleted += () => { Dispatcher.DispatchImmediate(new RemoveBlackboardAssetFromGraphCommand(GraphAsset, blackboardAsset, false)); };
            }
        }

        private BlackboardAssetElement CreateAndGetBlackboardAssetElement(BlackboardAsset asset)
        {
            BlackboardAssetElement element = new BlackboardAssetElement(asset);
            return element;
        }

        private void CreateBlackboardAssetsSection()
        {
            // Create the additional Blackboard Assets section with a divider.
            m_BlackboardAssetsContainer = new VisualElement();
            m_BlackboardAssetsContainer.name = "BlackboardAssetElementContainer";
            Divider divider = new Divider();
            divider.size = Size.S;
            m_BlackboardAssetsContainer.Add(divider);
            ViewContent.Add(m_BlackboardAssetsContainer);
        }
        
        protected override string GetBlackboardVariableTypeName(Type variableType)
        {
            if (variableType == typeof(BehaviorGraph))
            {
                return "Subgraph";
            }
            return BlackboardUtils.GetNameForType(variableType);
        }
    }
}
#endif