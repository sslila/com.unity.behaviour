using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [GeneratePropertyBag, NodeDescription(
        name: "Run Subgraph",
        description: "Runs the assigned subgraph and returns the graph's final status.",
        category: "Subgraphs",
        id: "99ca68fd9e704c8abdaacf6697e42a4a")]
    internal partial class RunSubgraph : Action
    {
        [SerializeReference] public BehaviorGraphModule Subgraph;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Subgraph?.Root == null)
            {
                return Status.Failure;
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
            if (Subgraph?.Root != null)
            {
                Subgraph.EndNode(Subgraph.Root);
            }
        }
    }
}