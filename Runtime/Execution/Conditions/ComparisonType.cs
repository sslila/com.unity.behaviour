namespace Unity.Behavior
{
    /// <summary>
    /// The comparison type of Comparison Operator.
    /// </summary>
    public enum ComparisonType
    {
        /// <summary> The comparison includes all supported variable types.</summary>
        All,
        /// <summary> The comparison is strictly a boolean comparison.</summary>
        Boolean,
        /// <summary> The comparison is comparing between two Blackboard Variables.</summary>
        BlackboardVariables
    }
}