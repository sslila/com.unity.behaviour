using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Switch",
        description: "Branches off based on enum value.", 
        icon: "Icons/Sequence",
        category: "Flow/Conditional",
        id: "ef072beedcccd16ac0cd3cb5295fe4cd")]
    internal partial class SwitchComposite : Composite
    {
        // The returned status when no node is attached to the desired port.
        internal Status DefaultStatus = Status.Success;

        [SerializeReference] public BlackboardVariable EnumVariable;

        private int m_CurrentChild = -1;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            if (Children.Count == 0)
            {
                return Status.Success;
            }

            m_CurrentChild = Array.IndexOf(Enum.GetValues(EnumVariable.ObjectValue.GetType()), EnumVariable.ObjectValue);

            if (m_CurrentChild >= Children.Count || m_CurrentChild < 0)
            {
                Debug.LogError(
                    $"The value '{EnumVariable.ObjectValue.ToString()}' is not available as an output port. This can happen when you manually changed the enum type '{EnumVariable.Type}' and forgot to recompile the graph.");
                return DefaultStatus;
            }

            Node child = Children[m_CurrentChild];
            if (child == null)
            {
                Debug.LogWarning($"The current enum value '{EnumVariable.ObjectValue}' is not connected to a child node.");
                return DefaultStatus;
            }

            Status status = StartNode(child);
            return status switch
            {
                Status.Success => Status.Success,
                Status.Failure => Status.Failure,
                _ => Status.Waiting
            };
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            return Children[m_CurrentChild].CurrentStatus;
        }

        /// <inheritdoc cref="ResetStatus" />
        protected internal override void ResetStatus()
        {
            CurrentStatus = Status.Uninitialized;
            foreach (var child in NullFreeChildren())
            {
                child.ResetStatus();
            }
        }

        private IEnumerable<Node> NullFreeChildren() => Children.Where(child => child != null);
    }
}
