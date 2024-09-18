using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Don't Destroy On Load",
        story: "Don't Destroy [Target] On Load",
        category: "Action/GameObject",
        description: "Prevents a GameObject from being destroyed on load.",
        id: "7c5cbc302a5b27b1126db802cdc7c965")]
    internal partial class DontDestroyOnLoadAction : Action
    {
        [SerializeReference] public BlackboardVariableGameObject Target;

        protected override Status OnStart()
        {
            if (Target.Value == null)
            {
                LogFailure("No target assigned.");
                return Status.Failure;
            }

            GameObject.DontDestroyOnLoad(Target.Value);
            return Status.Success;
        }
    }
}
