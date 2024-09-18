#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using Unity.Behavior;

internal class RemoveConditionFromNodeCommandHandler : CommandHandler<RemoveConditionFromNodeCommand>
{
    public override bool Process(RemoveConditionFromNodeCommand command)
    {
        ConditionInfo info = ConditionUtility.GetInfoForConditionType(command.ConditionModel.ConditionType);
        Asset.MarkUndo($"Remove condition {info.Name} from a node");
        BehaviorGraphNodeModel behaviorNodeModel = command.NodeModel as BehaviorGraphNodeModel;

        if (behaviorNodeModel is IConditionalNodeModel conditionalNodeModel)
        {
            conditionalNodeModel.RemoveCondition(command.ConditionModel);
        }

        // Have we processed the command and wish to block further processing?
        return false;
    }
}
#endif