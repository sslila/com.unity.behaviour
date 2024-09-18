using UnityEngine;

namespace Unity.Behavior.GenericSerializeReference
{
    public class BlackboardVariableGameObject : BlackboardVariable<GameObject>
    {
        public BlackboardVariableGameObject(){}
        public BlackboardVariableGameObject(GameObject value) : base(value){}
    }
    public class BlackboardVariableScriptableObject : BlackboardVariable<ScriptableObject>
    {
        
    }
}