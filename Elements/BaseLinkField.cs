#if UNITY_EDITOR

using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    /// <summary>
    /// Base class for all UI link fields.
    /// </summary>
    public class BaseLinkField : ExVisualElement
    {
        internal Dispatcher Dispatcher => m_GraphEditor.Dispatcher;
        private Type m_LinkVariableType = typeof(object);

        internal Type LinkVariableType
        {
            get => m_LinkVariableType;
            set => m_LinkVariableType = value;
        }
        internal bool AllowAssetEmbeds { get; set; }

        private string m_LinkedLabelPrefix;
        internal string LinkedLabelPrefix
        {
            get => m_LinkedLabelPrefix;
            set
            {
                m_LinkedLabelPrefix = value;
                if (m_LinkedVariable != null)
                {
                    SetLinkVisuals(m_LinkedVariable);   
                }
            }
        }

        private readonly Icon m_LinkButton;
        private readonly Icon m_UnlinkButton;
        
        /// <summary>
        /// Label describing the link field.
        /// </summary>
        protected readonly Label LinkedLabel;

        private readonly VisualElement m_FieldIcon;

        private GraphEditor m_GraphEditor;
        private VisualElement m_LinkFieldContainer;
        private readonly VisualElement m_LinkedLabelContainer;
        private VariableModel m_LinkedVariable;
        private VisualElement m_FieldContainer;
        private Label m_NameLabel;
        
        /// <summary>
        /// Field container element.
        /// </summary>
        protected VisualElement FieldContainer => m_FieldContainer;

        private BaseModel m_Model;

        internal BaseModel Model
        {
            get => m_Model;
            set
            {
                m_Model = value;
                if (m_Model != null)
                {
                    UpdateValue(Model.GetVariableLink(FieldName, LinkVariableType));
                }
            }
        }

        internal string Label
        {
            get => m_NameLabel.text;
            set => m_NameLabel.text = value;
        }

        internal delegate void LinkChangedCallback(VariableModel newValue);
        internal event LinkChangedCallback OnLinkChanged;

        internal string FieldName
        {
            get => name;
            set
            {
                name = value;
                m_NameLabel.text = LinkedLabelPrefix + value;
            }
        }

        internal virtual VariableModel LinkedVariable
        {
            get => m_LinkedVariable;
            set
            {
                if (m_LinkedVariable == value)
                    return;

                m_LinkedVariable = value;
                SetLinkVisuals(value);
                OnLinkChanged?.Invoke(value);
            }
        }

        internal BaseLinkField()
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/LinkFieldStyles.uss"));
            AddToClassList("LinkField");
            
            m_LinkFieldContainer = new VisualElement();
            m_LinkFieldContainer.AddToClassList("LinkFieldContainer");

            m_FieldIcon = new VisualElement();
            m_FieldIcon.AddToClassList("FieldTypeIcon");
            m_LinkFieldContainer.Add(m_FieldIcon);

            m_FieldContainer = new VisualElement();
            FieldContainer.AddToClassList("FieldContainer");
            m_LinkFieldContainer.Add(FieldContainer);

            m_NameLabel = new Label();
            m_NameLabel.AddToClassList("FieldNameLabel");
            FieldContainer.Add(m_NameLabel);

            m_LinkedLabelContainer = new VisualElement();
            m_LinkedLabelContainer.AddToClassList("LinkedLabelContainer");
            LinkedLabel = new Label();
            LinkedLabel.AddToClassList("LinkedLabel");
            m_LinkedLabelContainer.Add(LinkedLabel);
            m_LinkFieldContainer.Add(m_LinkedLabelContainer);
            
            Add(m_LinkFieldContainer);

            VisualElement linkButtonContainer = new VisualElement();
            linkButtonContainer.AddToClassList("LinkButtonContainer");
            Add(linkButtonContainer);

            m_LinkButton = new Icon();
            m_LinkButton.iconName = "Link";
            m_LinkButton.image = ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/link.png");
            m_LinkButton.pickingMode = PickingMode.Position;
            m_LinkButton.AddToClassList("LinkButton");
            m_LinkButton.AddManipulator(new Pressable(OnLinkButton));
            Add(m_LinkButton);

            m_UnlinkButton = new Icon();
            m_UnlinkButton.AddToClassList("UnlinkButton");
            m_UnlinkButton.iconName = "x";
            m_UnlinkButton.image = ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/unlink_icon.png");
            m_UnlinkButton.pickingMode = PickingMode.Position;
            m_UnlinkButton.AddManipulator(new Pressable(OnUnlinkButton));
            m_LinkedLabelContainer.Add(m_UnlinkButton);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

#if UNITY_EDITOR
            RegisterCallback<DragEnterEvent>(OnDragEnterEvent);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            RegisterCallback<DragExitedEvent>(OnDragExitEvent);
            RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
#endif
        }

        /// <summary>
        /// Returns whether a type is assignable to the field.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Returns true for assignable, false otherwise.</returns>
        public virtual bool IsAssignable(Type type)
        {
            return LinkVariableType.IsAssignableFrom(type);
        }

#if UNITY_EDITOR
        private void OnDragEnterEvent(DragEnterEvent evt)
        {
            VariableModel variableModel = UnityEditor.DragAndDrop.GetGenericData("VariableModel") as VariableModel;
            if (variableModel != null && IsAssignable(variableModel.Type))
            {
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Link;
                
                SetLinkVisuals(variableModel);
                this.CapturePointer(PointerId.mousePointerId);
            }
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            VariableModel variableModel = UnityEditor.DragAndDrop.GetGenericData("VariableModel") as VariableModel;
            if (variableModel != null && IsAssignable(variableModel.Type))
            {
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Link;
            }
        }

        private void OnDragExitEvent(DragExitedEvent evt)
        {
            if (this.HasPointerCapture(PointerId.mousePointerId))
            {
                this.ReleasePointer(PointerId.mousePointerId);
            }
            SetLinkVisuals(LinkedVariable);
        }

        private void OnDragLeaveEvent(DragLeaveEvent evt)
        {
            if (this.HasPointerCapture(PointerId.mousePointerId))
            {
                this.ReleasePointer(PointerId.mousePointerId);
            }
            SetLinkVisuals(LinkedVariable);
        }

        private void OnDragPerformEvent(DragPerformEvent evt)
        {
            if (this.HasPointerCapture(PointerId.mousePointerId))
            {
                this.ReleasePointer(PointerId.mousePointerId);
            }

            VariableModel variableModel = UnityEditor.DragAndDrop.GetGenericData("VariableModel") as VariableModel;
            if (variableModel != null && IsAssignable(variableModel.Type))
            {
                LinkedVariable = variableModel;
                UnityEditor.DragAndDrop.AcceptDrag();
                using (LinkFieldTypeChangeEvent changeEvent = LinkFieldTypeChangeEvent.GetPooled(this, variableModel?.Type))
                {
                    SendEvent(changeEvent);
                }
            }
            else
            {
                SetLinkVisuals(LinkedVariable);
            }
        }
#endif

        internal virtual void SetLinkWithoutNotify(VariableModel blackboardVariable)
        {
            m_LinkedVariable = blackboardVariable;
            SetLinkVisuals(m_LinkedVariable);
        }

        internal void SetLinkVisuals(VariableModel variable)
        {
            if (variable == null)
            {
                RemoveFromClassList("Linked");
                return;
            }

            AddToClassList("Linked");
            
            if (!string.IsNullOrEmpty(LinkedLabelPrefix))
            {
                LinkedLabel.text = LinkedLabelPrefix + variable.Name; 
            }
            else
            {
                LinkedLabel.text = variable.Name;   
            }
        }

        private void OnUnlinkButton()
        {
            LinkedVariable = null;
            using (LinkFieldLinkButtonEvent evt = LinkFieldLinkButtonEvent.GetPooled(this, null))
            {
                SendEvent(evt);
            }
        }

        private void OnLinkButton()
        {
            using (LinkFieldLinkButtonEvent evt = LinkFieldLinkButtonEvent.GetPooled(this, LinkVariableType, AllowAssetEmbeds))
            {
                SendEvent(evt);
            }
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_GraphEditor = evt.destinationPanel.visualTree.Q<GraphEditor>();
            m_GraphEditor?.RegisterCallback<VariableDeletedEvent>(OnVariableDeletedEvent);
            m_GraphEditor?.RegisterCallback<VariableRenamedEvent>(OnVariableRenamedEvent);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_GraphEditor?.UnregisterCallback<VariableDeletedEvent>(OnVariableDeletedEvent);
            m_GraphEditor?.UnregisterCallback<VariableRenamedEvent>(OnVariableRenamedEvent);
        }

        private void OnVariableDeletedEvent(VariableDeletedEvent evt)
        {
            if (LinkedVariable == evt.Variable)
            {
                LinkedVariable = null;
            }
        }

        private void OnVariableRenamedEvent(VariableRenamedEvent evt)
        {
            if (LinkedVariable == evt.Variable)
            {
                if (!string.IsNullOrEmpty(LinkedLabelPrefix))
                {
                    LinkedLabel.text = LinkedLabelPrefix + evt.Variable.Name;
                }
                else
                {
                    LinkedLabel.text = evt.Variable.Name;   
                }
            }
        }

        internal virtual void UpdateValue(IVariableLink field)
        {
            SetLinkWithoutNotify(field.BlackboardVariable);
        }

        internal void SetFieldIcon(Type type)
        {
            m_FieldIcon.style.backgroundImage = type.GetIcon();
            m_FieldIcon.EnableInClassList("FieldTypeIcon_Assigned", m_FieldIcon.style.backgroundImage.value != null);
        }

        internal virtual void SetValue(object value) { }
    }
}
#endif