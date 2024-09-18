using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    /// <summary>
    /// The EventChannelDescriptionAttribute contains metadata for event channels used in Muse Behavior graphs. 
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class EventChannelDescriptionAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes an instance of the <see cref="EventChannelDescriptionAttribute"/> class with the provided metadata.
        /// </summary>
        /// <param name="name">The name of the event channel.</param>
        /// <param name="description">The description of the event channel's function.</param>
        /// <param name="message">The message that describes the event that is being triggered.</param>
        /// <param name="icon">The path of the icon to be used when representing the event channel in the graph editor.</param>
        /// <param name="category">The category path this event channel belongs to and will be shown on the search window.</param>
        /// <param name="id">A unique ID used to identify this event channel.</param>
        /// <param name="filePath">The path to the script file containing this attribute.</param>
        public EventChannelDescriptionAttribute(string name = "", string description = "", string message = "", string icon = "", string category = "", string id = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
        {
            Name = name;
            Description = description;
            Message = message;
            Icon = icon;
            Category = category;
            FilePath = filePath;
            GUID = new SerializableGUID(id);
        }

        /// <summary>
        /// The name of the event channel.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The description of the event channel's function.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// The message that describes the event that has occured.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// The path of the icon to be used when representing the event channel in the graph editor.
        /// </summary>
        public string Icon { get; }

        /// <summary>
        /// The category path this event channel belongs to and will be shown on the search window.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// A unique ID used to identify this event channel.
        /// </summary>
        public SerializableGUID GUID { get; }

        /// <summary>
        /// The path to the script file containing this attribute.
        /// </summary>
        public string FilePath { get; }
    }
}