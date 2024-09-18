#if UNITY_EDITOR
using System;
using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(SubgraphNodeModel))]
    internal class SubgraphNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        private SubgraphNodeModel m_NodeModel => InspectedNode as SubgraphNodeModel;
        private BaseLinkField m_SubgraphField;
        private BaseLinkField m_BlackboardAssetField;
        private const string k_StaticNodeTitle = "Run Subgraph";
        private const string k_DynamicNodeTitle = "Run Subgraph Dynamically";
        private const string k_DefaultDescription = "Running subgraphs allows you to keep your graphs clean and to switch out functionality at runtime using dynamic subgraphs.";
        private const string k_DynamicNodeDescription = "You're going to run a subgraph dynamically. Make sure you have your subgraph implement a Blackboard asset that you can refer to in this inspector.";
        
        public SubgraphNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }

        public override void Refresh()
        {
            UpdateInspectorTitleAndDescription();

            NodeProperties.Clear();
            CreateSubgraphField();
            CreateDescriptionField();
            if (m_NodeModel.IsDynamic)
            {
                CreateBlackboardAssetField();
            }
            else
            {
                if (m_SubgraphField.LinkedVariable != null)
                {
                    CreateSubgraphRepresentationToggle();
                    CreateBlackboardFields();     
                }
            }
        }

        private void CreateSubgraphRepresentationToggle()
        {
            VisualElement representationToggleElement = new VisualElement();
            representationToggleElement.name = "ToggleOptionField";
            representationToggleElement.AddToClassList("ToggleOptionField");
            representationToggleElement.Add(new Label("Subgraph Representation"));
            representationToggleElement.tooltip = "Show the subgraph representation on the node UI.";
            Toggle toggle = new Toggle();
            representationToggleElement.Add(toggle);
            
            toggle.value = m_NodeModel.ShowStaticSubgraphRepresentation;
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_NodeModel.ShowStaticSubgraphRepresentation = evt.newValue;
                m_NodeModel?.Asset.SetAssetDirty();
            });
            
            NodeProperties.Add(representationToggleElement);
        }

        private void UpdateInspectorTitleAndDescription()
        {
            Title = m_NodeModel.IsDynamic ? k_DynamicNodeTitle : k_StaticNodeTitle;
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(m_NodeModel.NodeTypeID);
            if (m_NodeModel.SubgraphField.LinkedVariable == null)
            {
                // Show a default description text before a subgraph has been assigned.
                Description = k_DefaultDescription;
            }
            else
            {
                // Set the description depending on if the graph is being run dynamically or not.
                Description = m_NodeModel.IsDynamic ? k_DynamicNodeDescription : nodeInfo.Description;   
            }
        }

        private void CreateDescriptionField()
        {
            if (m_NodeModel.SubgraphField.LinkedVariable != null)
            {
                ScrollView descriptionField = new ScrollView();
                descriptionField.name = "DescriptionField";
                Label descriptionLabel = new Label();
                if (!string.IsNullOrEmpty(m_NodeModel.SubgraphAuthoringAsset?.Description))
                {
                    descriptionLabel.text = m_NodeModel.SubgraphAuthoringAsset?.Description;
                }
                else
                {
                    descriptionLabel.text = "(No description added)";
                    // Todo: Remove this if we want to add the placeholder text above.
                    descriptionField.Hide();
                }
                descriptionField.Add(descriptionLabel);
                NodeProperties.Add(descriptionField);
            }
        }

        private void CreateSubgraphField()
        {
            if (m_SubgraphField == null)
            {
                // Typically, we'd use CreateField() here, but we need to use a different field type for the subgraph field,
                // which isn't supported by LinkFieldUtility.CreateForType(name, type).
                m_SubgraphField = new BaseLinkField { 
                    FieldName = SubgraphNodeModel.k_SubgraphFieldName, 
                    LinkVariableType = typeof(BehaviorGraph),
                    AllowAssetEmbeds = true,
                    Model = InspectedNode
                };
                m_SubgraphField.OnLinkChanged += _ =>
                {
                    m_SubgraphField.Dispatcher.DispatchImmediate(new SetNodeVariableLinkCommand(m_NodeModel, m_SubgraphField.FieldName, m_SubgraphField.LinkVariableType, m_SubgraphField.LinkedVariable, true));

                    m_NodeModel.OnValidate();
                    m_NodeModel.CacheRuntimeGraphId();

                    // Reset the blackboard field if there is no subgraph variable linked.
                    if (m_SubgraphField.LinkedVariable == null)
                    {
                        if (m_BlackboardAssetField != null)
                        {
                            m_BlackboardAssetField.LinkedVariable = null;      
                        }
                    }
                };
                
                m_SubgraphField.AddToClassList("LinkField-Light");
                m_SubgraphField.RegisterCallback<LinkFieldTypeChangeEvent>(_ =>
                {
                    Refresh();
                });
            }
            
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.AddToClassList("Inspector-FieldContainer");
            fieldContainer.Add(new Label(SubgraphNodeModel.k_SubgraphFieldName));
            fieldContainer.Add(m_SubgraphField);
            
            NodeProperties.Add(fieldContainer);

            // Update the inspector LinkField value from the NodeModel LinkedVariable value.
            m_SubgraphField.LinkedVariable = m_NodeModel.SubgraphField.LinkedVariable == null ? null : m_NodeModel.SubgraphField.LinkedVariable;
        }
        
        private void CreateBlackboardAssetField()
        {
            if (m_BlackboardAssetField == null)
            {
                // Typically, we'd use CreateField() here, but we need to use a different field type for the blackboard asset field,
                // which isn't supported by LinkFieldUtility.CreateForType(name, type).
                m_BlackboardAssetField = new BaseLinkField { 
                    FieldName = SubgraphNodeModel.k_BlackboardFieldName, 
                    LinkVariableType = typeof(BehaviorBlackboardAuthoringAsset),
                    AllowAssetEmbeds = true,
                    Model = InspectedNode
                };
                m_BlackboardAssetField.OnLinkChanged += _ =>
                {
                    m_BlackboardAssetField.Dispatcher.DispatchImmediate(new SetNodeVariableLinkCommand(m_NodeModel, m_BlackboardAssetField.FieldName, m_BlackboardAssetField.LinkVariableType, m_BlackboardAssetField.LinkedVariable, true));
                    m_NodeModel.OnValidate();
                };
                m_BlackboardAssetField.AddToClassList("LinkField-Light");
            }
            
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.AddToClassList("Inspector-FieldContainer");
            fieldContainer.Add(new Label("Blackboard"));
            fieldContainer.Add(m_BlackboardAssetField);
            
            NodeProperties.Add(fieldContainer);

            if (m_NodeModel.BlackboardAssetField?.LinkedVariable != null)
            {
                m_BlackboardAssetField.LinkedVariable = m_NodeModel.BlackboardAssetField.LinkedVariable;
            }

            CreateRequiredBlackboardAssetFields();
        }

        private void CreateBlackboardFields()
        {
            if (m_NodeModel.RuntimeSubgraph == null)
            {
                return;
            }

            if (m_NodeModel.SubgraphAuthoringAsset == null)
            {
                return;
            }
            
            Divider divider = new Divider();
            divider.size = Size.S;
            divider.AddToClassList("FieldDivider");
            NodeProperties.Add(divider);

            foreach (VariableModel variable in m_NodeModel.SubgraphAuthoringAsset.Blackboard.Variables)
            {
                CreateAssignVariableFieldElement(variable);
            }

            // Create fields for any added Blackboard group variables.
            foreach (BehaviorBlackboardAuthoringAsset blackboard in m_NodeModel.SubgraphAuthoringAsset.m_Blackboards)
            {
                foreach (VariableModel variable in blackboard.Variables)
                {
                    CreateAssignVariableFieldElement(variable);
                }
            }
        }
        
        private void CreateRequiredBlackboardAssetFields()
        {
            if (m_NodeModel.BlackboardAssetField?.LinkedVariable == null)
            {
                return;
            }
            
            Divider divider = new Divider();
            divider.size = Size.S;
            divider.AddToClassList("FieldDivider");
            NodeProperties.Add(divider);

            if (m_NodeModel.RequiredBlackboard.Variables.Count == 0)
            {
                NodeProperties.Add(new Label("Blackboard is empty"));
            }

            foreach (VariableModel variable in m_NodeModel.RequiredBlackboard.Variables)
            {
                CreateAssignVariableFieldElement(variable);
            }
        }

        private void CreateAssignVariableFieldElement(VariableModel variable)
        {
            if (variable.IsShared)
            {
                CreateSharedVariableElement(variable.Name, variable.Type);
            }
            else
            {
                CreateField(variable.Name, variable.Type);   
            }
        }

        private VisualElement CreateSharedVariableElement(string fieldName, Type fieldType)
        {
            string nicifiedFieldName = Util.NicifyVariableName(fieldName);
            BaseLinkField field = LinkFieldUtility.CreateNodeLinkField(fieldName, fieldType);
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.AddToClassList("Inspector-FieldContainer");
            fieldContainer.Add(new Label($"{nicifiedFieldName} (Shared)"));
            fieldContainer.Add(field);
            NodeProperties.Add(fieldContainer);
            field.AddToClassList("LinkField-Light");
            field.SetEnabled(false);
            field.tooltip = "Variables marked as 'Shared' can not be assigned through a subgraph node.";

            return field;
        }
    }
}
#endif