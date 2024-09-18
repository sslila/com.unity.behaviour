using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Executes branches in order until one fails or all succeed.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Sequence",
        description: "Executes branches in order until one fails or all succeed.", 
        icon: "Icons/Sequence",
        category: "Flow",
        id: "dfd2a5f53dc54b8dad31dc3f7a794079")]
    internal partial class SequenceComposite : Composite
    {
        [CreateProperty] int m_CurrentChild;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            m_CurrentChild = 0;
            if (Children.Count == 0)
                return Status.Success;

            var status = StartNode(Children[0]);
            if (status == Status.Failure)
                return Status.Failure;
            if (status == Status.Success)
                return Status.Running;

            return Status.Waiting;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            var currentChild = Children[m_CurrentChild];
            Status childStatus = currentChild.CurrentStatus;
            if (childStatus == Status.Success)
            {
                if (m_CurrentChild == Children.Count-1)
                    return Status.Success;

                m_CurrentChild++;

                var status = StartNode(Children[m_CurrentChild]);
                if (status == Status.Failure)
                    return Status.Failure;
                if (status == Status.Success)
                    return Status.Running;
            }
            else if (childStatus == Status.Failure)
            {
                return Status.Failure;
            }
            return Status.Waiting;
        }
    }
}
