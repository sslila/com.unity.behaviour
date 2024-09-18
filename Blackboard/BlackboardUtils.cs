using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior.GraphFramework.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_EDITOR
// Don't remove this using (even if Rider claims it is unused), it is used in a runtime only part in GetEnumVariableTypes() 
using System.Reflection;
#endif
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    internal static class BlackboardUtils
    {
        private static Dictionary<Type, Texture2D> m_VariableTypeIcons = new()
        {
            {
                typeof(bool),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/boolean_icon.png")
            },
            {
                typeof(double),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/float_icon.png")
            },
            {
                typeof(string),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/string_icon.png")
            },
            {
                typeof(Color),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/color_icon.png")
            },
            {
                typeof(float),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/float_icon.png")
            },
            {
                typeof(int),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/integer_icon.png")
            },
            {
                typeof(Transform),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/position_icon.png")
            },
            {
                typeof(Vector2),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/vector2_icon.png")
            },
            {
                typeof(Vector3),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/vector3_icon.png")
            },
            {
                typeof(Vector4),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/vector4_icon.png")
            },
            {
                typeof(GameObject),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/object_icon.png")
            },
            {
                typeof(UnityEngine.Object),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/object_icon.png")
            },
            {
                typeof(Enum),
                ResourceLoadAPI.Load<Texture2D>("Packages/com.unity.behavior/Blackboard/Assets/Icons/enum_icon.png")
            }
        };

        public static Texture2D GetIcon(this Type type)
        {
            if (type == null)
            {
                return ResourceLoadAPI.Load<Texture2D>(
                    "Packages/com.unity.behavior/Blackboard/Assets/Icons/variable_icon.png");
            }

            if (type.IsEnum)
                return m_VariableTypeIcons[typeof(Enum)];

            if (m_VariableTypeIcons.TryGetValue(type, out var texture2D))
            {
                return texture2D;
            }

            return GetIcon(type.BaseType);
        }

        public static void AddCustomIcon(Type typeKey, Texture2D texture)
        {
            if (!m_VariableTypeIcons.ContainsKey(typeKey))
            {
                m_VariableTypeIcons.Add(typeKey, texture);
            }
        }

        public static Texture2D GetScriptableObjectIcon(ScriptableObject obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                // Get the icon of the ScriptableObject
                Texture2D iconTexture = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image as Texture2D;
                return iconTexture;
#endif
            }

            return null;
        }

        public static string GetArrowUnicode()
        {
            return "\u2192";
        }

        public static string GetNameForType(Type type)
        {
            if (type == typeof(float))
            {
                return "Float";
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                Type elementType = type.GetGenericArguments()[0];
                return $"{elementType.Name} List";
            }

            return type.Name;
        }

        public static Type GetVariableModelTypeForType(Type type)
        {
            if (TypedVariableModelMap.TryGetValue(type, out var typedVariable))
            {
                return typedVariable;
            }

            throw new NotImplementedException($"No mapping found for: {type}");
            // if (type == typeof(int))
            // {
            //     Debug.LogError($"Return type as int");
            //     return typeof(TypedVariableModelInt);
            // }
            //
            // if (type == typeof(float))
            // {
            //     return typeof(TypedVariableModelFloat);
            // }
            //
            // return typeof(TypedVariableModel<>).MakeGenericType(type);
        }

        private static readonly Dictionary<Type, Type> TypedVariableModelMap = new Dictionary<Type, Type>()
        {
            { typeof(bool), typeof(TypedVariableModelBool) },
            { typeof(List<bool>), typeof(TypedVariableModelBoolList) },
            { typeof(int), typeof(TypedVariableModelInt) },
            { typeof(List<int>), typeof(TypedVariableModelIntList) },
            { typeof(float), typeof(TypedVariableModelFloat) },
            { typeof(List<float>), typeof(TypedVariableModelFloatList) },
            { typeof(double), typeof(TypedVariableModelDouble) },
            { typeof(List<double>), typeof(TypedVariableModelDoubleList) },
            { typeof(string), typeof(TypedVariableModelString) },
            { typeof(List<string>), typeof(TypedVariableModelStringList) },
            //Unity

            { typeof(Vector2), typeof(TypedVariableModelVector2) },
            { typeof(List<Vector2>), typeof(TypedVariableModelVector2List) },
            { typeof(Vector2Int), typeof(TypedVariableModelVector2Int) },
            { typeof(List<Vector2Int>), typeof(TypedVariableModelVector2IntList) },
            { typeof(Vector3), typeof(TypedVariableModelVector3) },
            { typeof(List<Vector3>), typeof(TypedVariableModelVector3IntList) },
            { typeof(Vector3Int), typeof(TypedVariableModelVector3Int) },
            { typeof(List<Vector3Int>), typeof(TypedVariableModelVector3IntList) },
            { typeof(Vector4), typeof(TypedVariableModelVector4) },
            { typeof(List<Vector4>), typeof(TypedVariableModelVector4List) },
            { typeof(Color), typeof(TypedVariableModelColor) },
            { typeof(List<Color>), typeof(TypedVariableModelColorList) },

            { typeof(GameObject), typeof(TypedVariableModelGameObject) },
            { typeof(List<GameObject>), typeof(TypedVariableModelGameObjectList) },

            { typeof(ScriptableObject), typeof(TypedVariableModelScriptableObject) },
            { typeof(List<ScriptableObject>), typeof(TypedVariableModelScriptableObjectList) },

            { typeof(Transform), typeof(TypedVariableModelTransform) },
            { typeof(List<Transform>), typeof(TypedVariableModelTransformList) },
        };
    }
}