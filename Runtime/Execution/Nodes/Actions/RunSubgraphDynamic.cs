using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior
{
    internal partial class RunSubgraphDynamic : Action
    {
        [SerializeReference] public BlackboardVariable<BehaviorGraph> SubgraphVariable;
        public BehaviorGraphModule Subgraph => SubgraphVariable.Value.RootGraph;
        [SerializeReference] public RuntimeBlackboardAsset RequiredBlackboard;
        [SerializeReference] public List<DynamicBlackboardVariableOverride> DynamicOverrides;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            SubgraphVariable.OnValueChanged += OnSubgraphChanged;
            
            if (SubgraphVariable?.ObjectValue == null)
            {
                return Status.Failure;
            }

            if (Subgraph?.Root == null)
            {
                return Status.Failure;
            }

            if (GameObject != null)
            {
                BehaviorGraphAgent agent = GameObject.GetComponent<BehaviorGraphAgent>();
                if (agent != null)
                {
                    BehaviorGraph graph = agent.Graph;
                    if (graph != null && SubgraphVariable.Value == graph)
                    {
                        Debug.LogWarning($"Running {SubgraphVariable.Value.name} will create a cycle and can not be used as subgraph for {graph}. Select a different graph to run dynamically.");
                        return Status.Failure;
                    }   
                }   
            }

            if (RequiredBlackboard != null)
            {
                TrySetVariablesOnSubgraph();
            }

            return Subgraph.StartNode(Subgraph.Root) switch
            {
                Status.Success => Status.Success,
                Status.Failure => Status.Failure,
                _ => Status.Running,
            };
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            Subgraph.Tick();
            return Subgraph.Root.CurrentStatus switch
            {
                Status.Success => Status.Success,
                Status.Failure => Status.Failure,
                _ => Status.Running,
            };
        }

        /// <inheritdoc cref="OnEnd" />
        protected override void OnEnd()
        {
            SubgraphVariable.OnValueChanged -= OnSubgraphChanged;
            
            if (SubgraphVariable.ObjectValue == null)
            {
                return;
            }

            if (Subgraph?.Root != null)
            {
                Subgraph.EndNode(Subgraph.Root);
            }
        }

        private void OnSubgraphChanged()
        {
            if (Subgraph != null)
            {
                SubgraphVariable.OnValueChanged -= OnSubgraphChanged;
                TrySetVariablesOnSubgraph();
                StartNode(this);
            }
        }

        private void TrySetVariablesOnSubgraph()
        {
            if (RequiredBlackboard == null || DynamicOverrides == null)
            {
                return;
            }

            bool matchingBlackboard = false;

            foreach (BlackboardReference reference in Subgraph.BlackboardGroupReferences)
            {
                if (reference.SourceBlackboardAsset.AssetID != RequiredBlackboard.AssetID)
                {
                    continue;
                }

                foreach (DynamicBlackboardVariableOverride dynamicOverride in DynamicOverrides)
                {
                    foreach (BlackboardVariable variable in reference.Blackboard.Variables)
                    {
                        if (variable.Name != dynamicOverride.Name || variable.Type != dynamicOverride.Variable.Type)
                        {
                            continue;
                        }

                        variable.ObjectValue = dynamicOverride.Variable.ObjectValue;

                        // If the variable is a Blackboard Variable and not a local value assigned from the Inspector.
                        if (string.IsNullOrEmpty(dynamicOverride.Variable.Name))
                        {
                            continue;
                        }

                        if (variable.GUID == dynamicOverride.Variable.GUID)
                        {
                            continue;
                        }

                        variable.OnValueChanged += () =>
                        {
                            // Update the original assigned variable if it has been modified in the subgraph.
                            dynamicOverride.Variable.ObjectValue = variable.ObjectValue;
                        };
                    }
                }

                matchingBlackboard = true;
            }

            if (!matchingBlackboard)
            {
                Debug.LogWarning($"No matching Blackboard of type {RequiredBlackboard.name} found for graph {SubgraphVariable.Value.name}. Any assigned variables will not be set.");
            }
        }
    }
}