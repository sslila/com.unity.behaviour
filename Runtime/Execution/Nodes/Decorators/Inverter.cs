using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Inverts the result of the child action.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Inverter", 
        description: "Inverts the result of the child action.", 
        icon: "Icons/inverter", 
        id: "d0d10b1d45144d1b802b9443b57b5adb")]
    internal partial class InverterModifier : Modifier
    {
        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Child == null)
            {
                return Status.Failure;
            }
            
            Status status = StartNode(Child);
            if (status == Status.Success)
                return Status.Failure;
            if (status == Status.Failure)
                return Status.Success;
            return Status.Waiting;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            Status status = Child.CurrentStatus;
            if (status == Status.Success)
                return Status.Failure;
            if (status == Status.Failure)
                return Status.Success;
            return Status.Waiting;
        }
    }
}