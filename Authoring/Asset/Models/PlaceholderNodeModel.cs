using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using System.Linq;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    internal class PlaceholderNodeModel : BehaviorGraphNodeModel
    {
        internal string Name { set => m_Name = value; get => m_Name; }
        [SerializeField]
        private string m_Name;
        internal string Story { set => m_Story = value; get => m_Story; }
        [SerializeField]
        private string m_Story;
        internal List<VariableInfo> Variables { set => m_Variables = value; get => m_Variables; }
        [SerializeField]
        private List<VariableInfo> m_Variables = new List<VariableInfo>();
        internal PlaceholderNodeType PlaceholderType { set => m_PlaceholderType = value; get => m_PlaceholderType; }
        [SerializeField]
        private PlaceholderNodeType m_PlaceholderType;

        internal List<string> NamedChildren { get => m_NamedChildren; set => m_NamedChildren = value; }
        [SerializeField]
        private List<string> m_NamedChildren;

        public override bool HasDefaultOutputPort => NamedChildren == null || NamedChildren.Count == 0;
        public override int MaxOutputsAccepted => PlaceholderType == PlaceholderNodeType.Composite ? int.MaxValue : base.MaxOutputsAccepted;
        public override bool IsDuplicatable => false;
        internal enum PlaceholderNodeType
        {
            Action,
            Modifier,
            Composite
        }

        public override bool IsSequenceable => PlaceholderType == PlaceholderNodeType.Action;

        public PlaceholderNodeModel(NodeInfo nodeInfo) : base()
        {
            if (nodeInfo == null)
            {
                return;
            }
            Name = nodeInfo.Name;
            Story = nodeInfo.Story;
            if (nodeInfo.Variables != null)
            {
                Variables = new List<VariableInfo>();
                foreach (VariableInfo variable in nodeInfo.Variables)
                {
                    Variables.Add(new VariableInfo { Name = variable.Name, Type = variable.Type });
                }
            }
            NamedChildren = nodeInfo.NamedChildren;
        }

        public PlaceholderNodeModel(BehaviorAuthoringGraph.NodeModelInfo nodeInfo) : base()
        {
            Name = nodeInfo.Name;
            Story = nodeInfo.Story;
            if (nodeInfo.Variables != null)
            {
                Variables = new List<VariableInfo>();
                foreach (VariableInfo variable in nodeInfo.Variables)
                {
                    Variables.Add(new VariableInfo { Name = variable.Name, Type = variable.Type });
                }
            }
            NamedChildren = nodeInfo.NamedChildren;
        }

        protected PlaceholderNodeModel(PlaceholderNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset)
            : base(nodeModelOriginal, asset)
        {
            Name = nodeModelOriginal.Name;
            Story = nodeModelOriginal.Story;
            PlaceholderType = nodeModelOriginal.PlaceholderType;
            if (nodeModelOriginal.Variables != null)
            {
                Variables = new List<VariableInfo>();
                foreach (VariableInfo variable in nodeModelOriginal.Variables)
                {
                    Variables.Add(new VariableInfo { Name = variable.Name, Type = variable.Type });
                }
            }
        }

        protected override void EnsureFieldValuesAreUpToDate()
        {
            
        }

        protected internal override void EnsurePortsAreUpToDate()
        {
            if (NamedChildren == null)
            {
                return;
            }
            foreach (string childName in NamedChildren)
            {
                if (FindPortModelByName(childName) == null)
                {
                    PortModel portModel = new PortModel(childName, PortDataFlowType.Output) { IsFloating = true };
                    AddPortModel(portModel);
                }
            }
            SortOutputPortModelsBy(NamedChildren);
        }
    }
}