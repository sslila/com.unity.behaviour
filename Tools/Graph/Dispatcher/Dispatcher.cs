#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal class Dispatcher
    {
        private readonly Dictionary<Type, List<BaseCommandHandler>> m_CommandTypeToHandlers = new();
        private readonly Queue<Command> m_DispatchQueue = new();
        private readonly IDispatcherContext m_DispatcherContext;

        public Dispatcher(IDispatcherContext context)
        {
            m_DispatcherContext = context;
        }

        public void RegisterHandler<CommandType, HandlerType>() 
            where CommandType : Command
            where HandlerType : CommandHandler<CommandType>, new()
        {
            RegisterHandler<CommandType, HandlerType>(new HandlerType());
        }

        public void RegisterHandler<CommandType, HandlerType>(HandlerType handler) 
            where CommandType : Command
            where HandlerType : CommandHandler<CommandType>                                                               
        {
            handler.DispatcherContext = m_DispatcherContext;
            if (m_CommandTypeToHandlers.TryGetValue(typeof(CommandType), out List<BaseCommandHandler> commandHandlers)) 
            {
                commandHandlers.Add(handler);
            }
            else
            {
                commandHandlers = new List<BaseCommandHandler> { handler };
                m_CommandTypeToHandlers.Add(typeof(CommandType), commandHandlers);
            }
        }

        public void UnregisterHandler<CommandType, HandlerType>()
            where CommandType : Command
            where HandlerType : CommandHandler<CommandType>                     
        {
            if (m_CommandTypeToHandlers.TryGetValue(typeof(CommandType), out List<BaseCommandHandler> commandHandlers)) 
            {
                commandHandlers.RemoveAll(handler => handler is HandlerType);
            }
        }

        public void Dispatch(Command command)
        {
            m_DispatchQueue.Enqueue(command);
        }

        public void DispatchImmediate(Command command, bool setHasOutstandingChanges = true)
        {
            Type commandType = command.GetType();
            if (!m_CommandTypeToHandlers.TryGetValue(commandType, out List<BaseCommandHandler> commandHandlers))
            {
                Debug.LogWarning("No registered command handler for command type: " + commandType.Name);
                return;
            }

            if (command.MarkUndo)
            {
                if (m_DispatcherContext.GraphAsset != null)
                {
                    m_DispatcherContext.GraphAsset.MarkUndo(command.GetType().Name);
                }

                if (m_DispatcherContext.BlackboardAsset != null)
                {
                    m_DispatcherContext.BlackboardAsset.MarkUndo(command.GetType().Name);
                }
            }
            
            foreach (BaseCommandHandler commandHandler in commandHandlers)
            {
                if (commandHandler.Process(command))
                {
                    break;
                }
            }
            if (m_DispatcherContext.GraphAsset)
            {
                m_DispatcherContext.GraphAsset.SetAssetDirty(setHasOutstandingChanges);
            }
            if (m_DispatcherContext.BlackboardAsset != null)
            {
                m_DispatcherContext.BlackboardAsset.SetAssetDirty();
            }
        }

        public void Tick()
        {
            while (m_DispatchQueue.TryDequeue(out Command command))
            {
                DispatchImmediate(command);
            }
        }

        public void ClearDispatchQueue()
        {
            m_DispatchQueue.Clear();
        }
    }
}
#endif