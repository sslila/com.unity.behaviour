#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(SwitchNodeModel))]
    internal class SwitchNodeUI : BehaviorNodeUI
    {
        private readonly LinkField<Enum, RuntimeEnumField> m_EnumLinkField;
        private VariableModel m_LastAssignedEnumVariable;
        
        public SwitchNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/SwitchNodeStyles.uss"));
            AddToClassList("SwitchNodeUI");
            AddToClassList("TwoLineNode");

            Title = "Switch";
            
            m_EnumLinkField = new BehaviorLinkField<Enum, RuntimeEnumField>()
            {
                style =
                {
                    alignSelf = Align.Center,
                    minHeight = 22
                },
                FieldName = "EnumVariable",
                Model = nodeModel
            };
            
            m_EnumLinkField.RegisterCallback<LinkFieldTypeChangeEvent>(evt =>
            {
                nodeModel.Asset.MarkUndo("Assign new enum to switch node");
                nodeModel.RemoveOutputPortModels();
                
                if (evt.FieldType is { IsEnum: true })
                {
                    foreach (var member in Enum.GetNames(evt.FieldType))
                    {
                        nodeModel.AddPortModel(new PortModel(member, PortDataFlowType.Output) { IsFloating = true });
                    }
                    nodeModel.Asset.CreateNodePortsForNode(nodeModel);
                }
            });

            m_EnumLinkField.OnLinkChanged += (newLink =>
            {
                m_EnumLinkField.Dispatcher.DispatchImmediate(new SetNodeVariableLinkCommand(nodeModel, m_EnumLinkField.FieldName, m_EnumLinkField.LinkVariableType, m_EnumLinkField.LinkedVariable, true));
                if (newLink == null)
                {
                    nodeModel.RemoveOutputPortModels();
                    RefreshOutputPortUIs();
                }
            });
            m_EnumLinkField.Q<RuntimeEnumField>().label = "Choose an Enum";


            NodeValueContainer.Add(m_EnumLinkField);

            m_LastAssignedEnumVariable = m_EnumLinkField.LinkedVariable;
        }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);
            if (m_EnumLinkField.LinkedVariable != m_LastAssignedEnumVariable)
            {
                RefreshOutputPortUIs();
            }
            m_LastAssignedEnumVariable = m_EnumLinkField.LinkedVariable;
        }
        
        private void RefreshOutputPortUIs()
        {
            // Clear output port UIs.
            OutputPortsContainer.Clear();
            
            // Create new port UIs.
            foreach (PortModel portModel in Model.OutputPortModels)
            {
                var portUIContainer = CreatePortUI(portModel);
                if (portUIContainer == null)
                {
                    throw new Exception(
                        $"The port UI created for {portModel.Name} does not contain a element of type {nameof(Port)}, which is required.");
                }
                
                OutputPortsContainer.Add(portUIContainer);
            }
        }
    }
}
#endif