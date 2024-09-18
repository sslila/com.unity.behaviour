using System;
using System.Reflection;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(ParallelAllComposite))]
    [NodeModelInfo(typeof(ParallelAllSuccess))]
    [NodeModelInfo(typeof(ParallelAnyComposite))]
    [NodeModelInfo(typeof(ParallelAnySuccess))]
    internal class RunInParallelNodeModel : CompositeNodeModel
    {
        [Serializable]
        public enum ParallelMode
        {
            Default,
            UntilAnyComplete,
            UntilAnySucceed,
            UntilAnyFail            
        }
        [SerializeField]
        private ParallelMode m_Mode;
        public ParallelMode Mode { get => m_Mode; set => m_Mode = value; }

        public RunInParallelNodeModel(NodeInfo nodeInfo) : base(nodeInfo) { }

        protected RunInParallelNodeModel(RunInParallelNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
            this.Mode = nodeModelOriginal.Mode;
        }

        public override void OnValidate()
        {
            base.OnValidate();
            UpdateNodeType();
        }

        private void UpdateNodeType()
        {
            switch (Mode)
            {
                case ParallelMode.Default:
                    NodeType = typeof(ParallelAllComposite);
                    break;

                case ParallelMode.UntilAnySucceed:
                    NodeType = typeof(ParallelAnySuccess);
                    break;

                case ParallelMode.UntilAnyFail:
                    NodeType = typeof(ParallelAllSuccess);
                    break;

                case ParallelMode.UntilAnyComplete:
                    NodeType = typeof(ParallelAnyComposite);
                    break;
            }
            Type type = NodeType;
            NodeDescriptionAttribute attribute = type.GetCustomAttribute<NodeDescriptionAttribute>();
            if (attribute != null)
            {
                NodeTypeID = attribute.GUID;
            }
        }
    }
}