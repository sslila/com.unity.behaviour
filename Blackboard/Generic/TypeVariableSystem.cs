using System;
using System.Collections.Generic;

namespace Unity.Behavior.GraphFramework.Generic
{
    public class TypedVariableModelString : TypedVariableModel<string>{}
    public class TypedVariableModelStringList : TypedVariableModel<List<string>>{}
    public class TypedVariableModelEnum : TypedVariableModel<Enum>{}
    public class TypedVariableModelEnumList : TypedVariableModel<List<Enum>>{}
}