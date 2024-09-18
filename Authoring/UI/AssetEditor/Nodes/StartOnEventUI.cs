#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(StartOnEventModel))]
    internal class StartOnEventUI : BehaviorNodeUI
    {
        public StartOnEventUI(NodeModel nodeModel) : base(nodeModel)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/OnEventStartNodeStylesheet.uss"));
            AddToClassList("RootNode");
            CreateReflectionElement();
        }

        private void CreateReflectionElement()
        {
            EventNodeModel eventNodeModel = Model as EventNodeModel;
            Type runtimeNodeType = NodeRegistry.GetInfoFromTypeID(eventNodeModel.NodeTypeID).Type;
            EventReflectionElement reflectionElement = new EventReflectionElement(runtimeNodeType, eventNodeModel);

            NodeTitle.Add(reflectionElement.Q("EventStartNodeTitle"));
            NodeValueContainer.Add(reflectionElement.Q("AssignFieldsElement"));
        }

        internal override void UpdateLinkFields()
        {
            NodeTitle.Clear();
            NodeValueContainer.Clear();
            CreateReflectionElement();
        }
    }
}
#endif