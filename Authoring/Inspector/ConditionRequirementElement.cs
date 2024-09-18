#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class ConditionRequirementElement : VisualElement
    {
        string m_ConditionRequirementLabel;
        Dropdown m_ConditionRequirementDropdown;
        public ConditionRequirementElement(string conditionRequirementLabel, IConditionalNodeModel conditionalNodeModel)
        {
            m_ConditionRequirementLabel = conditionRequirementLabel;

            VisualElement conditionRequirementDropdown = new VisualElement();
            conditionRequirementDropdown.AddToClassList("DropdownPropertyElement");
            Label label = new Label(m_ConditionRequirementLabel);
            label.tooltip = "Select if your node should check if any condition is met or if all conditions must return true.";
            conditionRequirementDropdown.Add(label);
            m_ConditionRequirementDropdown = new Dropdown();
            List<string> types = new List<string>
            {
                "Any Are True",
                "All Are True"
            };
            m_ConditionRequirementDropdown.bindItem = (item, i) => item.label = types[i];
            m_ConditionRequirementDropdown.sourceItems = types;
            m_ConditionRequirementDropdown.selectedIndex = conditionalNodeModel.RequiresAllConditionsTrue ? 1 : 0;
            conditionRequirementDropdown.Add(m_ConditionRequirementDropdown);

            m_ConditionRequirementDropdown.RegisterValueChangedCallback(evt =>
            {
                conditionalNodeModel.RequiresAllConditionsTrue = evt.newValue.First() == 1;
                BehaviorGraphNodeModel model = conditionalNodeModel as BehaviorGraphNodeModel;
                model?.OnValidate();
                model?.Asset.SetAssetDirty();
                //Refresh();
            });

            Add(conditionRequirementDropdown);
        }
    }
}
#endif