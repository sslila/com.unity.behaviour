using System;

namespace Unity.Behavior.GraphFramework
{
    [Serializable]
    internal abstract class Command
    {
        public bool MarkUndo { get; }

        protected Command(bool markUndo)
        {
            MarkUndo = markUndo;
        }
    }
}