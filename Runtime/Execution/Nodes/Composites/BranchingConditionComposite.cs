using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// Chooses one branch depending if the condition is true or false.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Conditional Branch",
        description: "Redirects the flow to the appropriate branch based on whether the condition is true or false.",
        category: "Flow/Conditional",
        id: "15fd229322992eabc0f7186e51eabcca")]
    internal partial class BranchingConditionComposite : Composite, IConditional
    {
        [SerializeReference]
        protected List<Condition> m_Conditions = new List<Condition>();
        public List<Condition> Conditions { get => m_Conditions; set => m_Conditions = value; }

        [SerializeField]
        protected bool m_RequiresAllConditions;
        public bool RequiresAllConditions { get => m_RequiresAllConditions; set => m_RequiresAllConditions = value; }

        /// <summary>
        /// The node that is executed if the condition is true.
        /// </summary>
        [SerializeReference] public Node True;
        
        /// <summary>
        /// The node that is executed if the condition is false.
        /// </summary>
        [SerializeReference] public Node False;

        private Node m_CurrentChild;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Conditions.Count == 0)
            {
                return Status.Failure;
            }
            
            Status status;

            foreach (Condition condition in Conditions)
            {
                condition.OnStart();
            }
            
            if (ConditionUtils.CheckConditions(Conditions, RequiresAllConditions))
            {
                status = StartNode(True);
                m_CurrentChild = True;
            }
            else
            {
                status = StartNode(False);
                m_CurrentChild = False;
            }

            return status switch
            {
                Status.Success => Status.Success,
                Status.Failure => Status.Failure,
                _ => Status.Waiting
            };
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            return m_CurrentChild.CurrentStatus;
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