#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;


namespace Unity.Behavior
{
    internal class ReflectionElement : VisualElement
    {
        internal bool IsTwoLineElement;
        internal BehaviorGraphNodeModel Node
        {
            set
            {
                foreach (VisualElement child in Children())
                {
                    if (child is BaseLinkField field)
                    {
                        field.Model = value;
                    }
                }
            }
        }

        internal ReflectionElement()
        {
            AddToClassList("Behavior-Reflection");
        }

        internal ReflectionElement(NodeInfo nodeInfo) : this()
        {
            CreateFields(nodeInfo);
        }

        internal void CreateFields(NodeInfo nodeInfo)
        {            
            tooltip = nodeInfo.Name;

            Clear();
            if (nodeInfo.Story.Length == 0)
            {
                IsTwoLineElement = false;
                Add(new Label(nodeInfo.Name));
                return;
            }

            IsTwoLineElement = true;
            StoryElementUtility.CreateStoryElement(nodeInfo.Story , nodeInfo.Variables, this, (variableName, type) =>
            {
                BaseLinkField field = LinkFieldUtility.CreateNodeLinkField(variableName, type);
                field.FieldName = variableName;
                return field;
            });
        }
    }
}
#endif