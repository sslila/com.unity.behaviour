#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    [NodeUI(typeof(EventNodeModel))]
    internal class EventActionNodeUI : BehaviorNodeUI
    {
        
        public EventActionNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("Action");
            AddToClassList("ShowNodeColor");
            CreateReflectionElement();
        }

        void CreateReflectionElement()
        {
            if (Model == null)
            {
                return;
            }

            EventNodeModel eventNodeModel = Model as EventNodeModel;
            Type runtimeNodeType = NodeRegistry.GetInfoFromTypeID(eventNodeModel.NodeTypeID).Type;
            EventReflectionElement reflectionElement = new EventReflectionElement(runtimeNodeType, eventNodeModel);
            Insert(0, reflectionElement);
        }

        internal void InitFromInfo(Type runtimeNodeType, string message, Type[] fieldTypes)
        {
            Clear();
            EventReflectionElement reflectionElement = new EventReflectionElement(runtimeNodeType, null);
            reflectionElement.InitializeUI(message, fieldTypes);
            Insert(0, reflectionElement);
        }

        internal override void UpdateLinkFields()
        {
            Clear();
            CreateReflectionElement();
        }
    }
}
#endif