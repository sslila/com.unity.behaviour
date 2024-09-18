#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Behavior.GenerativeAI;
using Unity.AppUI.UI;

namespace Unity.Behavior
{
    internal class ContextMenuManipulator : GraphFramework.ContextMenuManipulator
    {
        BehaviorGraphView Target => target as BehaviorGraphView;
        Vector2 MousePos { get; set; }
        GraphElement ClickedElement { get; set; }

        internal ContextMenuManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent);
        }

        protected override void OnPointerUpEvent(PointerUpEvent evt)
        {
            if (CanStartManipulation(evt))
            {
                ClickedElement = null;
                MousePos = evt.position;

                // Editor ContextMenu element.
                GraphFramework.ContextMenu menu = new GraphFramework.ContextMenu(Target);
                // Runtime Menu element.
                MenuBuilder menuBuilder = MenuBuilder.Build(Target);
                menuBuilder.SetShouldFlip(false);

                NodeUI clickedNode = Target.NodeAt(MousePos);
                Edge clickedEdge = Target.EdgeAt(MousePos);

                if (clickedNode == null && clickedEdge == null)
                {
                    // Default editor context menu.
                    if (Target.panel.contextType == ContextType.Editor)
                    {
                        menu.AddItem("Add...", () => Target.ShowNodeSearch(MousePos));
                        menu.AddSeparator();
#if UNITY_EDITOR
                        Vector2 localMousePos = Target.WorldPosToLocal(MousePos);
                        menu.AddItem("Create new/Action", () => Target.ShowActionNodeWizard(localMousePos));
                        menu.AddItem("Create new/Modifier", () => Target.ShowModifierNodeWizard(localMousePos));
                        menu.AddItem("Create new/Sequencing", () => Target.ShowSequencingNodeWizard(localMousePos));
#if ENABLE_MUSE_BEHAVIOR
                        if (!IsBranchWidgetOpen(Target))
                        {
                            menu.AddItem("Generate branch from text", () => OnGenerateBranchFromText(Target, MousePos));   
                        }
                        menu.AddSeparator();
#endif
#endif
                    }

                    // Default runtime-only context menu items.
                    if (Target.panel.contextType == ContextType.Player)
                    {
                        menuBuilder.AddAction(0, "Add...", null, _ => Target.ShowNodeSearch(MousePos));
                    }
                }

                bool addSeparator = false;
                if (clickedNode != null && clickedNode.Model.IsDuplicatable)
                {
                    // Editor Duplicate and Copy menu items.
                    if (Target.panel.contextType == ContextType.Editor)
                    {
                        menu.AddItem("Duplicate", OnDuplicate);
                        menu.AddItem("Copy", OnCopy);
                        addSeparator = true;
                    }

                    // Runtime Duplicate and Copy menu items.
                    if (Target.panel.contextType == ContextType.Player)
                    {
                        menuBuilder.AddAction(0, "Duplicate", null, _ => OnDuplicate());
                        menuBuilder.AddAction(0, "Copy", null, _ => OnCopy());
                    }
                }

                if (GUIUtility.systemCopyBuffer != null)
                {
                    var nodes = GetNodesFromClipboard();
                    if (nodes != null)
                    {
                        // Editor Paste menu item.
                        if (Target.panel.contextType == ContextType.Editor)
                        {
                            menu.AddItem("Paste", OnPaste);
                            addSeparator = true;
                        }

                        // Runtime Paste menu item.
                        if (Target.panel.contextType == ContextType.Player)
                        {
                            menuBuilder.AddAction(0, "Paste", null, _ => OnPaste());
                        }
                    }
                }

                if (addSeparator)
                {
                    menu.AddSeparator();
                }

                if (clickedNode != null || clickedEdge != null || Target.ViewState.Selected.Any())
                {
                    ClickedElement = clickedNode != null ? clickedNode : clickedEdge;
                    bool showDelete = (ClickedElement == null  && Target.ViewState.Selected.Any()) || ClickedElement.IsDeletable;
                    if (showDelete)
                    {
                        // Editor Delete menu item.
                        if (Target.panel.contextType == ContextType.Editor)
                        {
                            menu.AddItem("Delete", OnDelete);
                        }

                        // Runtime Delete menu item.
                        if (Target.panel.contextType == ContextType.Player)
                        {
                            menuBuilder.AddAction(0, "Delete", null, _ => OnDelete());
                        }
                    }
                }

                if (clickedNode != null)
                {
#if UNITY_EDITOR
                    if (clickedNode.Model is BehaviorGraphNodeModel nodeModel)
                    {
                        if (target.panel.contextType == ContextType.Editor && nodeModel is not PlaceholderNodeModel)
                        {
                            menu.AddSeparator();
                            NodeInfo info = NodeRegistry.GetInfoFromTypeID(nodeModel.NodeTypeID);
                            if (!Util.IsNodeInPackageRuntimeAssembly(info))
                            {
                                menu.AddItem("Edit Definition", OnEditNode);
                                menu.AddItem("Edit Script", OnEditScript);    
                            }
                            else
                            {
                                menu.AddDisabledItem("Edit Definition");
                                menu.AddItem("Inspect Script", OnEditScript);
                            }
                        }

                        menu.AddSeparator();
                        BehaviorAuthoringGraph asset = nodeModel.Asset as BehaviorAuthoringGraph;
                        if (asset.DebugInfo != null && asset.DebugInfo.IsNodeBreakpointEnabled(nodeModel.ID))
                        {
                            menu.AddItemCheckmarked("Toggle Breakpoint", OnToggleBreakpoint);
                        }
                        else
                        {
                            menu.AddItem("Toggle Breakpoint", OnToggleBreakpoint);
                        }
                    }
#endif
                    // Editor Align menu items.
                    if (target.panel.contextType == ContextType.Editor)
                    {
                        menu.AddSeparator();
                        menu.AddItem("Align/Immediate Children", OnAlignChildNodes);
                        menu.AddItem("Align/All Children", OnAlignSubgraph);
                    }

                    // Runtime Align menu items.
                    if (Target.panel.contextType == ContextType.Player)
                    {
                        menuBuilder.PushSubMenu(0, "Align", null);
                        menuBuilder.AddAction(0, "Immediate Children", null, _ => OnAlignChildNodes());
                        menuBuilder.AddAction(0, "All Children", null, _ => OnAlignSubgraph());
                    }
                }

                // Show editor menu.
                if (Target.panel.contextType == ContextType.Editor)
                {
                    menu.Show();
                }
            
                // Show runtime menu.
                if (Target.panel.contextType == ContextType.Player)
                {
                    GraphUIUtility.PlacePopupAt(menuBuilder, MousePos, menuBuilder.currentMenu.resolvedStyle.width, menuBuilder.currentMenu.resolvedStyle.height);
                    menuBuilder.Show();
                }
            }
        }

        private List<NodeModel> GetNodesFromClipboard()
        {
            var jsonString = GUIUtility.systemCopyBuffer;
            try
            {
                NodeCopyData copyData = new NodeCopyData();
                JsonUtility.FromJsonOverwrite(jsonString, copyData);
                return copyData.Nodes;
            }
            catch
            {
                return null;
            }
        }

        private void OnAlignChildNodes()
        {
            if (Target.ViewState.Selected.Contains(ClickedElement))
            {
                // Align all selected nodes.
                GraphUILayoutUtility.AlignSelectedNodesImmediateChildren(Target);
            }
            else
            {
                // Align from the clicked node only. Check if the nodeUI is in a sequence. If so, align from the sequence.
                NodeUI rootUI = ClickedElement as NodeUI;
                Group sequenceUI = rootUI!.GetFirstAncestorOfType<Group>();
                Target.Asset.MarkUndo("Align Child Nodes");
                var nodePositions = GraphUILayoutUtility.ComputeChildNodePositions(sequenceUI ?? rootUI);
                GraphUILayoutUtility.ScheduleNodeMovement(Target, nodePositions);
            }
        }

        private void OnAlignSubgraph()
        {
            if (Target.ViewState.Selected.Contains(ClickedElement))
            {
                // Align all selected nodes.
                GraphUILayoutUtility.AlignSelectedNodesAndAllChildren(Target);
            }
            else
            {
                // Align from the clicked node only. Check if the nodeUI is in a sequence. If so, align from the sequence.
                NodeUI rootUI = ClickedElement as NodeUI;
                Group sequenceUI = rootUI!.GetFirstAncestorOfType<Group>();
                Target.Asset.MarkUndo("Align Subgraph");
                var nodePositions = GraphUILayoutUtility.ComputeSubgraphNodePositions(sequenceUI ?? rootUI);
                GraphUILayoutUtility.ScheduleNodeMovement(Target, nodePositions);
            }
        }

        private void OnEditScript()
        {
#if UNITY_EDITOR
            if (ClickedElement is not NodeUI node)
            {
                return;
            }

            NodeModel model = node.Model;
            if (model is BehaviorGraphNodeModel aiModel)
            {
                string path = NodeRegistry.GetInfo(aiModel.NodeType).FilePath.Replace("\\", "/");
                string relativePath = path.StartsWith(Application.dataPath)
                    ? ("Assets" + path.Substring(Application.dataPath.Length))
                    : path;
                CodeEditor.CodeEditor.Editor.CurrentCodeEditor.OpenProject(relativePath);
            }
#endif
        }

        private void OnEditNode()
        {
#if UNITY_EDITOR
            if (ClickedElement is not NodeUI node)
            {
                return;
            }

            Dictionary<string, Type> variableSuggestions = Util.GetVariableSuggestions(Target.Asset);

            if (node.Model is ActionNodeModel actionNodeModel)
            {
                NodeInfo info = NodeRegistry.GetInfoFromTypeID(actionNodeModel.NodeTypeID);
                ActionNodeWizardWindow.GetAndShowEditWindow(Target, info, variableSuggestions);
            }
            else if (node.Model is ModifierNodeModel modifierNodeModel)
            {
                NodeInfo info = NodeRegistry.GetInfoFromTypeID(modifierNodeModel.NodeTypeID);
                ModifierNodeWizardWindow.GetAndShowEditWindow(Target, info, variableSuggestions);
            }
            else if (node.Model is CompositeNodeModel sequencingNodeModel)
            {
                NodeInfo info = NodeRegistry.GetInfoFromTypeID(sequencingNodeModel.NodeTypeID);
                SequencingNodeWizardWindow.GetAndShowEditWindow(Target, info, node.Model, variableSuggestions);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnToggleBreakpoint()
        {
            if (ClickedElement is not NodeUI nodeUI)
                return;

            BehaviorGraphNodeModel nodeModel = nodeUI.Model as BehaviorGraphNodeModel;
            BehaviorAuthoringGraph asset = nodeModel.Asset as BehaviorAuthoringGraph;
            asset.DebugInfo.ToggleNodeBreakpoint(nodeModel.ID);
            nodeUI.ToggleInClassList("BreakpointEnabled");

            if (asset.DebugInfo.IsNodeBreakpointEnabled(nodeModel.ID) && !System.Diagnostics.Debugger.IsAttached)
            {
                // There is no debugger attached. Inform the user they should attach the debugger.
                EditorUtility.DisplayDialog("Code Debugger Not Attached",
                    "You currently don't have a debugger attached. For the breakpoints to work, please attach a debugger using your code IDE (Visual Studio, Rider, etc).",
                    "OK", DialogOptOutDecisionType.ForThisSession, "Behavior.Breakpoint.DebuggerNotAttachedWarning");
            }
        }
#endif

        private void OnDuplicate()
        {
            var duplicatableNodes = Target.ViewState.Selected.OfType<NodeUI>()
                .Where(nodeUI => nodeUI.Model.IsDuplicatable).ToList();
            if (ClickedElement == null || duplicatableNodes.Contains(ClickedElement))
            {
                Target.Dispatcher.DispatchImmediate(
                    new DuplicateNodeCommand(duplicatableNodes.Select(nodeUI => nodeUI.Model), Target.WorldPosToLocal(MousePos)));
            }
            else if (ClickedElement is NodeUI { Model: { IsDuplicatable: true } } nodeUI)
            {
                Target.Dispatcher.DispatchImmediate(new DuplicateNodeCommand(nodeUI.Model, Target.WorldPosToLocal(MousePos)));
            }
        }

        private void OnCopy()
        {
            var duplicatableNodes = Target.ViewState.Selected.OfType<NodeUI>()
                .Where(nodeUI => nodeUI.Model.IsDuplicatable).ToList();

            if (ClickedElement == null || duplicatableNodes.Contains(ClickedElement))
            {
                Target.Dispatcher.DispatchImmediate(
                    new CopyNodeCommand(duplicatableNodes.Select(nodeUI => nodeUI.Model)));
            }
            else if (ClickedElement is NodeUI { Model: { IsDuplicatable: true } } nodeUI)
            {
                Target.Dispatcher.DispatchImmediate(new CopyNodeCommand(nodeUI.Model));
            }
        }

        private void OnPaste()
        {
            List<NodeModel> nodes = GetNodesFromClipboard();
            Target.Dispatcher.DispatchImmediate(new PasteNodeCommand(nodes, Target.WorldPosToLocal(MousePos)));
        }

        private void OnDelete()
        {
            if (ClickedElement == null || Target.ViewState.Selected.Contains(ClickedElement))
            {
                DeleteSelected();
            }
            else
            {
                if (ClickedElement is Edge edgeUI)
                {
                    NodeUI start = edgeUI.Start.GetFirstAncestorOfType<NodeUI>();
                    NodeUI end = edgeUI.End.GetFirstAncestorOfType<NodeUI>();
                    PortModel startPort = start.Model.FindPortModelByName(edgeUI.Start.name);
                    PortModel endPort = end.Model.FindPortModelByName(edgeUI.End.name);

                    Target.Dispatcher.DispatchImmediate(new DeleteEdgeCommand(startPort, endPort, false));
                }
                else if (ClickedElement is NodeUI node && node.Model != null && node.IsDeletable)
                {
                    Target.Dispatcher.DispatchImmediate(new DeleteNodeCommand(node.Model, false));
                }
            }

            ClickedElement = null;
        }

        void DeleteSelected()
        {
            // The code below is duplicated in DeleteManipulator.OnKeyDown()
            List<GraphElement> notDeleted = new List<GraphElement>();
            List<Tuple<PortModel, PortModel>> edgesToDelete = new();
            List<NodeModel> nodesToDelete = new();
            foreach (GraphElement element in Target.ViewState.Selected)
            {
                if (element is Edge edge && edge.IsDeletable)
                {
                    edgesToDelete.Add(new Tuple<PortModel, PortModel>(edge.Start.PortModel, edge.End.PortModel));
                }
                else if (element is NodeUI nodeUI && nodeUI.IsDeletable)
                {
                    nodesToDelete.Add(nodeUI.Model);
                }
                else
                {
                    notDeleted.Add(element);
                }
            }

            Target.ViewState.SetSelected(notDeleted);
            Target.Dispatcher.Dispatch(new DeleteNodesAndEdgesCommand(edgesToDelete, nodesToDelete, markUndo: true));
        }

#if ENABLE_MUSE_BEHAVIOR
        void OnGenerateBranchFromText(BehaviorGraphView graphView, Vector2 mousePos)
        {
#if UNITY_EDITOR
            if (!MuseBehaviorUtilities.IsSessionUsable)
            {
                MuseBehaviorUtilities.OpenMuseDropdown();
                return;
            }
#endif
            mousePos.y -= 100f;
            var worldPos = graphView.WorldPosToLocal(mousePos);
            var widget = new BranchGenerationWidget(graphView, worldPos);
            widget.name = "BranchGenerationWidget";
            Modal modal = Modal.Build(graphView, widget);
            widget.CloseButton.clicked += () => modal.Dismiss();
            widget.OnBranchGenerated += () => modal.Dismiss();
            modal.Show();
        }

        bool IsBranchWidgetOpen(BehaviorGraphView graphView)
        {
            var graphEditor = graphView.GetFirstAncestorOfType<BehaviorGraphEditor>();
            if (graphEditor == null)
                return false;

            return graphEditor.Q<VisualElement>("EditorPanel")?.Q("BranchGenerationWidget") != null;
        }
#endif
    }
}
#endif