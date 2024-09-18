#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class DuplicateNodeCommandHandler : DuplicateNodeBaseCommandHandler<DuplicateNodeCommand>
    {
        public override bool Process(DuplicateNodeCommand command)
        {
            PastePosition = command.Position;
            return base.Process(command);
        }
    }
}
#endif