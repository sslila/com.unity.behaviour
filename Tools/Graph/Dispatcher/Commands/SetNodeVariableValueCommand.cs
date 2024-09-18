namespace Unity.Behavior.GraphFramework
{
    internal class SetNodeVariableValueCommand : Command
    {
        public NodeModel NodeModel;
        public string VariableName;
        public object Value;
        
        public SetNodeVariableValueCommand(NodeModel nodeModel, string variableName, object value, bool markUndo) : base(markUndo)
        {
            NodeModel = nodeModel;
            VariableName = variableName;
            Value = value;
        }
    }
}