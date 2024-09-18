#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.Behavior.GraphFramework
{
    internal class NodeRegistry
    {
        static NodeRegistry m_Instance;
        static NodeRegistry Instance => m_Instance ??= new NodeRegistry();

        public static Type GetUIType(Type type) => Instance.m_NodeModelTypeToUIType[type];
        public static Type GetNodeModelType(Type type) => Instance.m_UITypeToNodeModelType[type];

        public static Type GetInspectorUIType(Type type) => Instance.m_NodeModelTypeToInspectorUIType.TryGetValue(type, out Type InspectorUIType) ? InspectorUIType : null;

        public static Type GetVariableUIType(Type type)
        {
            if (Instance.m_VariableModelTypeToUIType.TryGetValue(type, out var uiType))
            {
                return uiType;
            }

            // Type should always be of TypedVariableModel<T> and therefore generic.
            var genericArgs = type.GenericTypeArguments;
            if (genericArgs != null && genericArgs.Length > 0)
            {
                if (genericArgs[0].IsEnum)
                {
                    return typeof(EnumVariableElement);
                }
                if (typeof(UnityEngine.Object).IsAssignableFrom(genericArgs[0])) {
                    return typeof(ObjectVariableElement);
                }
            }

            UnityEngine.Debug.LogError($"No UI type found for {type.Name}.");

            return null;
        }

        Dictionary<Type, Type> m_NodeModelTypeToUIType = new Dictionary<Type, Type>();
        Dictionary<Type, Type> m_UITypeToNodeModelType = new Dictionary<Type, Type>();

        Dictionary<Type, Type> m_NodeModelTypeToInspectorUIType = new Dictionary<Type, Type>();
        Dictionary<Type, Type> m_InspectorUITypeToNodeModelType = new Dictionary<Type, Type>();

        Dictionary<Type, Type> m_VariableModelTypeToUIType = new Dictionary<Type, Type>();
        Dictionary<Type, Type> m_UITypeToVariableModelType = new Dictionary<Type, Type>();

        internal bool m_Initialized => m_UITypeToNodeModelType.Count != 0;  

        NodeRegistry()
        {
            if (m_Instance == null)
            {            
                m_Instance = this;
                Init();
            }
        }

        ~NodeRegistry()
        {
            if (m_Instance == this)
            {
                m_Instance = null;
            }
        }

        void Init()
        {
            QueryAttributes<NodeUIAttribute, NodeUI>(ref Instance.m_NodeModelTypeToUIType, ref Instance.m_UITypeToNodeModelType);
            QueryAttributes<NodeInspectorUIAttribute, NodeInspectorUI>(ref Instance.m_NodeModelTypeToInspectorUIType, ref Instance.m_InspectorUITypeToNodeModelType);
            QueryAttributes<VariableUIAttribute, BlackboardVariableElement>(ref Instance.m_VariableModelTypeToUIType, ref Instance.m_UITypeToVariableModelType);
        }


        public static void RegisterVariableModelUI<ModelType, VariableUIType>()
            where ModelType : VariableModel
            where VariableUIType : BlackboardVariableElement
        {
            RegisterVariableModelUI(typeof(ModelType), typeof(VariableUIAttribute));
        }

        public static void RegisterVariableModelUI(Type modelType, Type variableUI)
        {
            Instance.m_VariableModelTypeToUIType[modelType] = variableUI;
            Instance.m_UITypeToVariableModelType[variableUI] = modelType;
        }

        void QueryAttributes<AttributeType, UIClassType>(ref Dictionary<Type, Type> modelToUI,
            ref Dictionary<Type, Type> UIToModel) where AttributeType : BaseUIAttribute
        {
#if UNITY_EDITOR
            List<Type> typeList = UnityEditor.TypeCache.GetTypesWithAttribute<AttributeType>()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(UIClassType).IsAssignableFrom(type))
                .ToList();
#else
            List<Type> typeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && typeof(UIClassType).IsAssignableFrom(type))
                ).ToList();
#endif
            foreach (Type uiType in typeList)
            {
                IEnumerable<AttributeType> attributes = uiType.GetCustomAttributes<AttributeType>();
                if (attributes == null || attributes.Count() == 0)
                { 
                    continue;
                }

                foreach (AttributeType attribute in attributes)
                {
                    modelToUI[attribute.Type] = uiType;
                    UIToModel[uiType] = attribute.Type;
                }
            }
        }
    }
}
#endif