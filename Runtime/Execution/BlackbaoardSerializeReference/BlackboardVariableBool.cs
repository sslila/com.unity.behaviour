using System.Collections.Generic;

namespace Unity.Behavior.GenericSerializeReference
{
    public class BlackboardVariableBool : BlackboardVariable<bool>
    {
        public BlackboardVariableBool()
        {
        }

        public BlackboardVariableBool(bool value) : base(value)
        {
        }
    }

    public class BlackboardVariableBoolList : BlackboardVariable<List<bool>>
    {
    }
}