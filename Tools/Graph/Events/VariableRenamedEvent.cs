using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
	internal class VariableRenamedEvent : EventBase<VariableRenamedEvent>
	{
		public VariableModel Variable { get; private set; }
		public static VariableRenamedEvent GetPooled(VisualElement target, VariableModel variable)
		{
			VariableRenamedEvent newEvent = GetPooled();
			newEvent.target = target;
			newEvent.Variable = variable;
			return newEvent;
		}
	}
}