#if UNITY_EDITOR
namespace Unity.Behavior
{
    internal static class BehaviorWindowDelegate
    {
        internal delegate void ShowSaveIndicatorHandler(BehaviorAuthoringGraph asset);
        
        internal delegate void OpenHandler(BehaviorAuthoringGraph itemData);
        
        internal static ShowSaveIndicatorHandler showSaveIndicatorHandler; 
        
        internal static OpenHandler openHandler;
        
        public static void ShowSaveIndicator(BehaviorAuthoringGraph asset)
        {
            showSaveIndicatorHandler?.Invoke(asset);
        }

        public static void Open(BehaviorAuthoringGraph itemData)
        {
            openHandler?.Invoke(itemData);
        }
    }
}

#endif