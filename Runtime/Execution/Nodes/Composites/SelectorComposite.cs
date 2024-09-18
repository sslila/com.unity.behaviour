using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Executes branches in order until one succeeds.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Try In Order",
        description: "Executes branches in order until one succeeds.", 
        icon: "Icons/selector",
        category: "Flow",
        id: "2bdfd1f8aaec469f8df1fd3190d7466b")]
    internal partial class SelectorComposite : Composite
    {
        [CreateProperty] private int m_CurrentChild;

        protected override Status OnStart()
        {
            m_CurrentChild = 0;
            if (Children.Count == 0)
                return Status.Success;

            var status = StartNode(Children[m_CurrentChild]);
            if (status == Status.Success)
                return Status.Success;
            if (status == Status.Failure)
                return Status.Running;

            return Status.Waiting;
        }

        protected override Status OnUpdate()
        {
            if (m_CurrentChild >= Children.Count)
                return Status.Success;

            Status childStatus = Children[m_CurrentChild].CurrentStatus;
            if (childStatus == Status.Success)
            {
                ++m_CurrentChild;
                return Status.Success;
            }
            else if (childStatus == Status.Failure)
            {
                if (++m_CurrentChild >= Children.Count)
                    return Status.Failure;

                var status = StartNode(Children[m_CurrentChild]);
                if (status == Status.Success)
                    return Status.Success;
                if (status == Status.Failure)
                    return Status.Running;
            }
            return Status.Waiting;
        }
    }
}
