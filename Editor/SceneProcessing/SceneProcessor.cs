#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.Behavior.SceneProcessing
{
    public class SceneProcessor: IProcessSceneWithReport 
    {
        public int callbackOrder { get; }
        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            var behaviorGraphAgents = Object.FindObjectsByType<BehaviorGraphAgent>(FindObjectsSortMode.None);
            foreach (var behaviorGraphAgent in behaviorGraphAgents)
            {
                if (behaviorGraphAgent.Graph)
                {
                    behaviorGraphAgent.Init();
                }
            }
        }
    }
}
#endif
