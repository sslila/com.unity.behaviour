using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    /// <summary>
    /// The NodeDescriptionAttribute contains metadata for nodes used in Muse Behavior graphs. 
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class NodeDescriptionAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes an instance of the <see cref="NodeDescriptionAttribute"/> class with the provided metadata.
        /// </summary>
        /// <param name="name">The name of the node.</param>
        /// <param name="description">The description of the node's function.</param>
        /// <param name="story">The story summarizing what occurs when the node executes.</param>
        /// <param name="icon">The path of the icon to be used when representing the node in the graph editor.</param>
        /// <param name="category">The category path this node belongs to and will be shown on the search window.</param>
        /// <param name="id">A unique ID used to identify this node.</param>
        /// <param name="hideInSearch">Controls if the node should be shown in the add node UI.</param>
        /// <param name="filePath">The path to the script file containing this attribute.</param>
        public NodeDescriptionAttribute(string name = "", string description = "", string story = "", string icon = "", string category = "", string id = "", bool hideInSearch = false, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            Name = name;
            Description = description;
            Story = story;
            Icon = icon;
            Category = category;
            FilePath = filePath;
            HideInSearch = hideInSearch;
            GUID = new SerializableGUID(id);
        }

        /// <summary>
        /// The name of the node.
        /// </summary>
        internal string Name { get; }
        
        /// <summary>
        /// The description of the node's function.
        /// </summary>
        internal string Description { get; }
        
        /// <summary>
        /// The story summarizing what occurs when the node executes.
        /// </summary>
        internal string Story { get; }
        
        /// <summary>
        /// The path of the icon to be used when representing the node in the graph editor.
        /// </summary>
        internal string Icon { get; }

        /// <summary>
        /// The category path this node belongs to and will be shown on the search window.
        /// </summary>
        internal string Category { get; }

        /// <summary>
        /// A unique ID used to identify this node.
        /// </summary>
        internal SerializableGUID GUID { get; }

        /// <summary>
        /// Controls if the node should be shown in the add node UI.
        /// </summary>
        internal bool HideInSearch { get; }

        /// <summary>
        /// The path to the script file containing this attribute.
        /// </summary>
        internal string FilePath { get; }
    }
}