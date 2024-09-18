using Unity.Behavior.GraphFramework;
using System;

namespace Unity.Behavior
{
    /// <summary>
    /// Utility used to transform authoring model data into a runtime classes.
    /// Allows to be more flexible with the data we put in models/runtime without leaking information over.
    /// Can also be used to create multiple/different runtime nodes out of a single authoring model (i.e. Abort/Restart node).
    /// </summary>
    internal interface INodeTransformer
    {
        Type NodeModelType { get; }
        Node CreateNodeFromModel(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel);
        void ProcessNode(GraphAssetProcessor graphAssetProcessor, NodeModel nodeModel, Node node);
    }
}