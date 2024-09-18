using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Unload Scene",
        story: "Unload scene [SceneName]",
        category: "Action/Scene",
        description: "Unloads a Unity scene.",
        id: "8da34c59581ad18979662407729f9a7e")]
    internal partial class UnloadSceneAction : Action
    {
        [Tooltip("The given Scene name can either be the full Scene path, the path shown in the Build Settings window or just the Scene name. " +
            "\nIf only the Scene name is given this will unload the first Scene in the list that matches.")]
        [SerializeReference] public BlackboardVariable<string> SceneName;
        [SerializeReference] public BlackboardVariable<UnloadSceneOptions> UnloadOptions;
        
        [CreateProperty] private bool m_PendingOperation;
        private AsyncOperation m_AsyncOperation;

        protected override Status OnStart()
        {
            // If the scene isn't loaded, we can skip the unloading process.
            var scene = SceneManager.GetSceneByName(SceneName.Value);
            if (scene != null && !scene.isLoaded)
            {
                return Status.Success;
            }

            m_AsyncOperation = SceneManager.UnloadSceneAsync(SceneName.Value, UnloadOptions);
            if (m_AsyncOperation == null)
            {
                LogFailure($"Failed to unload scene '{ SceneName.Value }'.", true);
                return Status.Failure;
            }

            m_AsyncOperation.completed += operation =>
            {
                if (CurrentStatus == Status.Waiting)
                {
                    AwakeNode(this);
                }
            };
            return m_AsyncOperation.isDone ? Status.Success : Status.Waiting;
        }

        protected override Status OnUpdate()
        {
            return Status.Success;
        }

        protected override void OnSerialize()
        {
            m_PendingOperation = CurrentStatus == Status.Waiting || !m_AsyncOperation.isDone;
        }

        protected override void OnDeserialize()
        {
            // If unloading when serialized, we need to re-run the operation.
            if (!m_PendingOperation)
            {
                return;
            }

            m_PendingOperation = false;
            if (OnStart() == Status.Success)
            {
                CurrentStatus = Status.Running;
            }
        }
    }
}