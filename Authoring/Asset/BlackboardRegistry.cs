#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.Behavior
{
    internal class BlackboardRegistry
    {
        public static List<BlackboardOption> GetDefaultBlackboardOptions()
        {
            return new List<BlackboardOption>
            {
                new BlackboardOption(typeof(GameObject), "GameObject", "object", priority: 2),
                new BlackboardOption(typeof(Transform), "Transform", "transform", priority: 1),
                new BlackboardOption(typeof(string), "Basic Types/String"),
                new BlackboardOption(typeof(float), "Basic Types/Float", "float"),
                new BlackboardOption(typeof(int), "Basic Types/Integer", "integer"),
                new BlackboardOption(typeof(double), "Basic Types/Double"),
                new BlackboardOption(typeof(bool), "Basic Types/Boolean"),
                new BlackboardOption(typeof(Vector2), "Vector Types/Vector2"),
                new BlackboardOption(typeof(Vector3), "Vector Types/Vector3"),
                new BlackboardOption(typeof(Vector4), "Vector Types/Vector4"),
                new BlackboardOption(typeof(Vector2Int), "Vector Types/Vector2 Int"),
                new BlackboardOption(typeof(Vector3Int), "Vector Types/Vector3 Int"),
                new BlackboardOption(typeof(Color), "Basic Types/Color"),

                // Resource Types
                new BlackboardOption(typeof(ScriptableObject), "Resources/Scriptable Object"),
                new BlackboardOption(typeof(Texture2D), "Resources/Texture2D"),
                new BlackboardOption(typeof(Sprite), "Resources/Sprite"),
                new BlackboardOption(typeof(Material), "Resources/Material"),
                new BlackboardOption(typeof(AudioClip), "Resources/Audio Clip"),
                new BlackboardOption(typeof(AudioClip), "Resources/Audio Resource"),
                new BlackboardOption(typeof(AnimationClip), "Resources/Animation Clip"),
                new BlackboardOption(typeof(AudioMixer), "Resources/Audio Mixer"),
                new BlackboardOption(typeof(TextAsset), "Resources/Text Asset"),
                new BlackboardOption(typeof(ParticleSystem), "Resources/Particle System"),
              
                // List Types
                new BlackboardOption(typeof(List<GameObject>), "List/Game Object List", "object"),
                new BlackboardOption(typeof(List<string>), "List/String List", "string"),
                new BlackboardOption(typeof(List<float>), "List/Float List", "float"),
                new BlackboardOption(typeof(List<int>), "List/Integer List", "integer"),
                new BlackboardOption(typeof(List<double>), "List/Double List", "double"),
                new BlackboardOption(typeof(List<bool>), "List/Boolean List", "boolean"),
                new BlackboardOption(typeof(List<Vector2>), "List/Vector2 List", "vector2"),
                new BlackboardOption(typeof(List<Vector3>), "List/Vector3 List", "vector3"),
                new BlackboardOption(typeof(List<Vector4>), "List/Vector4 List", "vector4"),
                new BlackboardOption(typeof(List<Vector2Int>), "List/Vector2 Int List", "vector2"),
                new BlackboardOption(typeof(List<Vector3Int>), "List/Vector3 Int List", "vector3"),
                new BlackboardOption(typeof(List<Color>), "List/Color List", "color"),
                
                // Behavior Types
                new BlackboardOption(typeof(BehaviorGraph), "Behavior/Subgraph", "object"),
            };
        }

        public static List<BlackboardOption> GetComparsionOperators() {
            return new List<BlackboardOption>
            {
                new BlackboardOption(typeof(ConditionOperator), "Comparsion Operators/Operator (All)"),
                new BlackboardOption(typeof(BooleanOperator), "Comparsion Operators/Operator (Boolean)"),
            };
        }

        public static List<BlackboardOption> GetEnumVariableTypes()
        {
            var enumOptions = new List<BlackboardOption>();
#if UNITY_EDITOR
            var enumTypes = UnityEditor.TypeCache.GetTypesWithAttribute<BlackboardEnumAttribute>()
                .Where(type => type.IsEnum && Enum.GetValues(type).Length > 0);
            foreach (var type in enumTypes)
            {
                enumOptions.Add(new BlackboardOption(type, "Enumeration/" + Util.NicifyVariableName(type.Name)));
            }
#else
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    // we don't consider empty enum types
                    if (!type.IsEnum || type.GetCustomAttribute<BlackboardEnumAttribute>() == null || Enum.GetValues(type).Length == 0)
                    {
                        continue;
                    }
                    enumOptions.Add(new BlackboardOption(type, "Enumeration/" + Util.NicifyVariableName(type.Name)));
                }
            }
#endif
            return enumOptions;
        }

        public static List<BlackboardOption> GetStoryVariableTypes()
        {
            var options = GetDefaultBlackboardOptions();
            var enums = GetEnumVariableTypes();
            options.Insert(0, new BlackboardOption { Type = typeof(RegularText) , Path = "Regular Text" });

            AddCustomTypes<Behaviour>(options, "Other/MonoBehaviours");
            AddCustomTypes<ScriptableObject>(options, "Other/ScriptableObjects");

            options.AddRange(enums);

            return options;
        }

        public static List<BlackboardOption> GetStoryVariableTypesWithOperators()
        {
            var options = GetStoryVariableTypes();
            var operators = GetComparsionOperators();
            options.AddRange(operators);

            return options;
        }

        private static List<BlackboardOption> m_CustomVariableOptions = null;
        public static List<BlackboardOption> GetCustomTypes()
        {
            if (m_CustomVariableOptions == null)
            {
                m_CustomVariableOptions = new List<BlackboardOption>();
                AddCustomTypes<Behaviour>(m_CustomVariableOptions, "MonoBehaviours");
                AddCustomTypes<ScriptableObject>(m_CustomVariableOptions, "ScriptableObjects");
            }
            return m_CustomVariableOptions;
        }


        public static List<BlackboardOption> AddCustomTypes<TypeName>(List<BlackboardOption> options, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }
            else
            {
                path += "/";
            }
#if UNITY_EDITOR
            var monobehaviourTypes = UnityEditor.TypeCache.GetTypesDerivedFrom<TypeName>();
            foreach (var type in monobehaviourTypes)
            {
#else
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
#endif
                if (type.IsNotPublic || !type.IsVisible || IsExcludedNamespaceOrType(type))
                {
                    continue;
                }
                string namespacePath = type.Namespace?.Replace('.', '/');
                if (!string.IsNullOrEmpty(namespacePath))
                {
                    namespacePath += "/";
                }
                options.Add(new BlackboardOption(type, $"{ path }{namespacePath}{ Util.NicifyVariableName(type.Name, detectAbbreviation: true) }"));
            }
#if !UNITY_EDITOR
            }
#endif
                return options;
        }

        private static readonly string[] kExcludedNamespaces =
            {
                "Editor",
                "Muse",
                "Behavior",
                "AppUI"
            };

        internal static bool IsExcludedNamespaceOrType(Type type)
        {
            if (typeof(EventChannelBase).IsAssignableFrom(type))
            {
                return true;
            }
            if (string.IsNullOrEmpty(type.Namespace))
            {
                return false;
            }

            foreach (string excludedNamespace in kExcludedNamespaces)
            {
                if (type.Namespace.Contains(excludedNamespace))
                {
                    return true;
                }
            }
            return false;
        }
    }    

    internal struct BlackboardOption
    {
        public string Name => System.IO.Path.GetFileName(Path);
        public string Path;
        public SerializableType Type;
        public string Icon;
        public int Priority;

        public BlackboardOption(SerializableType type, string path = null, string icon = null, int priority = 0)
        {
            Path = path;
            Type = type;
            Icon = icon;
            Priority = priority;

            if (string.IsNullOrEmpty(path))
            {
                Path = Util.NicifyVariableName(type.Type.Name);
            }
            if (string.IsNullOrEmpty(icon))
            {
                Icon = type.Type.Name.ToLower();
            }
        }
    }
}
#endif