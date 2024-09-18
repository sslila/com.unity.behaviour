#if UNITY_EDITOR
using UnityEngine;
using Unity.Behavior.GraphFramework;
using static Unity.Behavior.RepeatNodeModel;

namespace Unity.Behavior
{
    [NodeUI(typeof(RunInParallelNodeModel))]
    internal class RunInParallelNodeUI : CompositeNodeUI
    {
        private RunInParallelNodeModel ParallelNodeModel => Model as RunInParallelNodeModel;
        public RunInParallelNodeUI(NodeModel nodeModel) : base(nodeModel) { }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);
            UpdateNodeTitle();
        }

        private void UpdateNodeTitle()
        {
            NodeInfo info = NodeRegistry.GetInfo(ParallelNodeModel.NodeType);
            if (info != null)
            {
                Title = info.Name;
            }
        }
    }
}
#endif