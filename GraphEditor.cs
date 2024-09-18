#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Unity.Behavior.GraphFramework
{
    // [UxmlElement]
    internal partial class GraphEditor : VisualElement, IDispatcherContext
    {
        public GraphAsset Asset { get; private set; }
        public BlackboardView Blackboard { get; }
        public InspectorView Inspector { get; }

        BlackboardView IDispatcherContext.BlackboardView => Blackboard;

        GraphEditor IDispatcherContext.GraphEditor => this;

        public GraphView GraphView { get; }

        GraphAsset IDispatcherContext.GraphAsset => Asset;

        BlackboardAsset IDispatcherContext.BlackboardAsset => Asset.Blackboard;

        VisualElement IDispatcherContext.Root => this;

        public Dispatcher Dispatcher { get; }

        private readonly GraphToolbar m_Toolbar;

        private const string k_DefaultLayoutFile = "Packages/com.unity.behavior/Tools/Graph/Assets/GraphEditorLayout.uxml";
        private const string k_DefaultStylesheetFile = "Packages/com.unity.behavior/Tools/Graph/Assets/GraphEditorStylesheet.uss";

        /// <summary>
        /// Default constructor used by the UXML Serializer.
        /// </summary>
        public GraphEditor()
           : this(k_DefaultLayoutFile, k_DefaultStylesheetFile)
        {
            focusable = true;
        }

        public GraphEditor(string layoutfile = k_DefaultLayoutFile, string stylesheetFile = k_DefaultStylesheetFile)
        {
            focusable = true;

            AddToClassList("GraphEditor");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>(stylesheetFile));
            VisualTreeAsset visualTree = ResourceLoadAPI.Load<VisualTreeAsset>(layoutfile);
            visualTree.CloneTree(this);
            
            m_Toolbar = this.Q<GraphToolbar>();
            Dispatcher = new Dispatcher(this);
            Blackboard = CreateBlackboardView();
            GraphView = GetOrCreateGraphView();
            Inspector = CreateNodeInspector();
            if (Inspector != null)
            {
                Inspector.GraphEditor = this;
            }            
            
            if (GraphView.parent == null)
            {
                this.Q("EditorPanel")?.Add(GraphView);
            }
            
            Blackboard.Dispatcher = Dispatcher;
            GraphView.Dispatcher = Dispatcher;

            RegisterCommandHandlers();

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            schedule.Execute(Update);
        }

        protected virtual void Update()
        {
            if (!Asset)
            {
                return;
            }
            
            Dispatcher.Tick();
            if (Asset.HasOutstandingChanges)
            {
                Blackboard.RefreshFromAsset();
                GraphView.RefreshFromAsset();
                Inspector?.Refresh();
                Asset.HasOutstandingChanges = false;
            }
            schedule.Execute(Update);
        }

        public virtual void Load(GraphAsset asset)
        {
            Asset = asset;
            if (asset)
            {
                asset.OnValidate();
            }
            Blackboard.Load(Asset.Blackboard);
            GraphView.Load(asset);
            
            m_Toolbar.AssetTitle.text = Asset.name;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.S && evt.modifiers is EventModifiers.Control or EventModifiers.Command)
            {
                OnAssetSave();
                evt.StopImmediatePropagation();
            }
        }

        public virtual void OnAssetSave()
        {
            Asset.SaveAsset();
        }

        public virtual bool IsAssetVersionUpToDate()
        {
            return true;
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            if (panel.contextType == ContextType.Player)
            {
                styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/GraphRuntimeStylesheet.uss"));
            }
#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed += OnUndoRedoPerformed;
#endif
            // Create Blackboard and Inspector panels.
            ToggleBlackboard(true);
            ToggleNodeInspector(true);

            // Add graph icon stylesheet for the App UI panel.
            if (GetFirstAncestorOfType<Panel>() != null)
            {
                GetFirstAncestorOfType<Panel>().styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/GraphIconStylesheet.uss"));
            }
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed -= OnUndoRedoPerformed;
#endif
        }

        protected virtual void OnUndoRedoPerformed()
        {
            GraphView.IsPerformingUndo = true;
            Load(Asset);
            GraphView.IsPerformingUndo = false;
            Asset.SetAssetDirty();
            IsAssetVersionUpToDate();
        }

        protected virtual void RegisterCommandHandlers()
        {
            Dispatcher.RegisterHandler<DeleteNodeCommand, DeleteNodeCommandHandler>();
            Dispatcher.RegisterHandler<CreateNodeCommand, CreateNodeCommandHandler>();
            Dispatcher.RegisterHandler<DuplicateNodeCommand, DuplicateNodeCommandHandler>();
            Dispatcher.RegisterHandler<CopyNodeCommand, CopyNodeCommandHandler>();
            Dispatcher.RegisterHandler<PasteNodeCommand, PasteNodeCommandHandler>();
            Dispatcher.RegisterHandler<MoveNodesCommand, MoveNodesCommandHandler>();

            Dispatcher.RegisterHandler<ConnectEdgeCommand, ConnectEdgeCommandHandler>();
            Dispatcher.RegisterHandler<ConnectEdgesCommand, ConnectEdgesCommandHandler>();

            Dispatcher.RegisterHandler<DeleteNodeCommand, DeleteNodeCommandHandler>();
            Dispatcher.RegisterHandler<DeleteEdgeCommand, DeleteEdgeCommandHandler>();
            Dispatcher.RegisterHandler<DeleteNodesAndEdgesCommand, DeleteNodesAndEdgesCommandHandler>();

            Dispatcher.RegisterHandler<AddNodesToSequenceCommand, AddNodesToSequenceCommandHandler>();
            Dispatcher.RegisterHandler<CreateNewSequenceOnDropCommand, CreateNewSequenceOnDropCommandHandler>();

            Dispatcher.RegisterHandler<CreateVariableCommand, CreateVariableCommandHandler>();
            Dispatcher.RegisterHandler<RenameVariableCommand, RenameVariableCommandHandler>();
            Dispatcher.RegisterHandler<DeleteVariableCommand, DeleteVariableCommandHandler>();
        }

        public virtual SearchMenuBuilder CreateBlackboardOptions()
        {
            SearchMenuBuilder builder = new SearchMenuBuilder();
            
            void CreateVariableFromMenuAction(string variableTypeName, Type type)
            {
                // if (type == typeof(int))
                // {
                //     Dispatcher.DispatchImmediate(new CreateVariableCommand($"New {variableTypeName}", typeof(TypedVariableModelInt)));
                //     return;
                // }
                Dispatcher.DispatchImmediate(new CreateVariableCommand($"New {variableTypeName}", BlackboardUtils.GetVariableModelTypeForType(type)));
            }

            builder.Add("Object", iconName: "object", onSelected: delegate { CreateVariableFromMenuAction("Object", typeof(GameObject)); });
            builder.Add("String", iconName: "string", onSelected: delegate { CreateVariableFromMenuAction("String", typeof(string)); });
            builder.Add("Float", iconName: "float", onSelected: delegate { CreateVariableFromMenuAction("Float", typeof(float)); });
            builder.Add("Integer", iconName: "integer", onSelected: delegate { CreateVariableFromMenuAction("Integer", typeof(int)); });
            builder.Add("Double", iconName: "double", onSelected: delegate { CreateVariableFromMenuAction("Double", typeof(double)); });
            builder.Add("Boolean", iconName: "boolean", onSelected: delegate { CreateVariableFromMenuAction("Boolean", typeof(bool)); });
            builder.Add("Vector2", iconName: "vector2", onSelected: delegate { CreateVariableFromMenuAction("Vector2", typeof(Vector2)); });
            builder.Add("Vector3", iconName: "vector3", onSelected: delegate { CreateVariableFromMenuAction("Vector3", typeof(Vector3)); });
            builder.Add("Vector4", iconName: "vector4", onSelected: delegate { CreateVariableFromMenuAction("Vector4", typeof(Vector4)); });
            builder.Add("Color", iconName: "color", onSelected: delegate { CreateVariableFromMenuAction("Color", typeof(Color)); });

            builder.Add("List/Object", iconName: "object", onSelected: delegate { CreateVariableFromMenuAction("Object List", typeof(List<GameObject>)); });
            builder.Add("List/String", iconName: "string", onSelected: delegate { CreateVariableFromMenuAction("String List", typeof(List<string>)); });
            builder.Add("List/Float", iconName: "float", onSelected: delegate { CreateVariableFromMenuAction("Float List", typeof(List<float>)); });
            builder.Add("List/Integer", iconName: "integer", onSelected: delegate { CreateVariableFromMenuAction("Integer List", typeof(List<int>)); });
            builder.Add("List/Double", iconName: "double", onSelected: delegate { CreateVariableFromMenuAction("Double List", typeof(List<double>)); });
            builder.Add("List/Boolean", iconName: "boolean", onSelected: delegate { CreateVariableFromMenuAction("Boolean List", typeof(List<bool>)); });
            builder.Add("List/Vector2", iconName: "vector2", onSelected: delegate { CreateVariableFromMenuAction("Vector2 List", typeof(List<Vector2>)); });
            builder.Add("List/Vector3", iconName: "vector3", onSelected: delegate { CreateVariableFromMenuAction("Vector3 List", typeof(List<Vector3>)); });
            builder.Add("List/Vector4", iconName: "vector4", onSelected: delegate { CreateVariableFromMenuAction("Vector4 List", typeof(List<Vector4>)); });
            builder.Add("List/Color", iconName: "color", onSelected: delegate { CreateVariableFromMenuAction("Color List", typeof(List<Color>)); });

            return builder;
        }

        protected virtual GraphView GetOrCreateGraphView() => new GraphView();

        protected virtual BlackboardView CreateBlackboardView()
        {
            return new BlackboardView(CreateBlackboardOptions);
        }

        protected virtual InspectorView CreateNodeInspector()  
        {
            return new InspectorView();
        }
        
        private void ToggleBlackboard(bool displayValue)
        {
            VisualElement editorPanel = this.Q<VisualElement>("EditorPanel");
            if (displayValue)
            {
                if (editorPanel.Q<FloatingPanel>("Blackboard") == null)
                {
                    FloatingPanel blackboardPanel = FloatingPanel.Create(Blackboard, GraphView, "Blackboard");
                    blackboardPanel.IsCollapsable = true;
                    editorPanel.Add(blackboardPanel);
                }   
            }
            else
            {
                if (editorPanel.Q<FloatingPanel>("Blackboard") != null)
                {
                    editorPanel.Q<FloatingPanel>("Blackboard").Remove();
                }
            }
        }

        private void ToggleNodeInspector(bool displayValue)
        {
            VisualElement editorPanel = this.Q<VisualElement>("EditorPanel");
            if (displayValue)
            {
                if (editorPanel.Q<FloatingPanel>("Inspector") == null)
                {
                    FloatingPanel nodeInspectorPanel = FloatingPanel.Create(Inspector, GraphView, "Inspector", FloatingPanel.DefaultPosition.TopRight);
                    nodeInspectorPanel.IsCollapsable = true;
                    editorPanel.Add(nodeInspectorPanel);
                }   
            }
            else
            {
                if (editorPanel.Q<FloatingPanel>("Inspector") != null)
                {
                    editorPanel.Q<FloatingPanel>("Inspector").Remove();
                }
            }
        }
    }
}
#endif