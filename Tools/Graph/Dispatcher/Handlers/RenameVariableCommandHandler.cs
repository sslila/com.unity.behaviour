#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

internal class RenameVariableCommandHandler : CommandHandler<RenameVariableCommand>
{
    public override bool Process(RenameVariableCommand command)
    {
        command.Variable.Name = command.NewName;
        DispatcherContext.Root.SendEvent(VariableRenamedEvent.GetPooled(DispatcherContext.Root, command.Variable));
        BlackboardAsset.InvokeBlackboardChanged();
        return true;
    }
}
#endif