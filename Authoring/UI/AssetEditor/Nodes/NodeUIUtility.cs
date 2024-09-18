#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal static class NodeUIUtility
    {
        internal static VisualElement CreatePortUIContainer(PortModel portModel)
        {
            var port = new Port(portModel)
            {
                Orientation = PortOrientation.Vertical,
                Style = PortStyle.Socket,
            };

            var portContainer = new VisualElement
            {
                name = port.name + "-port-container"
            };
            portContainer.AddToClassList("PortContainer");

            var nicifiedPortName = Util.NicifyVariableName(port.name);
            var portLabel = new Label(nicifiedPortName)
            {
                name = port.name + "-Label"
            };
            portContainer.Add(portLabel);
            portContainer.Add(port);
            return portContainer;
        }

        internal static void AddNodeIcon(this VisualElement visualElement, NodeInfo nodeInfo)
        {
            if (nodeInfo.Icon == null)
                return;

            VisualElement icon = new VisualElement();
            icon.name = "Icon";
            icon.AddToClassList("NodeIcon");
            icon.style.backgroundImage = nodeInfo.Icon;
            visualElement.Add(icon);
        }
    }
}
#endif