using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;

namespace Unity.Behavior.Serialization.Binary
{
    static unsafe partial class BinarySerialization
    {
        /// <summary>
        /// Deserializes from the specified stream and returns a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="parameters">The parameters to use when reading.</param>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/> constructed from the serialized data.</returns>
        internal static T FromBinary<T>(UnsafeAppendBuffer.Reader* stream, BinarySerializationParameters parameters = default)
        {
            var state = parameters.State ?? (parameters.RequiresThreadSafety ? new BinarySerializationState() : GetSharedState());
            var visitor = state.GetBinaryPropertyReader();
            
            visitor.SetStream(stream);
            visitor.SetSerializedType(parameters.SerializedType);
            visitor.SetDisableRootAdapters(parameters.DisableRootAdapters);
            visitor.SetGlobalAdapters(GetGlobalAdapters());
            visitor.SetUserDefinedAdapters(parameters.UserDefinedAdapters);
            visitor.SetSerializedReferences(parameters.DisableSerializedReferences ? default : state.GetSerializedReferences());
            
            var container = new PropertyWrapper<T>(default);
            using (visitor.Lock()) PropertyContainer.Accept(visitor, ref container);
            
            if (null == parameters.State && !parameters.DisableSerializedReferences) state.GetSerializedReferences().Clear();
            
            return container.Value;
        }
    }
}
