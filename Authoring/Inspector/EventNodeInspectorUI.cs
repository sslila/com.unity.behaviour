#if UNITY_EDITOR
using System;
using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(EventNodeModel))]
    [NodeInspectorUI(typeof(StartOnEventModel))]
    internal class EventNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        private StartOnEventModel m_StartOnEventModel => InspectedNode as StartOnEventModel;
        private EventNodeModel m_EventNodeModel => InspectedNode as EventNodeModel;

        public EventNodeInspectorUI(NodeModel nodeModel) : base(nodeModel) { }

        private Dropdown m_BehaviorDropdown;

        public override void Refresh()
        {
            NodeProperties.Clear();

            if (m_StartOnEventModel is not null)
            {
                CreateTypeSelectionElement();
            }

            CreateEventChannelField();
            CreateEventFields();
        }

        private void CreateTypeSelectionElement()
        {
            m_BehaviorDropdown = CreateDropdownField(StartOnEventModel.k_TriggerModeFieldName,
                StartOnEventModel.k_TriggerModeTooltips,
                Enum.GetNames(typeof(StartOnEvent.TriggerBehavior)),
                (int)m_StartOnEventModel.TriggerBehavior);

            m_BehaviorDropdown.RegisterValueChangedCallback(evt =>
            {
                m_StartOnEventModel.Asset.MarkUndo("Undo Trigger Mode Change");
                using (var newVal = evt.newValue.GetEnumerator())
                {
                    if (newVal.MoveNext())
                    {
                        m_StartOnEventModel.TriggerBehavior = (StartOnEvent.TriggerBehavior)newVal.Current;
                    }
                }
            });
        }

        private void CreateEventChannelField()
        {
            var channelField = CreateField(EventNodeModel.ChannelFieldName, typeof(BlackboardVariable<EventChannelBase>));
            channelField.RegisterCallback<LinkFieldTypeChangeEvent>(_ => Refresh());
        }

        private void CreateEventFields()
        {
            (string message, Type[] fieldTypes) = EventChannelUtility.GetMessageDataFromChannelType(m_EventNodeModel.EventChannelType);
            if (fieldTypes == null)
            {
                return;
            }

            Type runtimeNodeType = NodeRegistry.GetInfoFromTypeID(m_EventNodeModel.NodeTypeID).Type;
            bool allowInlineValues = runtimeNodeType == typeof(TriggerEventAction);

            int messageFieldIndex = 0;
            string[] messageWords = message.Split(" ");
            for (int i = 0; i < messageWords.Length; ++i)
            {
                string word = messageWords[i];
                if (!(word.StartsWith("[") || word.EndsWith("]"))) // Non-parameter word.
                {
                    continue;
                }
                word = word.TrimStart('[');
                word = word.TrimEnd(']');

                BaseLinkField linkField = CreateField(word, fieldTypes[messageFieldIndex++]);
                VisualElement inputField = linkField.Q("InputField");
                inputField.SetEnabled(allowInlineValues);
            }
        }
    }
}
#endif