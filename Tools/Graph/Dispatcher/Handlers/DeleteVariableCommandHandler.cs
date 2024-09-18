#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

internal class DeleteVariableCommandHandler : CommandHandler<DeleteVariableCommand>
{
    public override bool Process(DeleteVariableCommand command)
    {
        DispatcherContext.Root.SendEvent(VariableDeletedEvent.GetPooled(DispatcherContext.Root, command.Variable));
        DispatcherContext.BlackboardAsset.Variables.Remove(command.Variable);
        BlackboardAsset.InvokeBlackboardChanged();
        BlackboardView.InitializeListView();
        return true;
    }
}
#endif