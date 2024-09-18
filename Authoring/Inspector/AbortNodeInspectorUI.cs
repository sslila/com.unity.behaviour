#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using System.Linq;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(AbortNodeModel))]
    internal class AbortNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        private AbortNodeModel m_NodeModel => InspectedNode as AbortNodeModel;

        private Dropdown m_TypeDropdown;
        private Dropdown m_ConditionRequirementDropdown;
        private Button m_AssignButton;
        private ListView m_ConditionListView;
        private VisualElement m_ConditionContainer;

        public AbortNodeInspectorUI(NodeModel nodeModel) : base(nodeModel)
        {
            InitNodeProperties();
        }

        public override void Refresh()
        {
            base.Refresh();
            InitNodeProperties();
        }

        private void InitNodeProperties()
        {
            NodeProperties.Clear();
            
            NodeProperties.Add(CreateTypeSelectionElement());
            NodeProperties.Add(CreateConditionRequirementElement());
            NodeProperties.Add(new ConditionInspectorElement(m_NodeModel));
        }

        private VisualElement CreateTypeSelectionElement()
        {
            VisualElement typeDropdownContainer = new VisualElement();
            typeDropdownContainer.AddToClassList("DropdownPropertyElement");
            Label label = new Label("Type");
            label.tooltip = "Select the type of abort node. 'Abort' ends the execution of all the children nodes and returns failure to the parent node. 'Restart' ends all the children nodes and then restarts them.";
            typeDropdownContainer.Add(label);
            m_TypeDropdown = new Dropdown();
            List<string> types = Enum.GetNames(typeof(AbortNodeModel.AbortType)).ToList();
            m_TypeDropdown.bindItem = (item, i) => item.label = types[i];
            m_TypeDropdown.sourceItems = types;
            m_TypeDropdown.selectedIndex = (int)m_NodeModel.ModelAbortType;
            typeDropdownContainer.Add(m_TypeDropdown);

            m_TypeDropdown.RegisterValueChangedCallback(evt =>
            {
                m_NodeModel.ModelAbortType = (AbortNodeModel.AbortType)evt.newValue.First();
                m_NodeModel.OnValidate();
                m_NodeModel.Asset.SetAssetDirty();
                Refresh();
            });

            return typeDropdownContainer;
        }
        
        private VisualElement CreateConditionRequirementElement()
        {
            VisualElement conditionRequirementDropdown = new VisualElement();
            conditionRequirementDropdown.AddToClassList("DropdownPropertyElement");
            Label label = new Label();   
            label.text = m_NodeModel.ModelAbortType == AbortNodeModel.AbortType.Abort ? "Aborts if" : "Restarts if";
            label.tooltip = "Select if your node should abort if any condition is met or if all conditions must return true.";
            conditionRequirementDropdown.Add(label);
            m_ConditionRequirementDropdown = new Dropdown();
            List<string> types = new List<string>
            {
                "Any Are True",
                "All Are True"
            };
            m_ConditionRequirementDropdown.bindItem = (item, i) => item.label = types[i];
            m_ConditionRequirementDropdown.sourceItems = types;
            m_ConditionRequirementDropdown.selectedIndex = m_NodeModel.RequiresAllConditionsTrue? 1 : 0;
            conditionRequirementDropdown.Add(m_ConditionRequirementDropdown);

            m_ConditionRequirementDropdown.RegisterValueChangedCallback(evt =>
            {
                m_NodeModel.RequiresAllConditionsTrue = evt.newValue.First() == 1;
                m_NodeModel.OnValidate();
                m_NodeModel.Asset.SetAssetDirty();
                Refresh();
            });

            return conditionRequirementDropdown;
        }
    }
}
#endif