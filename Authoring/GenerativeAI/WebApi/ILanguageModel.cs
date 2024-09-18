using System;

namespace Unity.Behavior.GenerativeAI
{
    internal interface ILanguageModel
    {
        void Chat(string input, Action<(string output, string conversationId)> onResponseReceived, string origin, string conversationId = null);
    }
}