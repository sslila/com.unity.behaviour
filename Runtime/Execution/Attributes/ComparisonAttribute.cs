using System;

namespace Unity.Behavior
{
    /// <summary>
    /// The ComparisonAttribute contains metadata for the comparison operator element used in conditions. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ComparisonAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ComparisonAttribute"/> class with the provided metadata.
        /// </summary>
        /// <param name="comparisonType">The comparison type of the comparison operator.</param>
        /// <param name="variable">The variable being compared in the comparison.</param>
        /// <param name="comparisonValue">The comparison value that the variable is being compared to.</param>
        public ComparisonAttribute(ComparisonType comparisonType, string variable = null, string comparisonValue = null)
        {
            ComparisonType = comparisonType;
            Variable = variable;
            ComparisonValue = comparisonValue;
        }
        
        /// <summary>
        /// The comparison type of the comparison operator.
        /// </summary>
        public ComparisonType ComparisonType { get;  }
        
        /// <summary>
        /// The variable being compared in the comparison.
        /// </summary>
        public string Variable { get; }
        
        /// <summary>
        /// The comparison value that the variable is being compared to.
        /// </summary>
        public string ComparisonValue { get; }
    }
}