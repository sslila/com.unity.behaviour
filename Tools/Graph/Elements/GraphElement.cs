using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class GraphElement : VisualElement
    {
        public GraphElement()
        {
            style.position = UnityEngine.UIElements.Position.Absolute;
        }

        public Vector2 Position
        {
            get => transform.position;
        }

        public Translate Translate
        {
            get => style.translate.value;
            set => style.translate = value;
        }

        public virtual bool IsMoveable => false;
        public bool IsDeletable { get; protected internal set; } = true;

        public virtual void OnSelect() { }
        public virtual void OnDeselect() { }

        /// <summary>
        /// We route through this method to easily debug through a centralized place.
        /// It's much easier to find out what VisualElement's are dirty and repainting now.
        /// </summary>
        internal void MarkDirtyAndRepaint()
        {
            MarkDirtyRepaint();
        }
    }
}