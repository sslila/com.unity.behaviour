using System;

namespace Unity.Behavior
{
    /// <summary>
    ///  The attribute specified when creating Blackboard enums.
    ///  Apply this attribute above newly created enums to ensure they can be recognized and parsed by the Blackboard.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class BlackboardEnumAttribute : Attribute
    {
    }
}