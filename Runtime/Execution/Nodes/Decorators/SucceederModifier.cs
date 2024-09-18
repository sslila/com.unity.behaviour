using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Forces success for the child node
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Succeeder", 
        description: "Forces success for the child node.", 
        icon: "Icons/success", 
        id: "2a2fadb041974c9a9bc85921a31f8762")]
    internal partial class SucceederModifier : Modifier
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Child == null)
            {
                return Status.Failure; 
            }
            Status childStatus = StartNode(Child);
            return SucceedIfChildIsComplete(childStatus);
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            return SucceedIfChildIsComplete(Child.CurrentStatus);
        }

        private Status SucceedIfChildIsComplete(Status childStatus)
        {
            if (childStatus is Status.Success or Status.Failure)
            {
                return Status.Success;
            }
            return Status.Waiting;
        }
    }
}