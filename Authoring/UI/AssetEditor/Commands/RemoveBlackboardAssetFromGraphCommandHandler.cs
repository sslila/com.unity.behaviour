#if UNITY_EDITOR
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal class RemoveBlackboardAssetFromGraphCommandHandler : CommandHandler<RemoveBlackboardAssetFromGraphCommand>
    {
        public override bool Process(RemoveBlackboardAssetFromGraphCommand command)
        {
            if (BlackboardView is not BehaviorGraphBlackboardView graphBlackboardView)
            {
                return false;
            }

            for (int index = 0; index < command.GraphAsset.m_Blackboards.Count; index++)
            {
                BehaviorBlackboardAuthoringAsset blackboardAuthoring = command.GraphAsset.m_Blackboards[index];
                if (blackboardAuthoring.AssetID == command.blackboardAuthoringAsset.AssetID)
                {
                    command.GraphAsset.m_Blackboards.Remove(blackboardAuthoring);
                }

#if UNITY_EDITOR
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.EditorUtility.SetDirty(command.GraphAsset);
#endif
            }

            foreach (VariableModel variable in command.blackboardAuthoringAsset.Variables)
            {
                DispatcherContext.Root.SendEvent(VariableDeletedEvent.GetPooled(DispatcherContext.Root, variable));    
            }
            
            graphBlackboardView.InitializeListView();

            // Have we processed the command and wish to block further processing?
            return false;
        }
    }
}
#endif