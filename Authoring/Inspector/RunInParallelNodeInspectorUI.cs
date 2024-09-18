#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(RunInParallelNodeModel))]
    internal class RunInParallelNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        Dropdown m_ModeDropdown;
        private RunInParallelNodeModel ParallelNodeModel => InspectedNode as RunInParallelNodeModel;

        public RunInParallelNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }

        private void OnModeValueChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            var enumerator = evt.newValue.GetEnumerator();
            if (enumerator.MoveNext())
            {
                RunInParallelNodeModel.ParallelMode newValue = (RunInParallelNodeModel.ParallelMode)enumerator.Current;
                ParallelNodeModel.Asset.MarkUndo("Change Parallel Mode.");
                ParallelNodeModel.Mode = newValue;
                ParallelNodeModel.OnValidate();
                ParallelNodeModel.Asset.SetAssetDirty();
                Refresh();
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            if (m_ModeDropdown == null)
            {
                CreateDropdownElement();
            }
            else
            {
                RunInParallelNodeModel.ParallelMode parallelMode = (RunInParallelNodeModel.ParallelMode)m_ModeDropdown.selectedIndex;
                if (ParallelNodeModel.Mode != parallelMode)
                {
                    m_ModeDropdown.selectedIndex = (int)ParallelNodeModel.Mode;
                }
            }
        }

        void CreateDropdownElement()
        {
            VisualElement dropdownContainer = new VisualElement();
            dropdownContainer.style.flexDirection = FlexDirection.Row;
            dropdownContainer.style.justifyContent = Justify.SpaceBetween;
            dropdownContainer.style.alignItems = Align.Center;
            NodeProperties.Add(dropdownContainer);

            Label parallelModeLabel = new Label("Parallel Mode");
            dropdownContainer.Add(parallelModeLabel);

            m_ModeDropdown = new Dropdown();
            var parallelModes = Enum.GetNames(typeof(RunInParallelNodeModel.ParallelMode));
            for (int i = 0; i < parallelModes.Length; i++)
            {
                parallelModes[i] = Util.NicifyVariableName(parallelModes[i]);
            }
            m_ModeDropdown.bindItem = (item, i) => item.label = parallelModes[i];
            m_ModeDropdown.sourceItems = parallelModes;
            m_ModeDropdown.selectedIndex = (int)ParallelNodeModel.Mode;
            m_ModeDropdown.RegisterValueChangedCallback(OnModeValueChanged);
            dropdownContainer.Add(m_ModeDropdown);
        }
    }
}
#endif