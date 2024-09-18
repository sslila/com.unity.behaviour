using System;
using Unity.Behavior.GenericSerializeReference;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [Condition(
        name: "Check Distance",
        category: "Conditions",
        story: "distance between [Transform] and [Target] [Operator] [Threshold]",
        id: "a7f68739e880f2880a0c8b3df7c50061")]
    internal class CheckDistanceCondition : Condition
    {
        [SerializeReference] public BlackboardVariableTransform Transform;
        [SerializeReference] public BlackboardVariableTransform Target;
        [Comparison(comparisonType: ComparisonType.All)]
        [SerializeReference] public BlackboardVariableConditionOperator Operator;
        [SerializeReference] public BlackboardVariableFloat Threshold;

        public override bool IsTrue()
        {
            if (Transform.Value == null || Target.Value == null)
            {
                return false;
            }

            float distance = Vector3.Distance(Transform.Value.position, Target.Value.position);
            return ConditionUtils.Evaluate(distance, Operator, Threshold.Value);
        }
    }
}
