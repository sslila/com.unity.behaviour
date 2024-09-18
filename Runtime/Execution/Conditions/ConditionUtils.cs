using System;
using System.Collections.Generic;

namespace Unity.Behavior
{
    /// <summary>
    /// A utility class for conditions used in Conditional nodes.
    /// </summary>
    public static class ConditionUtils
    {
        /// <summary>
        /// Evaluates the operation from a condition.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <param name="leftOperand">The left side value that is being compared.</param>
        /// <param name="conditionOperator">The condition operator.</param>
        /// <param name="rightOperand">The right side value that is being compared.</param>
        public static bool Evaluate(object leftOperand,
            BlackboardVariable<ConditionOperator> conditionOperator,
            object rightOperand)
        {
            if (leftOperand == null)
                return false;

            // If both operands are Blackboard variables.
            if (leftOperand is BlackboardVariable variable && rightOperand is BlackboardVariable comparisonValue)
            {
                if (variable.ObjectValue is IComparable)
                {
                    return CompareValues((IComparable)variable.ObjectValue, (IComparable)comparisonValue.ObjectValue,
                        conditionOperator);   
                }

                return CompareReferences(variable.ObjectValue, comparisonValue?.ObjectValue, conditionOperator);
            }

            // If left operand is a Blackboard variable but the right operand is not.
            if (leftOperand is BlackboardVariable leftVar && rightOperand is IComparable)
            {
                if (leftVar.ObjectValue is IComparable)
                {
                    return CompareValues((IComparable)leftVar.ObjectValue, (IComparable)rightOperand,
                        conditionOperator);
                }
                
                return CompareReferences(leftVar.ObjectValue, rightOperand, conditionOperator);
            }
            
            // If right operand is a Blackboard variable but the left operand is not.
            if (leftOperand is IComparable && rightOperand is BlackboardVariable rightVar)
            {
                if (rightVar.ObjectValue is IComparable)
                {
                    return CompareValues( (IComparable)leftOperand, (IComparable)rightVar.ObjectValue,
                        conditionOperator);
                }
                
                return CompareReferences( leftOperand, rightVar.ObjectValue, conditionOperator);
            }

            // If neither of comparison values are Blackboard variables.
            if (leftOperand is IComparable && rightOperand is IComparable)
            {
                return CompareValues((IComparable)leftOperand, (IComparable)rightOperand,
                    conditionOperator);   
            }

            return CompareReferences(leftOperand, rightOperand, conditionOperator);
        }

        private static bool CompareReferences(object left, object right, Enum conditionOperator)
        {
            return conditionOperator switch
            {
                ConditionOperator.Equal => left == right,
                ConditionOperator.NotEqual => left != null ,
                _ => false
            };
        }

        private static bool CompareValues(IComparable left, IComparable right, Enum conditionOperator)
        {
            int comparison = left.CompareTo(right);
            return conditionOperator switch
            {
                ConditionOperator.Equal => comparison == 0,
                ConditionOperator.NotEqual => comparison != 0,
                ConditionOperator.Greater => comparison > 0,
                ConditionOperator.Lower => comparison < 0,
                ConditionOperator.GreaterOrEqual => comparison >= 0,
                ConditionOperator.LowerOrEqual => comparison <= 0,
                _ => false
            };
        }

        internal static bool CheckConditions(IEnumerable<Condition> conditions, bool allRequired)
        {
            if (!allRequired)
            {
                foreach (Condition condition in conditions)
                {
                    if (condition.IsTrue())
                    {
                        return true;   
                    }
                }
        
                return false;
            }
            
            foreach (Condition condition in conditions)
            {
                if (!condition.IsTrue())
                    return false;
            }
        
            return true;
        }
    }
}