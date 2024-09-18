using UnityEditor;

namespace Unity.Behavior.Serialization.Editor
{
    static class DefaultEditorPropertyBags
    {
        [InitializeOnLoadMethod]
        internal static void Initialize()
        {
            DefaultPropertyBagInitializer.Initialize();
        }
    }
}