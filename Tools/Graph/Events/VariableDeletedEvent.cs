using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
	internal class VariableDeletedEvent : EventBase<VariableDeletedEvent>
	{
		public VariableModel Variable { get; private set; }
		public static VariableDeletedEvent GetPooled(VisualElement target, VariableModel variable)
		{
			VariableDeletedEvent newEvent = GetPooled();
			newEvent.target = target;
			newEvent.Variable = variable;
			return newEvent;
		}
	}
}