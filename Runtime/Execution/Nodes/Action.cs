using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// The base class for action nodes used in Muse Behavior Graphs.
    /// </summary>
    public abstract class Action : Node
    {
        /// <summary>
        /// The parent of the node.
        /// </summary>
        public Node Parent
        {
            get => m_Parent;
            internal set { m_Parent = value; }
        }
        [SerializeReference]
        internal Node m_Parent;
        
        /// <inheritdoc cref="Node.AwakeParents" />
        protected internal override void AwakeParents()
        {
            AwakeNode(Parent);
        }

        /// <inheritdoc cref="Node.AddParent" />
        internal override void AddParent(Node parent)
        {
            this.Parent = parent;
        }
    }
}