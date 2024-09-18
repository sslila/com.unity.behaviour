using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Attach Object",
        story: "Attach [Object] to [Target]",
        category: "Action/GameObject",
        description: "Attaches a GameObject to another GameObject with an offset.\n\n" +
        "This is done by assigning the GameObject's transform parent to the target.",
        id: "6ef37426dfc4b8cc84b6f36507e03824")]
    internal partial class AttachObjectAction : Action
    {
        [SerializeReference] public BlackboardVariableTransform Object;
        [Tooltip("If no target is provided, set the parent transform of the GameObject to null.")]
        [SerializeReference] public BlackboardVariableTransform Target;
        [SerializeReference] public BlackboardVariableVector3 LocalPosition;

        protected override Status OnStart()
        {
            if (Object.Value == null)
            {
                LogFailure("No object to be attached provided.");
                return Status.Failure;
            }

            if (Target.Value == null)
            {
                Object.Value.parent = null;
            }
            else
            {
                Object.Value.parent = Target.Value.transform;
                Object.Value.localPosition = LocalPosition.Value;
            }

            return Status.Success;
        }
    }
}
