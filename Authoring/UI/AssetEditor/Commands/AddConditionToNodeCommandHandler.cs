#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using Unity.Behavior;

namespace Unity.Behavior
{
    internal class AddConditionToNodeCommandHandler : CommandHandler<AddConditionToNodeCommand>
    {
        public override bool Process(AddConditionToNodeCommand command)
        {
            ConditionInfo info = ConditionUtility.GetInfoForConditionType(command.Condition.GetType());
            Asset.MarkUndo($"Add a condition {info.Name} to a node");
            BehaviorGraphNodeModel behaviorNodeModel = command.NodeModel as BehaviorGraphNodeModel;

            // Create a new condition model.
            ConditionModel conditionModel = new ConditionModel(behaviorNodeModel, command.Condition, info);

            if (behaviorNodeModel is IConditionalNodeModel conditionalNodeModel)
            {
                conditionalNodeModel.ConditionModels.Add(conditionModel);
            }

            // Have we processed the command and wish to block further processing?
            return false;
        }
    }

}
#endif