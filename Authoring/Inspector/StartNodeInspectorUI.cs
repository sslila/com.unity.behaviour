#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(StartNodeModel))]
    internal class StartNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        private Toggle m_RepeatField;
        public StartNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }

        public override void Refresh()
        {
            base.Refresh();
            if (m_RepeatField == null)
            {
                m_RepeatField = CreateField<Toggle>("Repeat");
                m_RepeatField.RegisterValueChangedCallback(OnRepeatValueChanged);
            }
            StartNodeModel startModel = InspectedNode as StartNodeModel;
            m_RepeatField.SetValueWithoutNotify(startModel.Repeat);
        }

        private void OnRepeatValueChanged(ChangeEvent<bool> evt)
        {
            StartNodeModel startModel = InspectedNode as StartNodeModel;
            startModel.Asset.MarkUndo("Toggle Start Node Repeat.");
            startModel.Repeat = evt.newValue;
        }
    }
}

#endif