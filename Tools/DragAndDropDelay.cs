using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
	internal class DragAndDropDelay
	{
		const float k_StartDragTreshold = 4.0f;
		public bool IsDragging { get; private set; }
		bool IsInitialized { set; get; }

		Vector2 mouseDownPosition { get; set; }

		public void Init(Vector2 mousePosition)
		{
			mouseDownPosition = mousePosition;
			IsDragging = false;
			IsInitialized = true;
		}

		public bool CanStartDrag(Vector2 mousePosition)
		{
		    return Vector2.Distance(mouseDownPosition, mousePosition) > k_StartDragTreshold && IsInitialized;
		}

		public void StartDrag()
		{
		    IsDragging = true;
		    IsInitialized = false;
		}

		public void Cancel()
		{
		    IsInitialized = false;
		    IsDragging = false;
		}
	}
}