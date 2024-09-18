using System;
using Unity.Behavior.GenericSerializeReference;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [Condition(
        name: "Variable Comparison",
        category: "Variable Conditions",
        story: "[Variable] is [Operator] [ComparisonValue]",
        id: "a57e6f54cc9d4f41a49bc935222e0710")]
    internal class VariableComparisonCondition : Condition
    {
        /// <summary>
        /// The blackboard variable that is being compared.
        /// </summary>
        [SerializeReference] public BlackboardVariable Variable;

        /// <summary>
        /// The condition operator that is used to compare the values.
        /// </summary>
        [Comparison(comparisonType: ComparisonType.BlackboardVariables, variable: "Variable", comparisonValue: "ComparisonValue")]
        [SerializeReference] public BlackboardVariableConditionOperator Operator;

        /// <summary>
        /// The value that the variable is being compared to.
        /// </summary>
        [SerializeReference] public BlackboardVariable ComparisonValue;

        public override bool IsTrue()
        {
            if (Variable == null)
            {
                return false;
            }

            return ConditionUtils.Evaluate(Variable, Operator, ComparisonValue);
        }
    }
}
