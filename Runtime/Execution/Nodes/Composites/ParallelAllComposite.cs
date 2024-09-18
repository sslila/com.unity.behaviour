using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Executes all branches at the same time.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Run In Parallel",
        category: "Flow/Parallel Execution",
        description: "Executes all branches at the same time.",
        icon: "Icons/parallel_all",
        id: "eff83f53d0984556bad3f4cbc8ff681e")]
    internal partial class ParallelAllComposite : Composite
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            bool shouldWait = false;
            int successCount = 0;
            for (int i = 0; i < Children.Count; ++i)
            {
                var childStatus = StartNode(Children[i]);
                if (childStatus is Status.Running or Status.Waiting)
                {
                    shouldWait = true;
                }
                else if (childStatus is Status.Success)
                {
                    successCount++;
                }
            }

            if (shouldWait)
            {
                return Status.Waiting;
            }

            return successCount == Children.Count ? Status.Success : Status.Failure;
        }

        /// <inheritdoc cref="OnStart" />
        protected override Status OnUpdate()
        {
            int successCount = 0;
            for (int i = 0; i < Children.Count; ++i)
            {
                var childStatus = Children[i].CurrentStatus;
                if (childStatus is Status.Running or Status.Waiting)
                {
                    return Status.Waiting;
                }
                else if (childStatus is Status.Success)
                {
                    successCount++;
                }
            }

            return successCount == Children.Count ? Status.Success : Status.Failure;
        }
    }

    /// <summary>
    /// Executes all branches at the same time, stopping if one fails.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Run In Parallel Until Any Fails",
        category: "Flow/Parallel Execution",
        description: "Execute all branches at the same time, stopping if one fails.",
        hideInSearch: true,
        icon: "Icons/parallel_all",
        id: "14a266d5d02d4c67a7940885be9078e8")]
    public partial class ParallelAllSuccess : Composite
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            bool shouldWait = false;
            for (int i = 0; i < Children.Count; ++i)
            {
                var childStatus = StartNode(Children[i]);
                if (childStatus is Status.Running or Status.Waiting)
                {
                    shouldWait = true;
                }
                else if (childStatus is Status.Failure)
                {
                    return Status.Failure; // early termination
                }
            }

            if (shouldWait)
                return Status.Waiting;
            else
                return Status.Success;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            bool shouldWait = false;
            for (int i = 0; i < Children.Count; ++i)
            {
                var childStatus = Children[i].CurrentStatus;
                if (childStatus is Status.Running or Status.Waiting)
                {
                    shouldWait = true;
                }
                else if (childStatus is Status.Failure)
                {
                    return Status.Failure; // early termination
                }
            }

            if (shouldWait)
                return Status.Waiting;
            else
                return Status.Success;
        }
    }
}
