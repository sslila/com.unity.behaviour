using System;

namespace Unity.Behavior.GraphFramework
{
    internal class BaseModel
    {
        public GraphAsset Asset { get; set; }
        public virtual IVariableLink GetVariableLink(string variableName, Type type) => null;
    }
}
