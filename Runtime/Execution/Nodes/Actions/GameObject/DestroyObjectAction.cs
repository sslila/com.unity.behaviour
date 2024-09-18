using System;
using Unity.Behavior;
using Unity.Behavior.GenericSerializeReference;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Unity.Behavior
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Destroy Object",
        story: "Destroy [Object]",
        category: "Action/GameObject",
        description: "Destroys a GameObject.",
        id: "213e398c103ffd856facab409462631d")]
    internal partial class DestroyObjectAction : Action
    {
        [SerializeReference] public BlackboardVariableGameObject Object;

        protected override Status OnStart()
        {
            if (Object.Value == null)
            {
                LogFailure("No valid object to destroy provided.");
                return Status.Failure;
            }

            GameObject.Destroy(Object.Value);
            return Status.Success;
        }
    }
}
