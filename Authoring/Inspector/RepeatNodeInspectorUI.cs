#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using Unity.AppUI.UI;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(RepeatNodeModel))]
    internal class RepeatNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        Dropdown m_RepeatModeDropdown;
        VisualElement m_ConditionsContainer;

        RepeatNodeModel RepeatNodeModel => InspectedNode as RepeatNodeModel;
        public RepeatNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }

        private void OnRepeatValueChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            var enumerator = evt.newValue.GetEnumerator();
            if (enumerator.MoveNext())
            {
                RepeatNodeModel.RepeatMode newValue = (RepeatNodeModel.RepeatMode)enumerator.Current;
                RepeatNodeModel.Asset.MarkUndo("Change Repeat Mode.");
                RepeatNodeModel.Mode = newValue;
                RepeatNodeModel.OnValidate();
                RepeatNodeModel.Asset.SetAssetDirty();
                Refresh();
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            if (m_RepeatModeDropdown == null)
            {
                CreateDropdownElement();
            }
            else
            {
                RepeatNodeModel.RepeatMode repeatMode = (RepeatNodeModel.RepeatMode)m_RepeatModeDropdown.selectedIndex;
                if (RepeatNodeModel.Mode != repeatMode)
                {
                    m_RepeatModeDropdown.selectedIndex = (int)RepeatNodeModel.Mode;
                }
            }

            if (m_ConditionsContainer == null)
            {
                m_ConditionsContainer = new VisualElement() { name = "ConditionsContainer" };
                NodeProperties.Add(m_ConditionsContainer);
            }
            RefreshConditionalFields();
        }

        void CreateDropdownElement()
        {
            VisualElement dropdownContainer = new VisualElement();
            dropdownContainer.style.flexDirection = FlexDirection.Row;
            dropdownContainer.style.justifyContent = Justify.SpaceBetween;
            dropdownContainer.style.alignItems = Align.Center;
            NodeProperties.Add(dropdownContainer);

            Label repeatModeLabel = new Label("Repeat Mode");
            dropdownContainer.Add(repeatModeLabel);

            m_RepeatModeDropdown = new Dropdown();
            var repeatModes = Enum.GetNames(typeof(RepeatNodeModel.RepeatMode));
            for (int i = 0; i < repeatModes.Length; i++)
            {
                repeatModes[i] = Util.NicifyVariableName(repeatModes[i]);
            }
            m_RepeatModeDropdown.bindItem = (item, i) => item.label = repeatModes[i];
            m_RepeatModeDropdown.sourceItems = repeatModes;
            m_RepeatModeDropdown.selectedIndex = (int)RepeatNodeModel.Mode;
            m_RepeatModeDropdown.RegisterValueChangedCallback(OnRepeatValueChanged);
            dropdownContainer.Add(m_RepeatModeDropdown);
        }

        private void RefreshConditionalFields()
        {
            m_ConditionsContainer.Clear();
            if (InspectedNode is not IConditionalNodeModel conditionalNode || RepeatNodeModel.Mode != RepeatNodeModel.RepeatMode.Condition)
            {
                m_ConditionsContainer.style.display = DisplayStyle.None;
                return;
            }

            m_ConditionsContainer.style.display = DisplayStyle.Flex;
            m_ConditionsContainer.Add(new Divider());
            m_ConditionsContainer.Add(new ConditionRequirementElement("Repeat if", conditionalNode));
            m_ConditionsContainer.Add(new ConditionInspectorElement(conditionalNode));

            // Hide the truncate NodeUI setting on Conditional Guard actions.
            if (InspectedNode is ConditionalGuardNodeModel)
            {
                VisualElement truncateOptionField = this.Q<VisualElement>("TruncateOptionField");
                truncateOptionField.Hide();
            }
        }
    }
}
#endif