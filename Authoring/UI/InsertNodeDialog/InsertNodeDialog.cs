#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Behavior
{
    internal class InsertNodeDialog : VisualElement
    {
        public Dispatcher Dispatcher { get; internal set; }
        internal string Title { get => m_Dialog.title; set => m_Dialog.title = value; }
        internal string Description { get => m_DescriptionLabel.text; set => m_DescriptionLabel.text = value; }
        
        private readonly Label m_DescriptionLabel;
        private readonly ScrollView m_ScrollView;
        private Button m_ConfirmButton;
        
        private readonly List<NodeOptionElement> m_MenuOptionElements = new ();
        private NodeInfo m_SelectedOptionNodeInfo;
        private List<NodeInfo> m_NodeOptions = new ();
        private Modal m_Modal;
        private Dialog m_Dialog;
        
        internal List<NodeInfo> NodeOptions
        {
            get => m_NodeOptions;
            set
            {
                m_NodeOptions = value;
                RebuildOptions();
            }
        }

        internal Tuple<PortModel, PortModel> ConnectionToBreak { get; set; }
        internal List<PortModel> ConnectedOutputPorts { get; set; } = new List<PortModel>();
        internal List<PortModel> ConnectedInputPorts { get; set; } = new List<PortModel>();

        public InsertNodeDialog()
        {
            AddToClassList("InsertNodeDialog");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/InsertNodeDialog/Assets/InsertNodeDialogStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/InsertNodeDialog/Assets/InsertNodeDialogLayout.uxml").CloneTree(this);
            
            m_DescriptionLabel = this.Q<Label>("Description");
            m_ScrollView = this.Q<ScrollView>("Content");
        }

        internal static InsertNodeDialog GetAndShowDialog(VisualElement targetView)
        {
            InsertNodeDialog insertNodeDialog = new InsertNodeDialog();
            insertNodeDialog.m_Dialog = new Dialog();
            insertNodeDialog.m_Dialog.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/InsertNodeDialog/Assets/BehaviorDialogStylesheet.uss"));
            insertNodeDialog.m_Dialog.dismissable = true;
            insertNodeDialog.m_Dialog.closeButton.quiet = true;
            insertNodeDialog.m_Dialog.contentContainer.Add(insertNodeDialog);
            insertNodeDialog.CreateDialogButtons();
            
            insertNodeDialog.m_Modal = Modal.Build(targetView, insertNodeDialog.m_Dialog);
            insertNodeDialog.m_Modal.Show();

            return insertNodeDialog;
        }

        private void CreateDialogButtons()
        {
            m_ConfirmButton = new Button();
            m_ConfirmButton.title = "Confirm";
            VisualElement buttonGroup = m_Dialog.Q<VisualElement>("appui-dialog__buttongroup");
            buttonGroup.Add(m_ConfirmButton);
            m_ConfirmButton.SetEnabled(false);
            
            m_ConfirmButton.clicked += OnConfirm;
        }
        
        private void OnConfirm()
        {
            m_Modal.Dismiss();

            Vector2 position = (ConnectionToBreak.Item1.NodeModel.Position + ConnectionToBreak.Item2.NodeModel.Position)/2;
            Dispatcher.Dispatch(new InsertNodeCommand(m_SelectedOptionNodeInfo, position, ConnectionToBreak, ConnectedOutputPorts, ConnectedInputPorts));
        }
        
        private void RebuildOptions()
        {
            m_ConfirmButton.SetEnabled(false);
            m_SelectedOptionNodeInfo = null;

            m_ScrollView.Clear();
            m_MenuOptionElements.Clear();
            foreach (NodeInfo nodeInfo in m_NodeOptions)
            {
                NodeOptionElement nodeOptionElement = new NodeOptionElement(nodeInfo);
                m_ScrollView.Add(nodeOptionElement);
                m_MenuOptionElements.Add(nodeOptionElement);
                nodeOptionElement.RegisterCallback<PointerDownEvent>(OnOptionPointerDown);
            }
        }

        private void OnOptionPointerDown(PointerDownEvent evt)
        {
            foreach (NodeOptionElement nodeOptionElement in m_MenuOptionElements)
            {
                nodeOptionElement.RemoveFromClassList("Selected");
            }
            NodeOptionElement nodeOption = evt.target as NodeOptionElement;
            nodeOption.AddToClassList("Selected");
            m_SelectedOptionNodeInfo = nodeOption.userData as NodeInfo;
            m_ConfirmButton.SetEnabled(true);
        }

        class NodeOptionElement : VisualElement
        {
            internal NodeOptionElement(NodeInfo nodeInfo)
            {
                userData = nodeInfo;
                AddToClassList("NodeOptionElement");

                Label name = new Label(nodeInfo.Name);
                name.name = "Node-Name";
                name.pickingMode = PickingMode.Ignore;
                Add(name);

                Label description = new Label(nodeInfo.Description);
                description.name = "Node-Description";
                description.pickingMode = PickingMode.Ignore;
                Add(description);
            }
        }
    }
}
#endif