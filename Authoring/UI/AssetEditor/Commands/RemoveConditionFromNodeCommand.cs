#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using Unity.Behavior;

internal class RemoveConditionFromNodeCommand : Command
{
    public NodeModel NodeModel;
    public ConditionModel ConditionModel;

    public RemoveConditionFromNodeCommand(IConditionalNodeModel nodeModel, ConditionModel condition, bool markUndo) : base(markUndo)
    {
        NodeModel = nodeModel as NodeModel;
        ConditionModel = condition;
    }
}
#endif