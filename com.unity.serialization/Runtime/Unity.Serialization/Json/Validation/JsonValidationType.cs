namespace Unity.Behavior.Serialization.Json
{
    /// <summary>
    /// The validation type to use.
    /// </summary>
    internal enum JsonValidationType
    {
        /// <summary>
        /// No validation is performed.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Validation is performed against the standard json spec.
        /// </summary>
        Standard = 1,
        
        /// <summary>
        /// Only structural validation is performed.
        /// </summary>
        Simple = 2
    }
}