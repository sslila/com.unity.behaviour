using System;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class CreateNodeCommand : Command
    {
        public Type NodeType { get; }
        public Vector2 Position { get; }
        public PortModel ConnectedPort { get; }
        public SequenceNodeModel SequenceToAddTo { get; }
        public object[] Args { get; }

        public CreateNodeCommand(Type nodeType, Vector2 position, PortModel connectedPort, SequenceNodeModel sequenceToAddTo, object[] args = null, bool markUndo = true) : base(markUndo)
        {
            NodeType = nodeType;
            Position = position;
            ConnectedPort = connectedPort;
            SequenceToAddTo = sequenceToAddTo;
            Args = args;

            if (sequenceToAddTo != null)
            {
                ConnectedPort = null;
            }
        }
    }
}