using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// A node that can have multiple inputs nodes.
    /// </summary>
    public abstract class Join : Node, IParent
    {
        /// <summary>
        /// The parents of the node.
        /// </summary>
        public List<Node> Parents { get => m_Parents; internal set => m_Parents = value; }
        [SerializeReference, DontCreateProperty]
        private List<Node> m_Parents = new List<Node>();

        /// <summary>
        /// The child of the node.
        /// </summary>
        [CreateProperty]
        public Node Child { get => m_Child; internal set => m_Child = value; }
        [SerializeReference, DontCreateProperty]
        private Node m_Child;

        /// <inheritdoc cref="ResetStatus" />
        protected internal override void ResetStatus()
        {
            CurrentStatus = Status.Uninitialized;
            Child?.ResetStatus();
        }

        /// <inheritdoc cref="AwakeParents" />
        protected internal override void AwakeParents()
        {
            for (int i = 0; i < Parents.Count; i++)
            {
                AwakeNode(Parents[i]);
            }
        }

        /// <inheritdoc cref="Add" />
        public void Add(Node child)
        {
            Child = child;
            child.AddParent(this);
        }

        /// <inheritdoc cref="AddParent" />
        internal override void AddParent(Node parent)
        {
            this.Parents.Add(parent);
        }
    }
}