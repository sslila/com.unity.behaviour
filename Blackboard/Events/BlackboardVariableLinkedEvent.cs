#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
	internal class LinkFieldLinkButtonEvent : EventBase<LinkFieldLinkButtonEvent>
	{
		public Type FieldType { get; private set; }
        public bool AllowAssetsEmbeds { get; private set; }

        public static LinkFieldLinkButtonEvent GetPooled(BaseLinkField target, Type fieldType, bool allowAssetsEmbeds = false)
		{
			LinkFieldLinkButtonEvent newEvent = GetPooled();
			newEvent.FieldType = fieldType;
			newEvent.target = target;
			newEvent.bubbles = true;
			newEvent.AllowAssetsEmbeds = allowAssetsEmbeds;
            return newEvent;
		}
	}

	internal class LinkFieldTypeChangeEvent : EventBase<LinkFieldTypeChangeEvent>
	{
		public Type FieldType { get; private set; }

		public static LinkFieldTypeChangeEvent GetPooled(BaseLinkField target, Type newFieldType)
		{
			LinkFieldTypeChangeEvent newEvent = GetPooled();
			newEvent.FieldType = newFieldType;
			newEvent.target = target;
			newEvent.bubbles = true;
			return newEvent;
		}
	}
}
#endif