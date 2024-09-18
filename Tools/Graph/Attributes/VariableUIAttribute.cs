using System;

namespace Unity.Behavior.GraphFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class VariableUIAttribute : BaseUIAttribute
    {
        public VariableUIAttribute(Type variableModelType) : base(variableModelType)
        {
        }
    }
}