using System;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable]
    internal class GameObjectToComponentBlackboardVariable<CastType> : BlackboardVariableCaster<GameObject, CastType>
        where CastType : Component
    {
        protected override GameObject GetSourceObjectFromTarget(CastType value) => value.gameObject;
        protected override CastType GetTargetObjectFromSource(GameObject variable) => variable.GetComponent<CastType>();

        // Required for serialization
        public GameObjectToComponentBlackboardVariable() { }

        public GameObjectToComponentBlackboardVariable(BlackboardVariable<GameObject> linkedVariable)
            : base(linkedVariable)
        { }
    }
}