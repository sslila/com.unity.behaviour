using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Unity.Behavior
{
    [System.Serializable]
    internal class SerializableCommandBuffer
    {
        [HideInInspector, SerializeReference] private List<Command> m_Commands = new List<Command>();

        public void SerializeDeferredCommand(Command command)
        {
            m_Commands.Add(command);
        }

#if UNITY_EDITOR
        public void DispatchCommands(Dispatcher dispatcher)
        {
            // No null check as we init on construct.
            while (m_Commands.Count > 0)
            {
                Command command = m_Commands[0];
                m_Commands.RemoveAt(0);
                dispatcher.DispatchImmediate(command);
            }
        }
#endif
    }
}