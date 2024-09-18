using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    [Serializable]
    internal class NodeModel : BaseModel
    {
        [SerializeField]
        public Vector2 Position;

        [SerializeField]
        public SerializableGUID ID = SerializableGUID.Generate();

        [SerializeReference]
        internal List<NodeModel> Parents = new ();

        [SerializeReference]
        internal List<PortModel> PortModels = new ();

        public IEnumerable<PortModel> AllPortModels => PortModels;
        public IEnumerable<PortModel> InputPortModels => PortModels.Where(p => p.IsInputPort);
        public IEnumerable<PortModel> OutputPortModels => PortModels.Where(p => p.IsOutputPort);

        public IEnumerable<PortModel> IncomingConnections => InputPortModels.SelectMany(p => p.Connections);
        public IEnumerable<PortModel> OutgoingConnections => OutputPortModels.SelectMany(p => p.Connections);
        public IEnumerable<NodeModel> PreviousNodeModels => IncomingConnections.Select(c => c.NodeModel);
        public IEnumerable<NodeModel> NextNodeModels => OutgoingConnections.Select(c => c.NodeModel);

        public bool HasPortModels => HasInputPortModels || HasOutputPortModels;
        public bool HasInputPortModels => InputPortModels.Any();
        public bool HasOutputPortModels => OutputPortModels.Any();

        public bool HasIncomingConnections => IncomingConnections.Any();
        public bool HasOutgoingConnections => OutgoingConnections.Any();
        public virtual bool IsDuplicatable => true;
        public virtual bool IsSequenceable => false;

        public virtual bool HasDefaultInputPort => true;
        public virtual bool HasDefaultOutputPort => true;

        public virtual int MaxInputsAccepted => 1;
        public virtual int MaxOutputsAccepted => 1;

        public NodeModel() { }

        protected NodeModel(NodeModel nodeModelOriginal, GraphAsset asset)
        {
            Asset = asset;
            Position = new Vector2(nodeModelOriginal.Position.x + 10, nodeModelOriginal.Position.y + 30);
            // ports are defined via "OnDefineNode()"
            // Parents/Children are not copied (shallow copy)
        }

        public virtual void OnDefineNode()
        {
            if (HasDefaultInputPort)
            {
                var defaultInput = PortModel.CreateDefaultInputPortModel();
                defaultInput.NodeModel = this;
                PortModels.Add(defaultInput);
            }

            if (HasDefaultOutputPort)
            {
                var defaultOutput = PortModel.CreateDefaultOutputPortModel();
                defaultOutput.NodeModel = this;
                PortModels.Add(defaultOutput);
            }
        }

        public PortModel FindPortModelByName(string portName)
        {
            foreach (PortModel port in PortModels)
            {
                if (port.Name == portName)
                {
                    return port;
                }
            }
            return null;
        }

        public bool TryDefaultInputPortModel(out PortModel inputPortModel)
        {
            inputPortModel = null;
            if (!HasDefaultInputPort)
                return false;

            inputPortModel = PortModels.First(portModel => portModel.IsDefaultInputPort);
            return true;
        }

        public bool TryDefaultOutputPortModel(out PortModel outputPortModel)
        {
            outputPortModel = null;
            if (!HasDefaultOutputPort)
                return false;

            outputPortModel = PortModels.First(portModel => portModel.IsDefaultOutputPort);
            return true;
        }

        public void AddPortModel(PortModel portModelNew)
        {
            if (string.IsNullOrEmpty(portModelNew.Name))
            {
                throw new Exception("Port name is empty.");
            }

            if (portModelNew.NodeModel != null)
            {
                throw new Exception("Port is already part of node.");
            }

            if (portModelNew.IsDefaultPort)
            {
                throw new Exception(
                    $"Default ports 'InputPort' and 'OutputPort' cannot be added manually. Use the fields '{nameof(HasDefaultInputPort)}'/'{nameof(HasDefaultOutputPort)}' instead.");
            }

            foreach (PortModel portModel in PortModels)
            {
                if (portModel.Name == portModelNew.Name)
                {
                    throw new Exception($"Port name {portModel.Name} already exists.");
                }
            }
            portModelNew.NodeModel = this;
            PortModels.Add(portModelNew);
        }

        public void RemoveOutputPortModels()
        {
            Asset.DeleteNodePortsForNode(this);
            foreach (PortModel port in PortModels.Where(p => p.IsOutputPort).ToList())
            {
                foreach (PortModel connection in port.Connections.ToList())
                {
                    Asset.DeleteEdge(port, connection);
                }
                PortModels.Remove(port);
            }
        }

        public void RemovePort(PortModel port)
        {
            if (!PortModels.Contains(port))
            {
                return;
            }
            foreach (PortModel connection in port.Connections.ToList())
            {
                Asset.DeleteEdge(port, connection);
                if (connection.NodeModel is FloatingPortNodeModel portNodeModel)
                {
                    Asset.DeleteNode(portNodeModel);
                }
            }
            PortModels.Remove(port);
        }

        public void SortOutputPortModelsBy(List<string> outputOrder)
        {
            PortModels = InputPortModels.Concat(OutputPortModels.OrderBy(item => outputOrder.IndexOf(item.Name))).ToList();
        }

        // this is only needed for
        // - BehaviorAuthoringGraph.UpdateNodeModel() 
        // - GraphAssetProcessor.CreateNode() to convert an implicit sequence to an explicit one, but there is a "to do" there anyway to review if this code is actually working as expected 
        // I would prefer not to allow updating the complete list of port models via API at once, source of bugs (e.g., if the new collection does not contain default ports but the model requires them, what should be done?)
        // should be removed once these two use cases don't need it anymore
        public void SetPortModels(IEnumerable<PortModel> nodeAllPortModels)
        {
            PortModels = new List<PortModel>(nodeAllPortModels);
        }

        public virtual void OnValidate()
        {
            Parents = Parents.Where(parent => parent != null).ToList();
            PortModels = PortModels.Where(port => port != null).ToList();
            foreach (PortModel portModel in PortModels)
            {
                var portModelConnections = GetPortModelConnectionsIgnoreNull(portModel, portModel.NodeModel);
                portModel.Connections = portModelConnections;
                
                for (var index = 0; index < portModel.Connections.Count; index++)
                {
                    var connection = portModel.Connections[index];
                    var connections = GetPortModelConnectionsIgnoreNull(connection, connection);
                    connection.Connections = connections;
                }
            }
        }

        private List<PortModel> GetPortModelConnectionsIgnoreNull<T>(PortModel portModel, T typeToIgnore)
        {
            List<PortModel> portModelConnections = new List<PortModel>();
            for (int i = 0; i < portModel.Connections.Count; i++)
            {
                var connection = portModel.Connections[i];
                if (typeToIgnore != null)
                {
                    portModelConnections.Add(connection);
                }
            }

            return portModelConnections;
        }

        public override int GetHashCode() => ID.GetHashCode();
        public override bool Equals(object other) => Equals(other as NodeModel);

        private static bool ShallowEquals(NodeModel modelA, NodeModel modelB)
        {
            if (modelA == null && modelB == null)
                return true;
            if (modelA == null || modelB == null)
                return false;

            return modelB.ID == modelA.ID && modelB.GetType() == modelA.GetType();
        }

        public bool Equals(NodeModel model)
        {
            if (!ShallowEquals(this,model))
                return false;

            // Check connections one edge from the node.
            foreach (PortModel port in this.PortModels)
            {
                bool foundMatch = false;
                foreach (PortModel otherPort in model.PortModels)
                {
                    if (port.Connections.Count() != otherPort.Connections.Count())
                        continue;

                    if (port.Connections.All(p => otherPort.Connections.Any(o => ShallowEquals(p.NodeModel, o.NodeModel))))
                    {
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                    return false;
            }

            return true;
        }
    }
}