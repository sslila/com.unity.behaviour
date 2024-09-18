using System;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    [Condition(
        name: "Variable Value Changed",
        category: "Variable Conditions",
        story: "[Variable] has changed",
        id: "81244bae408bf0ba83e9723fe4be4299")]
    internal class VariableValueChangedCondition : Condition
    {
        [SerializeReference] public BlackboardVariable Variable;
        private bool HasVariableChanged;

        public override bool IsTrue()
        {
            if (!HasVariableChanged)
            {
                return false;
            }

            HasVariableChanged = false;
            return true;
        }

        public override void OnStart()
        {
            if (Variable == null)
            {
                return;
            }

            Variable.OnValueChanged -= OnVariableValueChange;
            Variable.OnValueChanged += OnVariableValueChange;
        }

        public override void OnEnd()
        {
            Variable.OnValueChanged -= OnVariableValueChange;
        }

        private void OnVariableValueChange()
        {
            HasVariableChanged = true;
        }
    }
}
