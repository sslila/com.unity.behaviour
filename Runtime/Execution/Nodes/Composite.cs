using System;
using System.Collections.Generic;
using UnityEngine;


namespace Unity.Behavior
{
    /// <summary>
    /// Composite nodes serves as a control structure that manages the flow and organization of other nodes within the tree.
    /// </summary>
    [Serializable]
    public abstract class Composite : Node, IParent
    {
        /// <summary>
        /// The parent of the node.
        /// </summary>
        public Node Parent { get => m_Parent; internal set { m_Parent = value; } }
        [SerializeReference]
        internal Node m_Parent;

        /// <summary>
        /// The children of the node.
        /// </summary>
        public List<Node> Children { get => m_Children; internal set => m_Children = value; }
        [SerializeReference]
        internal List<Node> m_Children = new List<Node>();

        /// <inheritdoc cref="ResetStatus" />
        protected internal override void ResetStatus()
        {
            CurrentStatus = Status.Uninitialized;
            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].ResetStatus();
            }
        }

        /// <inheritdoc cref="AwakeParents" />
        protected internal override void AwakeParents()
        {
            AwakeNode(Parent);
        }

        /// <inheritdoc cref="Add" />
        public void Add(Node child)
        {
            Children.Add(child);
            child?.AddParent(this);
        }

        /// <inheritdoc cref="AddParent" />
        internal override void AddParent(Node parent)
        {
            this.Parent = parent;
        }
    }
}