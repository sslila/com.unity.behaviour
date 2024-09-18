#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    internal class InsertNodeCommand : Command
    {
        internal NodeInfo InsertedNodeTypeInfo { get; }
        internal Vector2 Position { get; }
        internal Tuple<PortModel, PortModel> ConnectionToBreak { get; }
        internal List<PortModel> ConnectedOutputPorts { get; }
        internal List<PortModel> ConnectedInputPorts { get; }
        
        internal InsertNodeCommand(NodeInfo insertedNodeTypeInfo, Vector2 position,  Tuple<PortModel, PortModel> connectionToBreak, List<PortModel> connectedOutputPorts, List<PortModel> connectedInputPorts, bool markUndo=true) : base(markUndo)
        {
            InsertedNodeTypeInfo = insertedNodeTypeInfo;
            Position = position;
            ConnectionToBreak = connectionToBreak;
            ConnectedOutputPorts = connectedOutputPorts;
            ConnectedInputPorts = connectedInputPorts;
        }
    }
}
#endif