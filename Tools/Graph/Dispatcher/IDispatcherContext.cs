#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal interface IDispatcherContext
    {
        public BlackboardView BlackboardView { get; }
        public GraphEditor GraphEditor { get; }
        public GraphView GraphView { get;  }
        public GraphAsset GraphAsset { get; }
        public BlackboardAsset BlackboardAsset { get; }
        public VisualElement Root { get; }
    }
}
#endif