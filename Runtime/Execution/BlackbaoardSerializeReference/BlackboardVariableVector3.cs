using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior.GenericSerializeReference
{
    public class BlackboardVariableVector3 : BlackboardVariable<Vector3>
    {
        public BlackboardVariableVector3() : base()
        {
        }
        public BlackboardVariableVector3(Vector3 value) : base(value)
        {
        }
    }

    public class BlackboardVariableVector3List : BlackboardVariable<List<Vector3>>
    {
    }

    public class BlackboardVariableVector2 : BlackboardVariable<Vector2>
    {
        public BlackboardVariableVector2() {}
        public BlackboardVariableVector2(Vector2 value) : base(value)
        {
        }
    }

    public class BlackboardVariableVector2List : BlackboardVariable<List<Vector2>>
    {
    }
    
    public class BlackboardVariableColor : BlackboardVariable<Color>
    {
        public BlackboardVariableColor(){}
        public BlackboardVariableColor(Color value) : base(value)
        {
        }
    }
    public class BlackboardVariableColorList : BlackboardVariable<List<Color>>
    {
    }
}