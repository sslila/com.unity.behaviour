using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Find With Tag",
        description: "Finds a GameObject with a tag.",
        story: "Find [Object] with tag: [Tag]",
        category: "Action/Find",
        id: "c83ba0235980a2a0ff12705e1f4fdcea")]
    internal partial class FindObjectWithTagAction : Action
    {
        [SerializeReference] public BlackboardVariableGameObject Object;
        [SerializeReference] public BlackboardVariableString Tag;

        protected override Status OnStart()
        {
            if (Object == null)
            {
                return Status.Failure;
            }

            Object.Value = GameObject.FindGameObjectWithTag(Tag.Value);
            return Object.Value == null ? Status.Failure : Status.Success;
        }
    }
}
