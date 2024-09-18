using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Behavior
{
    [Serializable]
    internal class NodeInfo
    {
        internal Type Type => SerializableType;
        [SerializeField] internal SerializableType SerializableType;
        [SerializeField] internal SerializableGUID TypeID;
        [SerializeField] internal SerializableType ModelType;

        [SerializeField] internal string Name;
        [SerializeField] internal string Description;
        [SerializeField] internal Texture2D Icon;
        [SerializeField] internal string Category;
        [SerializeField] internal bool HideInSearch;
        [SerializeField] internal string FilePath;
        [SerializeField] internal List<string> NamedChildren;

        [SerializeField] internal StoryInfo StoryInfo = new();
        [SerializeField] internal string Story => StoryInfo.Story;
        [SerializeField] internal List<VariableInfo> Variables => StoryInfo.Variables;
    }

    [Serializable]
    internal class StoryInfo
    {
        [SerializeField] internal string Story = string.Empty;
        [SerializeField] internal List<string> StoryVariableNames = new();
        [SerializeField] internal List<VariableInfo> Variables = new();
    }

    [Serializable]
    internal class VariableInfo
    {
        public string Name;
        public SerializableType Type;
        public object DefaultValue;
        public string Tooltip;
    }

    [Serializable]
    internal class ConditionInfo
    {
        internal Type Type => SerializableType;
        [SerializeField] internal SerializableType SerializableType;
        [SerializeField] internal SerializableGUID TypeID;
        [SerializeField] internal string Name;
        [SerializeField] internal string Category;
        [SerializeField] internal string FilePath;
        [SerializeField] internal string Path => string.IsNullOrEmpty(Category) ? Name : $"{Category}/{Name}";
        [SerializeField] internal StoryInfo StoryInfo = new();
        [SerializeField] internal string Story => StoryInfo.Story;
        [SerializeField] internal List<VariableInfo> Variables => StoryInfo.Variables;
    }

    internal class NodeRegistry
    {
        private static NodeRegistry m_Instance;
        internal static NodeRegistry Instance => m_Instance ??= new NodeRegistry();

        private List<NodeInfo> m_NodeInfos = new List<NodeInfo>();
        internal static List<NodeInfo> NodeInfos => Instance.m_NodeInfos;

        internal Dictionary<Type, NodeInfo> m_TypeToNodeInfo = new Dictionary<Type, NodeInfo>();

        internal Dictionary<SerializableGUID, NodeInfo> m_TypeIDToNodeInfo =
            new Dictionary<SerializableGUID, NodeInfo>();

        internal Dictionary<SerializableGUID, ConditionInfo> m_TypeIDToConditionInfo =
            new Dictionary<SerializableGUID, ConditionInfo>();

        internal Dictionary<Type, Type> m_RuntimeNodeTypeToNodeModelType = new Dictionary<Type, Type>();
        public HashSet<string> NodeCategories { get; private set; } = new HashSet<string>();

        NodeRegistry()
        {
            Init();
            m_Instance = this;
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
#endif
        }

        ~NodeRegistry()
        {
            if (m_Instance == this)
            {
                m_Instance = null;
            }
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
#endif
        }

        void Init()
        {
            m_TypeToNodeInfo.Clear();
            m_TypeIDToNodeInfo.Clear();
            m_TypeIDToConditionInfo.Clear();
            m_NodeInfos.Clear();

            PopulateRuntimeNodeToNodeModelDictionary();
#if UNITY_EDITOR
            PopulateTypeInfo();
            PopulateConditionTypeInfo();
#endif
        }

        private void PopulateRuntimeNodeToNodeModelDictionary()
        {
#if UNITY_EDITOR
            List<Type> typeList = UnityEditor.TypeCache.GetTypesWithAttribute<NodeModelInfoAttribute>()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(NodeModel).IsAssignableFrom(type))
                .ToList();
#else
            List<Type> typeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && typeof(NodeModel).IsAssignableFrom(type))
                ).ToList();
#endif
            foreach (Type type in typeList)
            {
                IEnumerable<NodeModelInfoAttribute> attributes = type.GetCustomAttributes<NodeModelInfoAttribute>();
                if (attributes == null || attributes.Count() == 0)
                {
                    continue;
                }

                foreach (NodeModelInfoAttribute attribute in attributes)
                {
                    m_RuntimeNodeTypeToNodeModelType[attribute.Type] = type;
                }
            }
        }

        internal static NodeInfo GetInfo(Type type)
        {
            Instance.m_TypeToNodeInfo.TryGetValue(type, out NodeInfo info);
            return info;
        }

        internal static NodeInfo GetInfoFromTypeID(SerializableGUID typeID)
        {
            Instance.m_TypeIDToNodeInfo.TryGetValue(typeID, out NodeInfo info);
            return info;
        }

        public static ConditionInfo GetConditionInfoFromTypeID(SerializableGUID typeID)
        {
            Instance.m_TypeIDToConditionInfo.TryGetValue(typeID, out ConditionInfo info);
            return info;
        }

#if UNITY_EDITOR
        private void PopulateConditionTypeInfo()
        {
            IEnumerable<Type> typeList = ConditionUtility.GetConditionTypes();
            foreach (Type type in typeList)
            {
                try
                {
                    ConditionAttribute attribute = type.GetCustomAttribute<ConditionAttribute>();
                    if (attribute == null || m_TypeIDToConditionInfo.ContainsKey(attribute.GUID))
                    {
                        continue;
                    }

                    var info = ConditionUtility.GetInfoForConditionType(type);
                    m_TypeIDToConditionInfo[info.TypeID] = info;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.Log(ex);
                }
            }
        }
#endif

#if UNITY_EDITOR
        private void PopulateTypeInfo()
        {
#if UNITY_EDITOR
            // Use the much faster TypeCache in editor.
            List<Type> typeList = UnityEditor.TypeCache.GetTypesWithAttribute<NodeDescriptionAttribute>()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Node)))
                .ToList();
#else
            List<Type> typeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Node)))
                ).ToList();
#endif

            foreach (Type type in typeList)
            {
                try
                {
                    NodeDescriptionAttribute attribute = type.GetCustomAttribute<NodeDescriptionAttribute>();
                    if (attribute == null || m_TypeIDToNodeInfo.ContainsKey(attribute.GUID))
                    {
                        continue; // skip nodes with missing attributes (start/test nodes) or that have already been covered
                    }

                    m_RuntimeNodeTypeToNodeModelType.TryGetValue(type, out Type modelType);
                    if (modelType == null)
                    {
                        if (type.IsSubclassOf(typeof(Action)))
                        {
                            if (type.IsSubclassOf(typeof(EventAction)))
                            {
                                modelType = typeof(EventNodeModel);
                            }
                            else
                            {
                                modelType = typeof(ActionNodeModel);
                            }
                        }
                        else if (type.IsSubclassOf(typeof(Composite)))
                        {
                            if (type == typeof(SwitchComposite))
                            {
                                modelType = typeof(SwitchNodeModel);
                            }
                            else
                            {
                                modelType = typeof(CompositeNodeModel);
                            }
                        }
                        else if (type.IsSubclassOf(typeof(Join)))
                        {
                            modelType = typeof(JoinNodeModel);
                        }
                        else if (type.IsSubclassOf(typeof(Modifier)))
                        {
                            modelType = typeof(ModifierNodeModel);
                        }
                    }

                    var info = new NodeInfo
                    {
                        SerializableType = new SerializableType(type),
                        TypeID = attribute.GUID,
                        ModelType = new SerializableType(modelType),
                        Name = attribute.Name.Length != 0 ? attribute.Name : Util.NicifyVariableName(type.Name),
                        Description = attribute.Description,
                        Icon = null, //todo fix: cannot call Load during serialization.  attribute.Icon.Length == 0 ? null : Resources.Load<Texture2D>(attribute.Icon),
                        Category = attribute.Category,
                        FilePath = attribute.FilePath,
                        HideInSearch = attribute.HideInSearch,
                        NamedChildren = GetNamesOfChildren(type).ToList(),
                        StoryInfo = new StoryInfo
                        {
                            Story = attribute.Story, Variables = GetNodeVariables(type),
                            StoryVariableNames = GetStoryVariableNames(attribute.Story)
                        },
                    };

                    if (info.Category.Length > 0)
                    {
                        NodeCategories.Add(info.Category);
                    }

                    m_TypeToNodeInfo[type] = info;
                    m_TypeIDToNodeInfo[info.TypeID] = info;
                    m_NodeInfos.Add(info);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.Log(ex);
                }
            }
        }
#endif

        internal static string GetVariableTooltip(MemberInfo member)
        {
            TooltipAttribute tooltipAttribute = member.GetCustomAttribute<TooltipAttribute>(true);
            if (tooltipAttribute == null)
            {
                return String.Empty;
            }

            return tooltipAttribute.tooltip;
        }

        internal static List<VariableInfo> GetNodeVariables(Type type)
        {
            List<VariableInfo> variables = new List<VariableInfo>();
            var objInstance = Activator.CreateInstance(type);
            PropertyInfo[] properties =
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                if (typeof(BlackboardVariable).IsAssignableFrom(property.PropertyType))
                {
                    variables.Add(new VariableInfo
                    {
                        Name = property.Name, Type = property.PropertyType,
                        DefaultValue = property.GetValue(objInstance), Tooltip = GetVariableTooltip(property)
                    });
                }
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (typeof(BlackboardVariable).IsAssignableFrom(field.FieldType))
                {
                    variables.Add(new VariableInfo
                    {
                        Name = field.Name, Type = field.FieldType, DefaultValue = field.GetValue(objInstance),
                        Tooltip = GetVariableTooltip(field)
                    });
                }
            }

            return variables;
        }

#if UNITY_EDITOR
        internal static List<string> GetStoryVariableNames(string story)
        {
            List<string> splits = story.Split(' ').ToList();
            List<string> storyVariables = new List<string>();

            foreach (string split in splits)
            {
                if (split.StartsWith("[") && split.EndsWith("]"))
                {
                    string splitWithoutBrackets = split.Substring(1, split.Length - 2);
                    splitWithoutBrackets =
                        GeneratorUtils.RemoveSpaces(GeneratorUtils.NicifyString(splitWithoutBrackets));
                    storyVariables.Add(splitWithoutBrackets);
                }
            }

            return storyVariables;
        }
#endif

        private static IEnumerable<string> GetNamesOfChildren(Type type)
        {
            FieldInfo[] fields =
                type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (typeof(Node).IsAssignableFrom(field.FieldType))
                {
                    yield return field.Name;
                }
            }
        }

#if UNITY_EDITOR
        private void OnAfterAssemblyReload()
        {
            Instance.Init();
        }
#endif
    }
}