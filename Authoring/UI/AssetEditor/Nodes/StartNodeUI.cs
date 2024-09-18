#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.Behavior
{
    [NodeUI(typeof(StartNodeModel))]
    internal class StartNodeUI : BehaviorNodeUI
    {
        private Toggle m_RepeatToggle; 
        
        public StartNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Start");
            AddToClassList("TwoLineNode");
            BehaviorGraphNodeModel behaviorNodeModel = nodeModel as BehaviorGraphNodeModel;
            if (behaviorNodeModel == null)
            {
                return;
            }

            StartNodeModel startModel = nodeModel as StartNodeModel;
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(behaviorNodeModel.NodeTypeID);
            Title = nodeInfo.Name;

            m_RepeatToggle = new Toggle() { label = "Repeat", name = "StartToggle" };
            m_RepeatToggle.value = startModel.Repeat;
            m_RepeatToggle.RegisterValueChangedCallback(OnRepeatValueChanged);
            NodeValueContainer.Add(m_RepeatToggle);
        }

        private void OnRepeatValueChanged(ChangeEvent<bool> evt)
        {
            StartNodeModel startModel = Model as StartNodeModel;
            startModel.Asset.MarkUndo("Toggle Start Node Repeat.");
            startModel.Repeat = evt.newValue;
        }

        internal override void UpdateLinkFields()
        {
            base.UpdateLinkFields();
            
            // A special case where we need to set the non-LinkField Toggle element to the updated value from the model.
            StartNodeModel startModel = Model as StartNodeModel;
            m_RepeatToggle.SetValueWithoutNotify(startModel.Repeat);
        }
    }
}
#endif