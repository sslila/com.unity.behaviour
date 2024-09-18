#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
    internal static class ConditionUtility
    {
        internal static List<Condition> GetConditions()
        {
            IEnumerable<Type> typeList = GetConditionTypes();
            
            List<Condition> conditionList = typeList.Where(t => typeof(Condition).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .Cast<Condition>()
                .ToList();

            return conditionList;
        }

        internal static IEnumerable<Type> GetConditionTypes()
        {
#if UNITY_EDITOR
            List<Type> typeList = UnityEditor.TypeCache.GetTypesWithAttribute<ConditionAttribute>()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Condition)))
                .ToList();
#else
            List<Type> typeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Condition)))
                ).ToList();
#endif

            return typeList;
        }

        internal static ConditionInfo GetInfoForConditionType(Type type)
        {
            ConditionAttribute attribute = (ConditionAttribute) Attribute.GetCustomAttribute(type, typeof (ConditionAttribute));

            string conditionName;
            SerializableGUID id;
            string category;
            string story;
            string filePath;
            if (attribute != null)
            {
                conditionName = attribute.Name;
                
                id = attribute.GUID;
                category = String.IsNullOrEmpty(attribute.Category) ? "Conditions" : attribute.Category;
                story = attribute.Story;
                filePath = attribute.FilePath;
            }
            else
            {
                conditionName = type.Name;
                id = SerializableGUID.Generate();
                category = "Conditions";
                story = string.Empty;
                filePath = string.Empty;
            }

            return new ConditionInfo { Name = conditionName, SerializableType = new SerializableType(type), TypeID = id, Category = category, FilePath = filePath, StoryInfo = new StoryInfo { Story = story, Variables = NodeRegistry.GetNodeVariables(type), StoryVariableNames=NodeRegistry.GetStoryVariableNames(story) }};
        }
    }
}
#endif