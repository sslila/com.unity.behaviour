#if UNITY_EDITOR
namespace Unity.Behavior.GraphFramework
{
    internal class PasteNodeCommandHandler : DuplicateNodeBaseCommandHandler<PasteNodeCommand>
    {
        public override bool Process(PasteNodeCommand command)
        {
            PastePosition = command.Position;
            return base.Process(command);
        }
    }
}
#endif