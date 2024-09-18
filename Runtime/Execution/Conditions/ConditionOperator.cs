namespace Unity.Behavior
{
    /// <summary>
    /// Represents the different operators that can be used to compare values.
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>
        /// Compare the values for equality.
        /// </summary>
        Equal,
        /// <summary>
        /// Compare the values for inequality.
        /// </summary>
        NotEqual,
        /// <summary>
        /// Compare the values for greater than.
        /// </summary>
        Greater,
        /// <summary>
        /// Compare the values for less than.
        /// </summary>
        Lower,
        /// <summary>
        /// Compare the values for greater than or equal.
        /// </summary>
        GreaterOrEqual,
        /// <summary>
        /// Compare the values for less than or equal.
        /// </summary>
        LowerOrEqual
    }
    
    internal enum BooleanOperator
    {
    }
}