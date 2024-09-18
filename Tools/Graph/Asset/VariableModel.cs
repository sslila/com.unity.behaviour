using System;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    /// <summary>
    /// Data model representing a variable.
    /// </summary>
    [Serializable]
    public class VariableModel : IEquatable<VariableModel>
    {
        /// <summary>
        /// ID of the variable.
        /// </summary>
        [SerializeField]
        public SerializableGUID ID = SerializableGUID.Generate();

        /// <summary>
        /// Name of the variable.
        /// </summary>
        [SerializeField]
        public string Name;

        /// <summary>
        /// Boolean indicating whether the variable is exposed.
        /// </summary>
        [SerializeField]
        public bool IsExposed = true;

        /// <summary>
        /// Bool indicating whether the variable is shared.
        /// </summary>
        [SerializeField]
        public bool IsShared = false;

        /// <summary>
        /// Interface for getting or setting the value of the variable.
        /// </summary>
        public virtual object ObjectValue { get; set; }
        /// <summary>
        /// Interface for getting the type of the variable.
        /// </summary>
        public virtual Type Type { get; }

        /// <summary>
        /// Base method allowing variable models to be validated.
        /// </summary>
        public virtual void OnValidate() { }
        
        /// <summary>
        /// Get's the hash code of the variable ID.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => ID.GetHashCode();
        
        /// <summary>
        /// Compares an object to this variable model, casts to VariableModel and invokes an id and type check.
        /// </summary>
        /// <param name="other">Object to compare.</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object other) => Equals(other as VariableModel);
        
        /// <summary>
        /// Compares two VariableModels for equality of ID and Type.
        /// </summary>
        /// <param name="other">VariableModel to compare to.</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(VariableModel other) => other != null && other.ID == ID && other.Type == Type;
        /// <summary>
        /// Compares two VariableModels for equality of ID and Type.
        /// </summary>
        /// <param name="a">First to compare.</param>
        /// <param name="b">Second to compare.</param>
        /// <returns>True if equal, false otherwise</returns>
        public static bool operator ==(VariableModel a, VariableModel b) => a?.Equals(b) ?? b is null;
        /// <summary>
        /// Compares two VariableModels for oinequality of ID and Type.
        /// </summary>
        /// <param name="a">First to compare.</param>
        /// <param name="b">Second to compare.</param>
        /// <returns>True if not equal, false otherwise.</returns>
        public static bool operator !=(VariableModel a, VariableModel b) => !(a == b);
    }
}