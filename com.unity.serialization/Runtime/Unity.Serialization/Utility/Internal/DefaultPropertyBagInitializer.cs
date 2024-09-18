using Unity.Properties;
using Unity.Behavior.Serialization.Json;
using UnityEngine;

namespace Unity.Behavior.Serialization
{
    [UnityEngine.Scripting.Preserve]
    class DefaultPropertyBagInitializer
    {
        [RuntimeInitializeOnLoadMethod]
        internal static void Initialize()
        {
            PropertyBag.Register(new Json.SerializedObjectViewPropertyBag());
            PropertyBag.Register(new Json.SerializedArrayViewPropertyBag());
            
            UnsafeSerializedObjectReader.CreateBurstDelegates();
        }
    }
}
