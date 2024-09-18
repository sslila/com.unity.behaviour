using System;

namespace Unity.Behavior.GraphFramework
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class NodeUIAttribute : BaseUIAttribute
    {
        public NodeUIAttribute(Type nodeModelType) : base(nodeModelType)
        {
        }
    }
}