#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.Scripting;
using ColorField = Unity.AppUI.UI.ColorField;
using Toggle = Unity.AppUI.UI.Toggle;
using DoubleField = Unity.AppUI.UI.DoubleField;
using FloatField = Unity.AppUI.UI.FloatField;
using IntegerField = Unity.AppUI.UI.IntField;
using Object = UnityEngine.Object;
using TextField = Unity.AppUI.UI.TextField;
using Vector2Field = Unity.AppUI.UI.Vector2Field;
using Vector3Field = Unity.AppUI.UI.Vector3Field;
using Vector4Field = Unity.AppUI.UI.Vector4Field;
using Vector2IntField = Unity.AppUI.UI.Vector2IntField;
using Vector3IntField = Unity.AppUI.UI.Vector3IntField;

namespace Unity.Behavior
{
    internal static class LinkFieldUtility
    {
        internal static BaseLinkField CreateNodeLinkField(string label, Type type)
        {
            BaseLinkField field = CreateForType(label, type);
            field.OnLinkChanged += _ =>
            {
                field.Dispatcher.DispatchImmediate(new SetNodeVariableLinkCommand(field.Model as NodeModel, field.FieldName, field.LinkVariableType, field.LinkedVariable, true));
            };
            field.RegisterCallback<LinkFieldValueChangeEvent>(evt =>
            {
                field.Dispatcher.DispatchImmediate(new SetNodeVariableValueCommand(field.Model as NodeModel, field.FieldName, evt.Value, true));
            });

            return field;
        }

        internal static BaseLinkField CreateConditionLinkField(string label, Type type, ConditionModel condition)
        {
            BaseLinkField field = CreateForType(label, type);
            field.OnLinkChanged += _ =>
            {
                field.Dispatcher.DispatchImmediate(new SetConditionVariableLinkCommand(condition, field.FieldName, field.LinkVariableType, field.LinkedVariable, true));
            };
            field.RegisterCallback<LinkFieldValueChangeEvent>(evt =>
            {
                field.Dispatcher.DispatchImmediate(new SetConditionVariableValueCommand(condition, field.FieldName, evt.Value, true));
            });

            return field;
        }

        internal static BaseLinkField CreateForType(string label, Type type)
        {
#if UNITY_EDITOR || UNITY_2022_1_OR_NEWER
            if (type == typeof(BlackboardVariable))
            {
                return new BaseLinkField();
            }
            
            if (typeof(BlackboardVariable).IsAssignableFrom(type))
            {
                type = type.GetProperty("Value").PropertyType;
            }

            if (type == typeof(float))
            {
                return new BehaviorLinkField<float, FloatField>();
            }
            if (type == typeof(double))
            {
                return new BehaviorLinkField<double, DoubleField>();
            }
            if (type == typeof(int))
            {
                return new BehaviorLinkField<int, IntegerField>();
            }
            if (type == typeof(bool))
            {
                return new BehaviorLinkField<bool, Toggle>();
            }
            if (type.IsEnum)
            {
                Type enumFieldType = typeof(EnumLinkField<>).MakeGenericType(type);
                return Activator.CreateInstance(enumFieldType) as BaseLinkField;
            }
            if (type == typeof(Vector2))
            {
                return new BehaviorLinkField<Vector2, Vector2Field>();
            }
            if (type == typeof(Vector3))
            {
                return new BehaviorLinkField<Vector3, Vector3Field>();
            }
            if (type == typeof(Vector4))
            {
                return new BehaviorLinkField<Vector4, Vector4Field>();
            }
            if (type == typeof(Vector2Int))
            {
                return new BehaviorLinkField<Vector2Int, Vector2IntField>();
            }
            if (type == typeof(Vector3Int))
            {
                return new BehaviorLinkField<Vector3Int, Vector3IntField>();
            }
            if (type == typeof(Color))
            {
#if UNITY_EDITOR
                return new BehaviorLinkField<Color, ColorField>();
#else
                return null;
#endif
            }
            if (type == typeof(string))
            {
                return new BehaviorLinkField<string, TextField>();
            }
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                var field = new BehaviorLinkField<ScriptableObject, RuntimeScriptableObjectField>() { LinkVariableType = type };
                field.Field.label = Util.NicifyVariableName(label).Replace("  ", " ");
                // todo: Should this be done by the caller instead of here?
                if (typeof(EventChannelBase).IsAssignableFrom(type))
                {
                    field.SetFieldIcon(typeof(EventChannelBase));
                }
                return field;
            }
            if (typeof(Object).IsAssignableFrom(type))
            {
                var field = new BehaviorLinkField<Object, RuntimeObjectField>();
                field.Field.label = Util.NicifyVariableName(label).Replace("  ", " ");
                field.LinkVariableType = type;
                field.AllowAssetEmbeds = true;
                return field;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) 
            {
                Type listType = type.GetGenericArguments()[0];
                return CreateFieldForListType(listType, label);
            }
            
#endif
            return null;
        }

        [Preserve]
        private static LinkField<List<T>, RuntimeListField<T>> CreateFieldForListTypeGeneric<T>(string label)
        {
            LinkField<List<T>, RuntimeListField<T>> field =
                new BehaviorLinkField<List<T>, RuntimeListField<T>>();
            RuntimeListField<T> listField = field.Field;
            listField.label = Util.NicifyVariableName(label).Replace("  ", " ");
            return field;
        }
        
        private static BaseLinkField CreateFieldForListType(Type itemType, string label)
        {
            MethodInfo method = typeof(LinkFieldUtility).GetMethod(nameof(CreateFieldForListTypeGeneric), 
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo specialized = method.MakeGenericMethod(itemType);
            return specialized.Invoke(null, new object[] { label }) as BaseLinkField;
        }
    }
}
#endif