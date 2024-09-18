using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Behavior.Serialization.Json.Unsafe
{
    unsafe struct UnsafeView
    {
        [NativeDisableUnsafePtrRestriction] public UnsafePackedBinaryStream* Stream;
        public int TokenIndex;
    }
}