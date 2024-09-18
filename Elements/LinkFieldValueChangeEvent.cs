#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class LinkFieldValueChangeEvent : EventBase<LinkFieldValueChangeEvent>
    {
        public object Value { get; private set; }
        public static LinkFieldValueChangeEvent GetPooled(BaseLinkField target, object evtNewValue)
        {
            LinkFieldValueChangeEvent newEvent = GetPooled();
            newEvent.Value = evtNewValue;
            newEvent.target = target;
            newEvent.bubbles = true;
            return newEvent;
        }
    }
}
#endif