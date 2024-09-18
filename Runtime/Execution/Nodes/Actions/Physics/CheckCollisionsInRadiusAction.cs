using System;
using UnityEngine;
using Unity.Properties;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Check Collisions In Radius", 
        story: "Check [Agent] collisions in [Radius] radius",
        description: "Checks for collisions in a specified radius around the agent using OverlapSphere. " +
        "\nIf a collision is found, the collided object is stored in [CollidedObject].",
        category: "Action/Physics", 
        id: "a6254a477920c00f5e477c8b886b205a")]
    internal partial class CheckCollisionsInRadiusAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<float> Radius;
        [SerializeReference] public BlackboardVariable<string> Tag;
        [Tooltip("[Out Value] This field is assigned with the collided object, if a collision was found.")]
        [SerializeReference] public BlackboardVariable<GameObject> CollidedObject;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent set.");
                return Status.Failure;
            }

            if (CollidedObject is IBlackboardVariableCast)
            {
                var caster = CollidedObject as IBlackboardVariableCast;
                LogFailure($"Invalid CollidedObject variable: Expecting 'GameObject' but is '{caster.SourceTypeName}'. Please provide a valid GameObject variable.");
                return Status.Failure;
            }

            Collider[] hitColliders = Physics.OverlapSphere(Agent.Value.transform.position, Radius.Value);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider hitCollider = hitColliders[i];
                if (hitCollider.gameObject == Agent.Value
                    || (Tag != null && Tag.Value != string.Empty && !hitCollider.CompareTag(Tag.Value)))
                {
                    continue;
                }

                if (CollidedObject.Value != null)
                {
                    CollidedObject.Value = hitCollider.gameObject;
                }
            
                return Status.Success;
            }

            return Status.Failure;
        }
    }
}
