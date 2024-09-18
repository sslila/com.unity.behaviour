#if UNITY_EDITOR
using System;
using Unity.Behavior.GraphFramework;
using Unity.Behavior;
using System.Collections.Generic;
using System.Linq;

internal class AddConditionFromSerializedTypeCommandHandler : CommandHandler<AddConditionFromSerializedCommand>
{
    public override bool Process(AddConditionFromSerializedCommand command)
    {
        Type type = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .FirstOrDefault(t => typeof(Condition).IsAssignableFrom(t) && t.Name == command.ConditionType + "Condition");
        
        List<Condition> conditions = ConditionUtility.GetConditions();
        Condition foundCondition = null;
        
        foreach (Condition condition in conditions)
        {
            if (condition.GetType() == type)
            {
                foundCondition = condition;
            }
        }
        if (foundCondition == null)
            return false;
        
        ConditionInfo info = ConditionUtility.GetInfoForConditionType(foundCondition.GetType());

        NodeModel foundNode = null;
        foreach (NodeModel nodeModel in Asset.Nodes)
        {
            if (nodeModel.ID == command.ConditionNodeGUID)
            {
                foundNode = nodeModel;
                break;
            }
        }

        if (foundNode == null)
        {
            return false;
        }
        
        BehaviorGraphNodeModel behaviorNodeModel = foundNode as BehaviorGraphNodeModel;
        if (behaviorNodeModel is IConditionalNodeModel conditionalNodeModel)
        {
            DispatcherContext.GraphView.Dispatcher.DispatchImmediate(new AddConditionToNodeCommand(conditionalNodeModel, foundCondition, true));

            if (DispatcherContext.GraphView is BehaviorGraphView behaviorGraphView)
            {
                behaviorGraphView.RefreshNode(command.ConditionNodeGUID);
            }
        }

        // Have we processed the command and wish to block further processing?
        return true;
    }
}

#endif