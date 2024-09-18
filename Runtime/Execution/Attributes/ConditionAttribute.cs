using System;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    /// <summary>
    /// The ConditionAttribute contains metadata for conditions used in Muse Behavior graphs. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConditionAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ConditionAttribute"/> class with the provided metadata.
        /// </summary>
        /// <param name="name">The name of the condition.</param>
        /// <param name="description">The description of the condition's function.</param>
        /// <param name="story">The story message that describes the condition that is being evaluated. </param>
        /// <param name="category">The category path this condition belongs to and will be shown on the search window.</param>
        /// <param name="id">A unique ID used to identify this condition.</param>
        /// <param name="filePath">The path to the script file containing this attribute.</param>
        public ConditionAttribute(string name = "", string description = "", string story = "", string category = "", string id = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            Name = name;
            Description = description;
            Story = story;
            Category = category;
            FilePath = filePath;
            GUID = new SerializableGUID(id);
        }

        /// <summary>
        /// The name of the condition.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The description of the condition's function.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// The condition message that describes the condition that has occured.
        /// </summary>
        public string Story { get; }

        /// <summary>
        /// The category path this condition belongs to and will be shown on the search window.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// A unique ID used to identify this condition.
        /// </summary>
        public SerializableGUID GUID { get; }

        /// <summary>
        /// The path to the script file containing this attribute.
        /// </summary>
        public string FilePath { get; }
    }
}