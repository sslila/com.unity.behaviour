#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeInspectorUI(typeof(PlaceholderNodeModel))]
    internal class PlaceholderNodeInspectorUI : BehaviorGraphNodeInspectorUI
    {
        private PlaceholderNodeModel PlaceholderNodeModel => InspectedNode as PlaceholderNodeModel;

        public PlaceholderNodeInspectorUI(NodeModel nodeModel) : base(nodeModel)
        {
            VisualElement container = new VisualElement();
            container.name = "PlaceholderWarningContainer";
            AppUI.UI.Icon icon = new AppUI.UI.Icon();
            icon.name = "PlaceholderWarningIcon";
            icon.size = AppUI.UI.IconSize.L;
            icon.iconName = "warning";
            container.Add(icon);
            container.Add(new AppUI.UI.Text("Placeholder nodes will be removed or replaced with a Sequence node during execution."));
            NodeInfo.Add(container);
        }

        public override void Refresh()
        {
            base.Refresh();

            Title = $"{PlaceholderNodeModel.Name} (Placeholder)";
            Description = PlaceholderNodeModel.Story;
        }
    }
}
#endif