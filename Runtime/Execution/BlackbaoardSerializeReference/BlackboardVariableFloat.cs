using System.Collections.Generic;

namespace Unity.Behavior.GenericSerializeReference
{
    public class BlackboardVariableFloat : BlackboardVariable<float>
    {
        public BlackboardVariableFloat(){}
        public BlackboardVariableFloat(float vale) : base(vale)
        {
        }
    }
    public class BlackboardVariableFloatList : BlackboardVariable<List<float>>
    {
    }
}