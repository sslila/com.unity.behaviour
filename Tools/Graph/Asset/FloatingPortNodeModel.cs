using System;

namespace Unity.Behavior.GraphFramework
{
    [Serializable]
    internal class FloatingPortNodeModel : NodeModel
    {
        public SerializableGUID ParentNodeID;
        public string PortName;

        public override bool IsDuplicatable => false;

        public FloatingPortNodeModel()
        {

        }

        public FloatingPortNodeModel(SerializableGUID parentNodeID, string portName)
        {
            ParentNodeID = parentNodeID;
            PortName = portName;
        }

        public override void OnDefineNode()
        {
            base.OnDefineNode();
            if (TryDefaultInputPortModel(out PortModel inputPortModel))
            {
                inputPortModel.IsFloating = true;
            }
        }
    }
}