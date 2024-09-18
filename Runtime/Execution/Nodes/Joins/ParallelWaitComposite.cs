using System;
using Unity.Properties;

namespace Unity.Behavior
{
    /// <summary>
    /// Activates its child when any parent starts this node. It cannot restart until the child's subgraph has ended.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Any",
        category: "Flow/Parallel Execution",
        description: "Activates its child when any parent starts this node. It cannot restart until the child's subgraph has ended.", 
        icon: "Icons/parallel_any",
        id: "bf0ecb9b9f44492eb96f0442454949a1")]
    internal partial class WaitForAnyComposite : Join
    {
        [CreateProperty] private int m_StartCount;
        [CreateProperty] private double m_LastFrameTimestamp;
        [CreateProperty] private Status m_PreviousStatus;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            m_StartCount++;
            var currentFrame = UnityEngine.Time.unscaledTimeAsDouble;
            if (m_StartCount > 1 || (HasCompletedParent() && m_PreviousStatus != Status.Uninitialized) || currentFrame == m_LastFrameTimestamp)
            {
                return m_PreviousStatus;
            }
            m_LastFrameTimestamp = currentFrame;
            if (Child == null)
            {
                return Status.Success;
            }

            Status childStatus = StartNode(Child);
            if (childStatus is Status.Success or Status.Failure)
            {
                return childStatus;
            }
            return Status.Waiting;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            Status childStatus = Child.CurrentStatus;
            if (childStatus is Status.Success or Status.Failure)
            {
                return childStatus;
            }
            return Status.Waiting;
        }

        /// <inheritdoc cref="OnEnd" />
        protected override void OnEnd()
        {
            m_PreviousStatus = CurrentStatus;
            m_StartCount--;
            if (m_StartCount == 0 && !HasCompletedParent())
            {
                base.OnEnd();
            }
        }

        /// <inheritdoc cref="ResetStatus" />
        protected internal override void ResetStatus()
        {
            var currentFrame = UnityEngine.Time.unscaledTimeAsDouble;
            if (CurrentStatus != Status.Uninitialized && m_StartCount == 0 && !HasCompletedParent()  && currentFrame != m_LastFrameTimestamp)
            {
                m_PreviousStatus = Status.Uninitialized;
                base.ResetStatus();
            }
        }

        private bool HasCompletedParent()
        {
            foreach (var parent in Parents)
            {
                if (parent.CurrentStatus is Status.Success or Status.Failure)
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    /// <summary>
    /// Activates its child when all parents have started this node. It cannot restart until the child's subgraph has ended.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For All",
        category: "Flow/Parallel Execution",
        description: "Activates its child when all parents have started this node. It cannot restart until the child's subgraph has ended.", 
        icon: "Icons/parallel_all",
        id: "848efe0dcb174f6aa79c7bfac31028fe")]
    public partial class WaitForAllComposite : Join
    {
        int m_StartCount;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            m_StartCount++;
            if (m_StartCount < Parents.Count)
            {            
                return Status.Running;
            }
            if (Child == null)
            {
                return Status.Success;
            }
            
            Status childStatus = StartNode(Child);
            if (childStatus is Status.Success or Status.Failure)
            {
                return childStatus;
            }
            return Status.Waiting;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            if (m_StartCount < Parents.Count)
            {
                return Status.Running;
            }
            if (Child == null)
            {
                return Status.Success;
            }
            
            Status childStatus = Child.CurrentStatus;
            if (childStatus is Status.Success or Status.Failure)
            {
                return childStatus;
            }
            return Status.Waiting;
        }

        /// <inheritdoc cref="OnEnd" />
        protected override void OnEnd()
        {
            m_StartCount--;
            if (m_StartCount == 0)
            {
                base.OnEnd();
            }
        }

        /// <inheritdoc cref="ResetStatus" />
        protected internal override void ResetStatus()
        {
            // Do not reset the status if the node is already running.
            if (CurrentStatus != Status.Uninitialized && m_StartCount == 0)
            {
                base.ResetStatus();
            }
        }
    }
}