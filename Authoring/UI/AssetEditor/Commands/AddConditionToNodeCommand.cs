#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using Unity.Behavior;

internal class AddConditionToNodeCommand : Command
{
    public NodeModel NodeModel;
    public Condition Condition;
    
    public AddConditionToNodeCommand(IConditionalNodeModel nodeModel, Condition condition, bool markUndo) : base(markUndo)
    {
        NodeModel = nodeModel as NodeModel;
        Condition = condition;
    }
}
#endif