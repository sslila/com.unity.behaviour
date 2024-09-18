using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Properties;

namespace Unity.Behavior
{
    static class DebugNodeUtility
    {
        public static void Log(BlackboardVariable variable, GameObject owner)
        {
            string ownerPrefix = owner ? $"[{owner.name}]" : "";
            string name = !string.IsNullOrEmpty(variable.Name) ? variable.Name : "<no name>";
            object value = variable.ObjectValue ?? "null";
            Debug.Log($"{ownerPrefix} {name} = {value}", owner);
        }
    }
    
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Log Variable", 
        story: "Log [variable] to the console", 
        description: "Logs the value of a variable to the console.",
        category: "Action/Debug",
        id: "b95551d408d852c7e54ce84d1369f56a")]
    internal partial class LogVariableToConsoleAction : Action
    {
        [SerializeReference] public BlackboardVariable Variable;

        protected override Status OnStart()
        {
            DebugNodeUtility.Log(Variable, GameObject);
            return Status.Success;
        }
    }
    
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Log Variable Change", 
        story: "Log [variable] change to the console", 
        description: "Logs the value of a variable to the console when it changes.",
        category: "Action/Debug",
        id: "b95551d408d852c7e54ce84d1369f56b")]
    internal partial class LogVariableValueChangeAction : Action
    {
        [SerializeReference] public BlackboardVariable Variable;
        [SerializeReference] public BlackboardVariable<bool> PauseEditor;

        protected override Status OnStart()
        {
            if (Variable == null)
            {
                return Status.Failure;
            }
            DebugNodeUtility.Log(Variable, GameObject);
            Variable.OnValueChanged += OnVariableChanged;
            return Status.Waiting;
        }

        protected override Status OnUpdate() => Status.Success;
        
        protected override void OnEnd()
        {
            if (Variable != null)
            {
                Variable.OnValueChanged -= OnVariableChanged;
            }
        }

        protected override void OnDeserialize()
        {
            // On deserialize will only be call if the node is waiting, so we need to register again.
            if (Variable != null)
            {
                Variable.OnValueChanged += OnVariableChanged;
            }
        }

        private void OnVariableChanged()
        {
            DebugNodeUtility.Log(Variable, GameObject);
#if UNITY_EDITOR
            if (PauseEditor.Value)
            {
                EditorApplication.isPaused = true;
            }
#endif
            AwakeNode(this);
        }
    }
    
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Log Message",
        story: "Log [message] to the console",
        description: "Logs a message to the console.",
        category: "Action/Debug",
        id: "b95551d408d852c7e54ce84d1369f56c")]
    internal partial class LogMessageToConsoleAction : Action
    {
        public enum LogType
        {
            Info,
            Warning,
            Error
        }
        
        [SerializeReference] public BlackboardVariable<string> Message;
        [SerializeReference] public BlackboardVariable<LogType> LogLevel = new BlackboardVariable<LogType>(LogType.Info);

        protected override Status OnStart()
        {
            if (Message == null)
            {
                return Status.Failure;
            }
            
            switch (LogLevel.Value)
            {
                case LogType.Info:
                    Debug.Log(Message.Value, GameObject);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(Message.Value, GameObject);
                    break;
                case LogType.Error:
                    Debug.LogError(Message.Value, GameObject);
                    break;
            }
            return Status.Success;
        }
    }
}

