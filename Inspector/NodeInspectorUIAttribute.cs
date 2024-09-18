using System;

namespace Unity.Behavior.GraphFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class NodeInspectorUIAttribute : BaseUIAttribute
    {
        public NodeInspectorUIAttribute(Type nodeModelType) : base(nodeModelType) { }
    }
}