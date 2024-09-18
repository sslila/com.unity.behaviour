using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    /// <summary>
    /// Display style of node ports.
    /// </summary>
    public enum PortStyle
    {
        /// <summary>
        /// Port appears on the edge of the node.
        /// </summary>
        Edge,
        /// <summary>
        /// Port appears as a socket.
        /// </summary>
        Socket
    };

    /// <summary>
    /// Orientation of the port.
    /// </summary>
    public enum PortOrientation
    {
        /// <summary>
        /// Vertical orientation.
        /// </summary>
        Vertical,
        /// <summary>
        /// Horizontal orientation.
        /// </summary>
        Horizontal
    };

    /// <summary>
    /// Date flow type for a port.
    /// </summary>
    public enum PortDataFlowType
    {
        /// <summary>
        /// Data is recieved into the port.
        /// </summary>
        Input,
        
        /// <summary>
        /// Data is transmistted out of a port.
        /// </summary>
        Output
    };

    [Serializable]
    internal class PortModel
    {
        public const string k_InputPortName = "InputPort";
        public const string k_OutputPortName = "OutputPort";

        public static PortModel CreateDefaultInputPortModel() => new (k_InputPortName, PortDataFlowType.Input);
        public static PortModel CreateDefaultOutputPortModel() => new (k_OutputPortName, PortDataFlowType.Output);

        public PortModel() {}

        public PortModel(string name, PortDataFlowType portDataFlowType)
        {
            Name = name;
            m_PortDataFlowType = portDataFlowType;
        }

        [SerializeField]
        string m_Name;
        public string Name { get => m_Name; set => m_Name = value; }

        [SerializeField]
        PortDataFlowType m_PortDataFlowType;
        public PortDataFlowType PortDataFlowType
        {
            get => m_PortDataFlowType;
            private set => m_PortDataFlowType = value;
        }

        [SerializeField]
        private bool m_IsFloating;
        public bool IsFloating { get => m_IsFloating; set => m_IsFloating = value; }

        public bool IsInputPort => m_PortDataFlowType == PortDataFlowType.Input;
        public bool IsOutputPort => m_PortDataFlowType == PortDataFlowType.Output;

        public bool IsDefaultInputPort => Name is k_InputPortName;
        public bool IsDefaultOutputPort => Name is k_OutputPortName;
        public bool IsDefaultPort => IsDefaultInputPort || IsDefaultOutputPort;

        [SerializeReference]
        private NodeModel m_NodeModel;
        public NodeModel NodeModel
        {
            get => m_NodeModel;
            set => m_NodeModel = value;
        }

        [SerializeReference]
        private List<PortModel> m_Connections = new ();
        public List<PortModel> Connections
        {
            get => m_Connections;
            set => m_Connections = value.ToList();
        }

        public void ConnectTo(PortModel portModel)
        {
            if (!m_Connections.Contains(portModel))
            {
                m_Connections.Add(portModel);
            }
            if (!portModel.m_Connections.Contains(this))
            {
                portModel.m_Connections.Add(this);
            }
        }

        public void RemoveConnectionTo(PortModel portModel)
        {
            m_Connections.Remove(portModel);
        }

        public void ClearConnections()
        {
            m_Connections.Clear();
        }
    }
}