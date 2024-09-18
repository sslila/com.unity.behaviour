using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using Unity.Behavior.Serialization.Json;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// Contains all interfaces and helper objects for runtime serialization of behavior graphs.
    /// </summary>
    public class RuntimeSerializationUtility
    {
        /// <summary>
        /// Interface for all object resolvers. During runtime serialization, an object resolver is required to convert
        /// an object to and from a unique ID.
        /// </summary>
        /// <typeparam name="TSerializedFormat">Serialized output type.</typeparam>
        public interface IUnityObjectResolver<TSerializedFormat>
        {
            /// <summary>
            /// Converts an object to a serializable id.
            /// </summary>
            /// <param name="obj">Object to convert.</param>
            /// <returns>Serializable ID.</returns>
            TSerializedFormat Map(UnityEngine.Object obj);
            
            /// <summary>
            /// Used to convert a serializable ID to an object.
            /// </summary>
            /// <param name="mappedValue">Serializable ID.</param>
            /// <typeparam name="TSerializedType">Serialized ID type.</typeparam>
            /// <returns>The object created for the serialized ID.</returns>
            TSerializedType Resolve<TSerializedType>(TSerializedFormat mappedValue) where TSerializedType : UnityEngine.Object;
        }

        /// <summary>
        /// Interface for behavior serializor implementations.
        /// </summary>
        /// <typeparam name="TSerializedFormat">Serialized output type.</typeparam>
        public interface IBehaviorSerializer<TSerializedFormat>
        {
            /// <summary>
            /// Serializes a BehaviorGraph into.
            /// </summary>
            /// <param name="graph">The Graph to serialize.</param>
            /// <param name="resolver">The object resolver implementation to use.</param>
            /// <returns>The serialized output</returns>
            TSerializedFormat Serialize(BehaviorGraph graph, IUnityObjectResolver<string> resolver);
            
            /// <summary>
            /// Deserializes a BehaviorGraph on to a graph object.
            /// </summary>
            /// <param name="serialized">Serialized data to be deserialized.</param>
            /// <param name="graph">BehaviorGraph to be updated.</param>
            /// <param name="resolver">The object resolver implementation to use.</param>
            void Deserialize(TSerializedFormat serialized, BehaviorGraph graph, IUnityObjectResolver<string> resolver);
        }

        /// <summary>
        /// Implementation fo JSON serializor.
        /// </summary>
        public class JsonBehaviorSerializer : IBehaviorSerializer<string>
        {
            private static GameObjectAdapter s_GameObjectAdapter;
            private static ComponentAdapter s_ComponentAdapter;

            private static JsonSerializationParameters s_JsonPackageSerializationParameters =
                new JsonSerializationParameters
                {
                    Indent = 4,
                    DisableRootAdapters = true,
                    UserDefinedAdapters = new List<IJsonAdapter>
                    {
                        (s_GameObjectAdapter = new GameObjectAdapter()),
                        (s_ComponentAdapter = new ComponentAdapter()),
                        new SerializableGUIDAdapter(),
                    }
                };

            private class SerializableGUIDAdapter : IJsonAdapter<SerializableGUID>
            {
                public void Serialize(in JsonSerializationContext<SerializableGUID> context, SerializableGUID value) =>
                    context.SerializeValue(value.Valid ? value.ToString() : string.Empty);

                public SerializableGUID Deserialize(in JsonDeserializationContext<SerializableGUID> context) =>
                    new(context.SerializedValue.AsStringView().ToString());
            }

            private class ComponentAdapter : IJsonAdapter<Component>
            {
                public IUnityObjectResolver<string> Resolver;

                public void Serialize(in JsonSerializationContext<Component> context, Component value) =>
                    context.SerializeValue(Resolver.Map(value));

                public Component Deserialize(in JsonDeserializationContext<Component> context)
                {
                    var name = context.DeserializeValue<string>(context.SerializedValue);
                    return name == null ? null : Resolver.Resolve<Component>(name);
                }
            }
            private class GameObjectAdapter : IJsonAdapter<GameObject>
            {
                public IUnityObjectResolver<string> Resolver;

                public void Serialize(in JsonSerializationContext<GameObject> context, GameObject value) =>
                    context.SerializeValue(Resolver.Map(value));

                public GameObject Deserialize(in JsonDeserializationContext<GameObject> context)
                {
                    var name = context.DeserializeValue<string>(context.SerializedValue);
                    return name == null ? null : Resolver.Resolve<GameObject>(name);
                }
            }
            
            /// <summary>
            /// Serializes a BehaviorGraph into JSON.
            /// </summary>
            /// <param name="graph">The Graph to serialize.</param>
            /// <param name="resolver">The object resolver implementation to use.</param>
            /// <returns>The serialized output</returns>
            public string Serialize(BehaviorGraph graph, IUnityObjectResolver<string> resolver)
            {
                s_GameObjectAdapter.Resolver = resolver;
                s_ComponentAdapter.Resolver = resolver;
                return JsonSerialization.ToJson(graph, s_JsonPackageSerializationParameters);
            }

            /// <summary>
            /// Deserializes JSON to a BehaviorGraph.
            /// </summary>
            /// <param name="graphJson">Serialized data to be deserialized.</param>
            /// <param name="graph">BehaviorGraph to be updated.</param>
            /// <param name="resolver">The object resolver implementation to use.</param>
            public void Deserialize(string graphJson, BehaviorGraph graph, IUnityObjectResolver<string> resolver)
            {
                s_GameObjectAdapter.Resolver = resolver;
                s_ComponentAdapter.Resolver = resolver;
                JsonSerialization.FromJsonOverride(graphJson, ref graph, s_JsonPackageSerializationParameters);
            }
        }
    }
}