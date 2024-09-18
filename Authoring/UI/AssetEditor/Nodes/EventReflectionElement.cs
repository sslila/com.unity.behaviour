#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class EventReflectionElement : VisualElement
    {
        private readonly EventNodeModel m_NodeModel;
        private readonly bool m_IsSendNode;
        private readonly bool m_IsRootNode;
        private VisualElement m_StartNodeTitle;
        private readonly List<BaseLinkField> m_MessageFields = new();
        private readonly string m_ChannelFieldName = EventNodeModel.ChannelFieldName;
        private string m_ChannelFieldDisplayName = "Event Channel";
        private BaseLinkField m_ChannelField;
        private VisualElement m_ChannelLine;
        private VisualElement m_ChannelMessageCaption;
        private VisualElement m_MessageLine;
        private VisualElement m_Message;
        private VisualElement m_AssignFieldsElement;

        internal EventReflectionElement(Type runtimeNodeType, EventNodeModel nodeModel)
        {
            m_NodeModel = nodeModel;
            m_IsSendNode = runtimeNodeType == typeof(TriggerEventAction);
            m_IsRootNode = runtimeNodeType == typeof(StartOnEvent);

            if (nodeModel == null)
            {
                return;
            }

            (string message, Type[] fieldTypes) = EventChannelUtility.GetMessageDataFromChannelType(nodeModel.EventChannelType);

            if (m_IsRootNode)
            {
                InitializeStartOnEventUI(message, fieldTypes);
            }
            else
            {
                InitializeUI(message, fieldTypes);
            }
        }

        internal EventReflectionElement(Type runtimeNodeType, string channelName, string message, Type[] fieldTypes)
        {
            // This constructor is used in the wizard node preview, at which point the channel type has not been generated.
            m_IsSendNode = runtimeNodeType == typeof(TriggerEventAction);
            m_ChannelFieldDisplayName = channelName;
            InitializeUI(message, fieldTypes);
            m_ChannelField.SetLinkVisuals(new TypedVariableModel<EventChannelBase> { Name = channelName });
        }

        private void InitializeStartOnEventUI(string message, Type[] messageFieldTypes)
        {
            AddToClassList("Behavior-EventReflection");
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/OnEventStartNodeLayout.uxml").CloneTree(this);
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/EventNodesStylesheet.uss"));

            m_StartNodeTitle = this.Q<VisualElement>("EventStartNodeTitle");
            m_ChannelLine = this.Q<VisualElement>("ChannelLine");
            m_ChannelMessageCaption = this.Q<VisualElement>("ChannelMessageCaption");
            m_AssignFieldsElement = this.Q<VisualElement>("AssignFieldsElement");
            m_MessageLine = this.Q<VisualElement>("MessageLine");
            m_Message = this.Q<VisualElement>("Message");

            m_ChannelMessageCaption.Clear();
            m_AssignFieldsElement.Clear();
            AddEventNodePrefix();
            PopulateChannelLine();

            if (message != null)
            {
                // Populate StartOnEvent node with the full message label and assign fields.
                m_ChannelMessageCaption.Add(new Label(GetMessageWithoutBrackets(message)));
                if (messageFieldTypes.Length != 0)
                {
                    PopulateAssignFields(message, messageFieldTypes);
                    m_AssignFieldsElement.EnableInClassList("Hidden", false);
                }
                else
                {
                    m_AssignFieldsElement.EnableInClassList("Hidden", true);
                    m_StartNodeTitle.AddToClassList("NoMessageTypes");
                }
                m_StartNodeTitle.AddToClassList("TwoLineNodeTitle");
                m_StartNodeTitle.RemoveFromClassList("OneLineNodeTitle");
            }
            else
            {
                m_ChannelMessageCaption.EnableInClassList("Hidden", true);
                m_AssignFieldsElement.EnableInClassList("Hidden", true);
                m_StartNodeTitle.AddToClassList("OneLineNodeTitle");
                m_StartNodeTitle.RemoveFromClassList("TwoLineNodeTitle");
            }
        }

        internal void InitializeUI(string message, Type[] messageFieldTypes)
        {
            AddToClassList("Behavior-EventReflection");

            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/EventNodesLayout.uxml").CloneTree(this);
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/EventNodesStylesheet.uss"));

            m_ChannelLine = this.Q<VisualElement>("ChannelLine");
            m_ChannelMessageCaption = this.Q<VisualElement>("ChannelMessageCaption");
            m_MessageLine = this.Q<VisualElement>("MessageLine");
            m_Message = this.Q<VisualElement>("Message");

            AddEventNodePrefix();
            PopulateMessageFields(message, messageFieldTypes);
            m_ChannelLine.Add(m_MessageLine);
            PopulateChannelLine();
        }

        void AddEventNodePrefix()
        {
            if (m_IsSendNode)
            {
                m_ChannelLine.Add(new Label("Send"));
                return;
            }

            if (m_IsRootNode)
            {
                var label = new Label("On");
                label.AddToClassList("RootNode");
                m_ChannelLine.Add(label);
                m_MessageLine.AddToClassList("RootNode");
            }
            else
            {
                m_ChannelLine.Add(new Label("Wait for"));
            }
        }

        void PopulateChannelLine()
        {
            if (m_IsRootNode)
            {
                m_ChannelFieldDisplayName = "Message";
            }

            m_ChannelField = LinkFieldUtility.CreateNodeLinkField(m_ChannelFieldDisplayName, typeof(BlackboardVariable<EventChannelBase>));
            m_ChannelField.FieldName = m_ChannelFieldName;
            m_ChannelField.Model = m_NodeModel;
            m_ChannelField.RegisterCallback<LinkFieldTypeChangeEvent>(evt => UpdateFields(evt.FieldType));

            if (!m_IsRootNode)
            {
                m_ChannelLine.Add(new Label("on"));
            }
            m_ChannelLine.Add(m_ChannelField);

            if (m_IsRootNode)
            {
                var startNodeModel = m_NodeModel as StartOnEventModel;
                if (startNodeModel == null || startNodeModel.TriggerBehavior == StartOnEvent.TriggerBehavior.Default)
                {
                    return;
                }


                var label = new Label(startNodeModel.TriggerBehavior == StartOnEvent.TriggerBehavior.Once ?
                    StartOnEventModel.k_TriggerOnceNodeUITitleName : StartOnEventModel.k_RestartOnNewMessageNodeUITitleName);
                label.AddToClassList("RootNode");
                m_ChannelLine.Add(label);
            }
        }

        void PopulateMessageFields(string message, Type[] eventMessageTypes)
        {
            bool noMessage = string.IsNullOrEmpty(message);
            if (noMessage && eventMessageTypes == null)
            {
                m_MessageLine.RemoveFromClassList("ChannelAssigned");
                return;
            }

            m_MessageLine.AddToClassList("ChannelAssigned");
            PopulateMessageFieldsWithMessage(message, eventMessageTypes);
        }

        void PopulateAssignFields(string message, Type[] eventMessageTypes)
        {
            int messageFieldIndex = 0;
            string[] messageWords = message.Split(" ");
            for (int i = 0; i < messageWords.Length; ++i)
            {
                string word = messageWords[i];
                if (!(word.StartsWith("[") && word.EndsWith("]"))) //ie a non-parameter word
                {
                    continue;
                }

                word = word.TrimStart('[');
                word = word.TrimEnd(']');

                BaseLinkField linkField = LinkFieldUtility.CreateNodeLinkField(word, eventMessageTypes[messageFieldIndex]);
                messageFieldIndex++;
                linkField.FieldName = word;
                linkField.Model = m_NodeModel;
                VisualElement inputField = linkField.Q("InputField");
                inputField.SetEnabled(m_IsSendNode);

                CreateAssignFieldLineElement(word, linkField);
            }
        }

        void CreateAssignFieldLineElement(string fieldName, BaseLinkField linkField)
        {
            VisualElement assignFieldLine = new VisualElement();
            assignFieldLine.name = "AssignFieldLine";
            Label assignLabel = new Label("Assign " + fieldName + " to");
            assignFieldLine.Add(assignLabel);
            assignFieldLine.Add(linkField);
            m_AssignFieldsElement.Add(assignFieldLine);
        }

        void PopulateMessageFieldsWithMessage(string message, Type[] eventMessageTypes)
        {
            int messageFieldIndex = 0;
            string[] messageWords = message.Split(" ");
            string currentLabelContents = "\"";

            for (int i = 0; i < messageWords.Length; ++i)
            {
                string word = messageWords[i];
                if (!(word.StartsWith("[") && word.EndsWith("]"))) //ie a non-parameter word
                {
                    if (i != 0 && currentLabelContents.Length != 0)
                    {
                        currentLabelContents += " ";
                    }
                    currentLabelContents += word;
                    continue;
                }

                if (currentLabelContents.Length != 0)
                {
                    Label label = new Label(currentLabelContents);
                    label.AddToClassList("BTLabelExtraSpace");
                    m_Message.Add(label);
                    currentLabelContents = string.Empty;
                }
                word = word.TrimStart('[');
                word = word.TrimEnd(']');

                BaseLinkField linkField = LinkFieldUtility.CreateNodeLinkField(word, eventMessageTypes[messageFieldIndex]);
                messageFieldIndex++;
                linkField.FieldName = word;
                linkField.Model = m_NodeModel;
                VisualElement inputField = linkField.Q("InputField");
                inputField.SetEnabled(m_IsSendNode);

                m_Message.Add(linkField);
                m_MessageFields.Add(linkField);
            }
            currentLabelContents += "\"";
            m_Message.Add(new Label(currentLabelContents));
        }

        void UpdateFields(Type eventChannelType)
        {
            // Clear serialized data for message fields
            m_NodeModel.m_FieldValues.RemoveAll(field => field.FieldName != m_ChannelFieldName);

            if (m_IsRootNode)
            {
                ClearAssignFields();
                ClearChannelMessageCaption();
            }
            else
            {
                ClearMessageLine();
            }

            if (eventChannelType != null)
            {
                m_MessageLine.AddToClassList("ChannelAssigned");
                (string message, Type[] fieldTypes) = EventChannelUtility.GetMessageDataFromChannelType(eventChannelType);
                if (m_IsRootNode)
                {
                    PopulateAssignFields(message, fieldTypes);
                    m_ChannelMessageCaption.Add(new Label(GetMessageWithoutBrackets(message)));
                }
                else
                {
                    PopulateMessageFields(message, fieldTypes);
                }
            }
            else
            {
                m_MessageLine.RemoveFromClassList("ChannelAssigned");
            }
        }

        void ClearAssignFields()
        {
            m_AssignFieldsElement.Clear();
        }

        void ClearChannelMessageCaption()
        {
            m_ChannelMessageCaption.Clear();
        }

        void ClearMessageLine()
        {

            // Remove node UI
            foreach (var field in m_MessageFields)
            {
                field.RemoveFromHierarchy();
            }

            m_MessageFields.Clear();
            m_Message.Clear();
        }

        string GetMessageWithoutBrackets(string message)
        {
            message = message.Replace("[", "");
            message = message.Replace("]", "");
            return message;
        }
    }
}
#endif