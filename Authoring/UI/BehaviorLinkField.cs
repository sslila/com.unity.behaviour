#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;
using System;

namespace Unity.Behavior
{
    internal class BehaviorLinkField<TValueType, TFieldType> : LinkField<TValueType, TFieldType> where TFieldType : VisualElement, INotifyValueChanged<TValueType>, new()
    {
        public override bool IsAssignable(Type type)
        {
            if (GraphAssetProcessor.GetBlackboardVariableConverter(type, LinkVariableType) != null)
            {
                return true;
            }
            return base.IsAssignable(type);
        }
    }
}
#endif