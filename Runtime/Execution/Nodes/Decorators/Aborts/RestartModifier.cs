using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Restarts child node execution on given conditions.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Restart",
        description: "Restarts branch when assigned conditions are true.", 
        category: "Flow/Abort",
        id: "4d0888f06af04abd987e4b7d61f72e36")]
    internal partial class RestartModifier : Modifier, IConditional
    {
        [SerializeReference]
        protected List<Condition> m_Conditions = new List<Condition>();
        public List<Condition> Conditions { get => m_Conditions; set => m_Conditions = value; }

        [SerializeField]
        protected bool m_RequiresAllConditions;
        public bool RequiresAllConditions { get => m_RequiresAllConditions; set => m_RequiresAllConditions = value; }

        protected override Status OnStart()
        {
            base.OnStart();
            
            if (Child == null)
            {
                return Status.Failure;
            }

            foreach (Condition condition in Conditions)
            {
                condition.OnStart();
            }
            
            Status status = StartNode(Child);
            if (status == Status.Success)
                return Status.Success;
            if (status == Status.Failure)
                return Status.Failure;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // Check the child status 
            Status status = Child.CurrentStatus;
            if (status == Status.Success)
                return Status.Success;
            if (status == Status.Failure)
                return Status.Failure;
            
            // Otherwise, check the conditions and if the branch should be restarted
            if (Conditions.Count == 0)
            {
                return Status.Running;
            }
            if (ConditionUtils.CheckConditions(Conditions, RequiresAllConditions))
            {
                EndNodesAndRestart();
            }

            return Status.Running;
        }

        private void EndNodesAndRestart()
        {
            Graph.EndNode(Child);
            Graph.StartNode(Child);
            
            // Reset the conditions
            foreach (Condition condition in Conditions)
            {
                condition.OnEnd();
                condition.OnStart();
            }
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