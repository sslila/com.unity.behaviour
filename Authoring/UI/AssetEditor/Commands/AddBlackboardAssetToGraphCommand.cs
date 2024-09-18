#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;
using UnityEngine.Serialization;

namespace Unity.Behavior
{
    internal class AddBlackboardAssetToGraphCommand : Command
    {
        public BehaviorAuthoringGraph GraphAsset;
        [FormerlySerializedAs("m_AuthoringBlackboardAsset")] public BehaviorBlackboardAuthoringAsset blackboardAuthoringAsset;

        public AddBlackboardAssetToGraphCommand(BehaviorAuthoringGraph graph, BehaviorBlackboardAuthoringAsset blackboardAuthoring, bool markUndo) : base(markUndo)
        {
            GraphAsset = graph;
            blackboardAuthoringAsset = blackboardAuthoring;
        }
    }
}

#endif