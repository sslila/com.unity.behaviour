using System;

namespace Unity.Behavior.GraphFramework
{
    internal class SetNodeVariableLinkCommand : Command
    {
        public NodeModel NodeModel;
        public string VariableName;
        public Type LinkVariableType;
        public VariableModel Link;
        public SetNodeVariableLinkCommand(NodeModel nodeModel, string variableName, Type linkVariableType, VariableModel link, bool markUndo) : base(markUndo)
        {
            NodeModel = nodeModel;
            VariableName = variableName;
            LinkVariableType = linkVariableType;
            Link = link;
        }
    }
}