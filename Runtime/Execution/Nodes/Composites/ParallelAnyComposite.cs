using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Executes all branches at the same time, stopping if one fails or succeeds.
    /// </summary>
    [Serializable, GeneratePropertyBag]  
    [NodeDescription(
        name: "Run In Parallel Until Any Completes",
        category: "Flow/Parallel Execution",
        description: "Execute all branches at the same time, stopping if one fails or succeeds.",
        icon: "Icons/parallel_any",
        hideInSearch: true,
        id: "e49414e2f12d45efbff56d88f5befb1d")]
    internal partial class ParallelAnyComposite : Composite
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Children.Count == 0)
                return Status.Success;

            for (int i = 0; i < Children.Count; ++i)
            {
                var childStatus = StartNode(Children[i]);
                if (childStatus is Status.Failure or Status.Success)
                {
                    return childStatus;
                }
            }

            return Status.Waiting;
        }

        /// <inheritdoc cref="OnStart" />
        protected override Status OnUpdate()
        {
            for (int i = 0; i < Children.Count; ++i)
            {
                Status childStatus = Children[i].CurrentStatus;
                if (childStatus is Status.Failure or Status.Success)
                {
                    return childStatus;
                }
            }
            
            return Status.Waiting;
        }
    }
    
    /// <summary>
    /// Executes all branches at the same time, stopping if one succeeds.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Run In Parallel Until Any Succeeds",
        category: "Flow/Parallel Execution",
        description: "Execute all branches at the same time, stopping if one succeeds.",
        icon: "Icons/parallel_any",
        hideInSearch: true,
        id: "2e528604708c452babf9c9ce86ca4313")]
    internal partial class ParallelAnySuccess : Composite
    {
        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnStart()
        {
            if (Children.Count == 0)
                return Status.Success;

            int failCount = 0;
            for (int i = 0; i < Children.Count; ++i)
            {
                var childStatus = StartNode(Children[i]);
                if (childStatus is Status.Success)
                {
                    return Status.Success;
                }
                else if (childStatus is Status.Failure)
                {
                    failCount++;
                }
            }

            return failCount == Children.Count ? Status.Failure : Status.Waiting;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            int failCount = 0;
            for (int i = 0; i < Children.Count; ++i)
            {
                Status childStatus = Children[i].CurrentStatus;
                if (childStatus is Status.Success)
                {
                    return Status.Success;
                }
                else if (childStatus is Status.Failure)
                {
                    failCount++;
                }
            }
            return failCount == Children.Count ? Status.Failure : Status.Waiting;
        }
    }
}