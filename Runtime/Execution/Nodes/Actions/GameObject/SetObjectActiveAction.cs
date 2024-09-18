using System;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Object Active State",
        category: "Action/GameObject",
        description: "Sets the active state of a GameObject.",
        story: "Set [Object] state to: [Active]",
        id: "634fab3048befb7df4b56527d8d76eee")]
    internal partial class SetObjectActiveAction : Action
    {
        [SerializeReference] public BlackboardVariableGameObject Object;
        [SerializeReference] public BlackboardVariableBool Active;

        protected override Status OnStart()
        {
            if (Object.Value == null)
            {
                LogFailure("No object assigned.");
                return Status.Failure;
            }

            Object.Value.SetActive(Active.Value);
            return Status.Success;
        }
    }
}
