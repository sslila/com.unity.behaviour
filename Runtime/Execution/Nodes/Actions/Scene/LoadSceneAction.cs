using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Load Scene",
        story: "Load scene [SceneName]",
        category: "Action/Scene",
        description: "Loads a Unity scene. Uses the Mode variable to select whether to load additive or replace the current scene [Single].",
        id: "8d8d7217fed8a45aa43f5776b9849e7f")]
    internal partial class LoadSceneAction : Action
    {
        [Tooltip("You can provide either the full Scene path, the path shown in the Build Settings window, or just the Scene name." +
            "\nIf you only provide the Scene name, Unity loads the first Scene in the list that matches.")]
        [SerializeReference] public BlackboardVariable<string> SceneName;
        [SerializeReference] public BlackboardVariable<LoadSceneMode> Mode;

        [CreateProperty] private bool m_PendingOperation;
        private AsyncOperation m_AsyncOperation;

        protected override Status OnStart()
        {
            // Checks if the scene is already loaded.
            var scene = SceneManager.GetSceneByName(SceneName.Value);
            if (scene != null && scene.isLoaded)
            {
                return Status.Success;
            }

            m_AsyncOperation = SceneManager.LoadSceneAsync(SceneName.Value, Mode);
            if (m_AsyncOperation == null)
            {
                LogFailure($"Failed to load scene '{SceneName.Value}'.", true);
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
            // If loading when serialized, we need to re-run the operation.
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
