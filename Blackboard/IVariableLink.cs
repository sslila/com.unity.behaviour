namespace Unity.Behavior.GraphFramework
{
    internal interface IVariableLink
    {
        public object Value { get; set; }
        public VariableModel BlackboardVariable { get; set; }
    }
}