using System;
using UnityEngine;

namespace Unity.Behavior
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class NodeModelInfoAttribute : Attribute
    {
        internal Type Type { get; }

        public NodeModelInfoAttribute(Type modelType)
        {
            Type = modelType;
        }
    }
}