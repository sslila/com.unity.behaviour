using System.Collections.Generic;

namespace Unity.Behavior.GenericSerializeReference
{
    public class BlackboardVariableString  :BlackboardVariable<string>
    {
        public BlackboardVariableString() {}
        public BlackboardVariableString(string value) : base(value){}
    }
    public class BlackboardVariableStringList  :BlackboardVariable<List<string>>
    {
    }
}