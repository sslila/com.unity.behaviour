namespace Unity.Behavior.GraphFramework
{
    internal class SetBlackboardVariableValueCommand : Command
    {
        public VariableModel Variable;
        public object Value;
         
        public SetBlackboardVariableValueCommand(VariableModel variable, object value, bool markUndo = true) : base(markUndo)
        {
            Variable = variable;
            Value = value;
        }
    }
}