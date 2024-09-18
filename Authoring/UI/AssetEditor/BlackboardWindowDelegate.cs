#if UNITY_EDITOR
namespace Unity.Behavior
{
    internal static class BlackboardWindowDelegate
    {
        public delegate void OpenHandler(BehaviorBlackboardAuthoringAsset itemData);
        public static OpenHandler openHandler;

        public static void Open(BehaviorBlackboardAuthoringAsset itemData)
        {
            openHandler?.Invoke(itemData);
        }
    }
}
#endif