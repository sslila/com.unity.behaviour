using System;
using System.Collections.Generic;
using Unity.Behavior.GenericSerializeReference;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Set Object List Active State",
        category: "Action/GameObject",
        story: "Set [ObjectList] state to: [Active]",
        description: "Sets the active state of all the GameObjects in the list.",
        id: "0adab91b7bb783220d2d66a5a768f169")]
    internal partial class SetObjectListActiveAction : Action
    {
        [SerializeReference] public BlackboardVariableGameObjectList ObjectList;
        [SerializeReference] public BlackboardVariableBool Active;

        protected override Status OnStart()
        {
            if (ObjectList.Value.Count == 0)
            {
                LogFailure("Empty object list assigned.");
                return Status.Failure;
            }

            foreach (GameObject obj in ObjectList.Value)
            {
                obj.SetActive(Active.Value);
            }

            return Status.Success;
        }
    }
}
