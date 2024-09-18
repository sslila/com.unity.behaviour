using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Repeats operation of the node until failure.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Repeat Until Failure",
        description: "Repeats until failure", 
        category: "Flow/Repeat",
        hideInSearch: true,
        icon: "Icons/repeat_until_change",
        id: "7483f88afa3148a1a565f17863ebd38d")]
    internal partial class RepeatUntilFailModifier : Modifier
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Child == null)
            {
                return Status.Failure;
            }
            
            Status childStatus = StartNode(Child);
            return GetReturnStatusForStartingChild(childStatus);
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            Status childStatus = Child.CurrentStatus;
            if (childStatus is Status.Success)
            {
                childStatus = StartNode(Child);
                return GetReturnStatusForStartingChild(childStatus);
            }
            else if (childStatus is Status.Failure)
            {
                return Status.Failure;
            }
            
            return Status.Waiting;
        }

        Status GetReturnStatusForStartingChild(Status childStatus)
        {
            switch (childStatus)
            {
                case Status.Failure: return Status.Success;
                case Status.Success: return Status.Running;
                default: return Status.Waiting;
            }
        }
    }
    
    /// <summary>
    /// Repeats operation of the node until success.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Repeat Until Success",
        description: "Repeats until success", 
        category: "Flow/Repeat",
        icon: "Icons/repeat_until_change",
        hideInSearch: true,
        id: "ab0357ae774a43b380ba36fad08dcec4")]
    public partial class RepeatUntilSuccessModifier : Modifier
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Child == null)
            {
                return Status.Failure;
            }
            
            Status childStatus = StartNode(Child);
            return GetReturnStatusForStartingChild(childStatus);
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            Status childStatus = Child.CurrentStatus;
            if (childStatus is Status.Failure)
            {
                childStatus = StartNode(Child);
                return GetReturnStatusForStartingChild(childStatus);
            }
            else if (childStatus is Status.Success)
            {
                return Status.Success;
            }
            return Status.Waiting;
        }

        Status GetReturnStatusForStartingChild(Status childStatus)
        {
            switch (childStatus)
            {
                case Status.Success: return Status.Success;
                case Status.Failure: return Status.Running;
                default: return Status.Waiting;
            }
        }
    }
}