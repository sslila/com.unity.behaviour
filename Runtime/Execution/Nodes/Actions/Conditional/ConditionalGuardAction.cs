using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// Returns Success if the condition evaluates to true and Failure otherwise.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Conditional Guard", 
        description: "Allows flow to pass only if the specified condition(s) are met.", 
        category: "Action/Conditional", 
        id: "d6079f431e4784966a3969d414151638")]
    internal partial class ConditionalGuardAction : Action, IConditional
    {
        [SerializeReference]
        protected List<Condition> m_Conditions = new List<Condition>();
        public List<Condition> Conditions { get => m_Conditions; set => m_Conditions = value; }

        [SerializeField]
        protected bool m_RequiresAllConditions;
        public bool RequiresAllConditions { get => m_RequiresAllConditions; set => m_RequiresAllConditions = value; }

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            foreach (Condition condition in Conditions)
            {
                condition.OnStart();
            }
            
            return ConditionUtils.CheckConditions(Conditions, RequiresAllConditions) ? Status.Success : Status.Failure;
        }
        
        protected override void OnEnd()
        {
            base.OnEnd();

            foreach (Condition condition in Conditions)
            {
                condition.OnEnd();
            }
        }
    }
}
