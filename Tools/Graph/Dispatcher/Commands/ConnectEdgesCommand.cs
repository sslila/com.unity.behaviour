using System;
using System.Collections.Generic;

namespace Unity.Behavior.GraphFramework
{
    internal class ConnectEdgesCommand : Command
    {
        public List<Tuple<PortModel, PortModel>> PortPairsToConnect { get; }

        public ConnectEdgesCommand(List<Tuple<PortModel, PortModel>> portPairs, bool markUndo= true) : base(markUndo)
        {
            PortPairsToConnect = portPairs;
        }
    }
}