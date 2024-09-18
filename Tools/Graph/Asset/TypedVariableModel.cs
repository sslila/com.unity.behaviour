using System;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    /// <summary>
    /// Model to represent a typed variable.
    /// </summary>
    /// <typeparam name="T">The type of the variable.</typeparam>
    [Serializable]
    public class TypedVariableModel<T> : VariableModel
    {
        /// <summary>
        /// The variables value, of type T.
        /// </summary>
        [SerializeField]
        public T m_Value;

        /// <summary>
        /// Sets or gets the variable's value.
        /// </summary>
        public override object ObjectValue { get => m_Value; set => m_Value = value == null ? default : (T)value; }

        /// <summary>
        /// Gets the type of the variable.
        /// </summary>
        public override Type Type => typeof(T);
    }
}