using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable]
    [NodeModelInfo(typeof(SwitchComposite))]
    internal class SwitchNodeModel : BehaviorGraphNodeModel
    {
        public override bool HasDefaultOutputPort => false;

        public SwitchNodeModel(NodeInfo nodeInfo) : base(nodeInfo) { }

        protected SwitchNodeModel(SwitchNodeModel nodeModelOriginal, BehaviorAuthoringGraph asset) : base(nodeModelOriginal, asset)
        {
            foreach (var outputPortModel in nodeModelOriginal.OutputPortModels)
            {
                AddPortModel(new PortModel(outputPortModel.Name, PortDataFlowType.Output) { IsFloating = true });
            }
        }
        
        protected internal override void EnsurePortsAreUpToDate()
        {
            foreach (FieldModel field in Fields)
            {
                if (field.FieldName == "EnumVariable" && field.LinkedVariable != null && field.LinkedVariable.Type.IsSubclassOf(typeof(Enum)))
                {
                    ValidatePortsFromEnumType(field.LinkedVariable.Type);
                    return;
                }
            }
        }

        private void ValidatePortsFromEnumType(Type enumType)
        {
            string[] enumNames = Enum.GetNames(enumType);
            bool portsChanged = false;
            List<PortModel> outputPortsToRemove = OutputPortModels.Where(port => !enumNames.Contains(port.Name)).ToList();
            foreach (PortModel outputPort in outputPortsToRemove)
            {
                RemovePort(outputPort);
                portsChanged = true;
            }

            foreach (string enumName in enumNames)
            {
                if (FindPortModelByName(enumName) == null)
                {
                    AddPortModel(new PortModel(enumName, PortDataFlowType.Output) { IsFloating = true });
                    portsChanged = true;
                }
            }

            if (portsChanged)
            {
                // Sort the order of the ports.
                SortOutputPortModelsBy(enumNames.ToList());
                Asset.CreateNodePortsForNode(this);
            }
        }
    }
}