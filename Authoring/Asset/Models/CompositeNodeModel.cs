using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    internal class CompositeNodeModel : BehaviorGraphNodeModel
    {
        [SerializeReference]
        private readonly List<string> m_NamedChildren;

        public override bool HasDefaultOutputPort => m_NamedChildren != null && !m_NamedChildren.Any();
        public override int MaxOutputsAccepted => int.MaxValue;

        public CompositeNodeModel(NodeInfo nodeInfo) : base(nodeInfo)
        {
            m_NamedChildren = nodeInfo.NamedChildren ?? new List<string>();
        }

        protected CompositeNodeModel(CompositeNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
            m_NamedChildren = nodeModelOriginal.m_NamedChildren ?? new List<string>();
        }

        protected internal override void EnsurePortsAreUpToDate()
        {
            NodeInfo nodeInfo = NodeRegistry.GetInfoFromTypeID(NodeTypeID);
            List<PortModel> outputPortsToRemove = OutputPortModels.Where(port => 
                port.Name != PortModel.k_OutputPortName && !nodeInfo.NamedChildren.Contains(port.Name)).ToList();
            foreach (PortModel port in outputPortsToRemove)
            {
                RemovePort(port);
            }
            foreach (string childName in nodeInfo.NamedChildren)
            {
                if (FindPortModelByName(childName) == null)
                {
                    PortModel portModel = new PortModel(childName, PortDataFlowType.Output) { IsFloating = true };
                    AddPortModel(portModel);
                }
            }
            SortOutputPortModelsBy(nodeInfo.NamedChildren);
        }
    }
}