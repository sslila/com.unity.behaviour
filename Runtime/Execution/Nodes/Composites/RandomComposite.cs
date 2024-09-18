using System;
using Unity.Properties;


namespace Unity.Behavior
{
    /// <summary>
    /// Executes a random branch.
    /// </summary>
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Random", 
        description: "Executes a random branch.", 
        icon: "Icons/random", 
        category: "Flow", 
        id: "3ec329cc9c414fd88aa9113e7c422f1a")]
    internal partial class RandomComposite : Composite
    {
        int m_RandomIndex = 0;

        /// <inheritdoc cref="OnStart" />
        protected override Status OnStart()
        {
            m_RandomIndex = UnityEngine.Random.Range(0, Children.Count);
            if (m_RandomIndex < Children.Count)
            {
                var status = StartNode(Children[m_RandomIndex]);
                if (status == Status.Success || status == Status.Failure)
                    return status;
                
                return Status.Waiting;
            }

            return Status.Success;
        }

        /// <inheritdoc cref="OnUpdate" />
        protected override Status OnUpdate()
        {
            var status = Children[m_RandomIndex].CurrentStatus;
            if (status == Status.Success || status == Status.Failure)
                return status;
            
            return Status.Waiting;
        }
    }
}