#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;
using ColorField = Unity.AppUI.UI.ColorField;
using DoubleField = Unity.AppUI.UI.DoubleField;
using FloatField = Unity.AppUI.UI.FloatField;
using IntegerField = Unity.AppUI.UI.IntField;
using TextField = Unity.AppUI.UI.TextField;
using Toggle = Unity.AppUI.UI.Toggle;
using Vector2Field = Unity.AppUI.UI.Vector2Field;
using Vector3Field = Unity.AppUI.UI.Vector3Field;
using Vector4Field = Unity.AppUI.UI.Vector4Field;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Behavior.GraphFramework.Generic;
using Vector2IntField = Unity.AppUI.UI.Vector2IntField;
using Vector3IntField = Unity.AppUI.UI.Vector3IntField;

namespace Unity.Behavior.GraphFramework
{
    internal class TypedVariableElement<T, FieldType> : BlackboardVariableElement where FieldType : VisualElement, new()
    {
        protected FieldType m_Field;

        public TypedVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            m_Field = new FieldType();
            if (variableModel.ID == k_ReservedID)
            {
                m_Field.SetEnabled(false);
                LocalizedTextElement selfTextElement = new LocalizedTextElement("The GameObject running the graph.");
                tooltip = selfTextElement.tooltip = m_Field.tooltip = "This variable will be assigned with the GameObject the graph is running on.\n"
                    + "It can be assigned to a different GameObject via the BehaviorAgent component.";
                Add(selfTextElement);
            }
            else
            {
                Add(m_Field);
            }

            if (m_Field is INotifyValueChanged<T> element)
            {
                element.RegisterValueChangedCallback(OnValueChanged);   
                element.SetValueWithoutNotify((T)variableModel.ObjectValue); 
            } 
            else if (m_Field is INotifyValueChanged<IEnumerable<int>> enumerableElement)
            {
                enumerableElement.RegisterValueChangedCallback(OnValueChanged);
            }
        }
        
        private void OnValueChanged(ChangeEvent<IEnumerable<int>> evt)
        {
            m_View.Dispatcher.DispatchImmediate(new SetBlackboardVariableValueCommand(VariableModel, evt.newValue.FirstOrDefault()));
        }

        private void OnValueChanged(ChangeEvent<T> evt)
        {
            m_View.Dispatcher.DispatchImmediate(new SetBlackboardVariableValueCommand(VariableModel, evt.newValue));
        }
    }
    
    internal class TypedListVariableElement<ValueType, BaseValueType> : TypedVariableElement<ValueType, ListView> where ValueType : BaseValueType
    {
        protected TypedListVariableElement(BlackboardView view, VariableModel variableModel, Type fieldType) : base(view, variableModel)
        {
            CreateListView(m_Field, new List<ValueType>(), fieldType);
        }

        private void CreateListView(ListView listView, List<ValueType> elements, Type fieldType)
        {
            if (VariableModel.ObjectValue != null)
            {
                elements = (List<ValueType>)VariableModel.ObjectValue;
            }
            else
            {
                VariableModel.ObjectValue = elements;
            }
            
            listView.itemsSource = elements;
            listView.showAddRemoveFooter = true;
            listView.makeItem = () =>
            {
                ListItemElement listItem = new ListItemElement(fieldType);
                listItem.Field.RegisterValueChangedCallback(delegate(ChangeEvent<BaseValueType> evt)
                {
                    elements[listItem.Index] = (ValueType)evt.newValue;
                    VariableModel.ObjectValue = elements;
                    m_View.Dispatcher.DispatchImmediate(new SetBlackboardVariableValueCommand(VariableModel, VariableModel.ObjectValue));
                });
                return listItem;
            };
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.showBorder = true;
            listView.bindItem = (element, i) =>
            {
                ListItemElement listItem = element as ListItemElement;
                if (listItem == null)
                {
                    return;
                }
                listItem.Index = i;
                listItem.Init(i, elements[i]);
            };
            listView.Q<UnityEngine.UIElements.Button>("unity-list-view__add-button").clicked += () =>
            {
                m_View.Dispatcher.DispatchImmediate(new SetBlackboardVariableValueCommand(VariableModel, VariableModel.ObjectValue));
            };
            listView.Q<UnityEngine.UIElements.Button>("unity-list-view__remove-button").clicked += () =>
            {
                m_View.Dispatcher.DispatchImmediate(new SetBlackboardVariableValueCommand(VariableModel, VariableModel.ObjectValue));
            };
        }

        private class ListItemElement : VisualElement
        {
            private readonly Label m_Label;
            public readonly INotifyValueChanged<BaseValueType> Field;
        
            public int Index;

            public ListItemElement(Type fieldElementType)
            {
                name = "ListVariableListViewItem";
                m_Label = new Label();
#if UNITY_EDITOR
                if (fieldElementType == typeof(UnityEditor.UIElements.ObjectField))
                {
                    UnityEditor.UIElements.ObjectField objectField = new UnityEditor.UIElements.ObjectField();
                    objectField.allowSceneObjects = false;
                    objectField.objectType = typeof(ValueType);
                    Field = (INotifyValueChanged<BaseValueType>)objectField;
                }
                else
#endif
                {
                    Field = (INotifyValueChanged<BaseValueType>)Activator.CreateInstance(fieldElementType);
                }

                Add(m_Label);
                Add(Field as VisualElement);
            }

            public void Init(int index, ValueType value)
            {
                m_Label.text = $"Element {index}";
                m_Label.AddToClassList("ListVariableItemLabel");
                Field.value = value;
            }
        }
    }

    [VariableUI(typeof(TypedVariableModel<int>)), VariableUI(typeof(TypedVariableModelInt))]
    internal class IntVariableElement : TypedVariableElement<int, IntegerField>
    {
        public IntVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            IntegerField field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<int>>)), VariableUI(typeof(TypedVariableModelIntList))]
    internal class IntListVariableElement : TypedListVariableElement<int, int>
    {
        public IntListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(IntegerField))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<float>)), VariableUI(typeof(TypedVariableModelFloat))]
    internal class FloatVariableElement : TypedVariableElement<float, FloatField>
    {
        public FloatVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            FloatField field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<float>>)), VariableUI(typeof(TypedVariableModelFloatList))]
    internal class FloatListVariableElement : TypedListVariableElement<float, float>
    {
        public FloatListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(FloatField))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<double>)), VariableUI(typeof(TypedVariableModelDouble))]
    internal class DoubleVariableElement : TypedVariableElement<double, DoubleField>
    {
        public DoubleVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            DoubleField field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<double>>)), VariableUI(typeof(TypedVariableModelDoubleList))]
    internal class DoubleListVariableElement : TypedListVariableElement<double, double>
    {
        public DoubleListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(DoubleField))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<bool>)), VariableUI(typeof(TypedVariableModelBool))]
    internal class BoolVariableElement : TypedVariableElement<bool, Toggle>
    {
        public BoolVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            Toggle toggle = m_Field;
            toggle.label = toggle.value.ToString();
            m_Field.RegisterValueChangedCallback(evt =>
            {
                m_Field.label = evt.newValue.ToString();
            });
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<bool>>)), VariableUI(typeof(TypedVariableModelBoolList))]
    internal class BoolListVariableElement : TypedListVariableElement<bool, bool>
    {
        public BoolListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(Toggle))
        {
        }
    }
    
#if UNITY_EDITOR
    [VariableUI(typeof(TypedVariableModel<Color>)), VariableUI(typeof(TypedVariableModelColor))]
    internal class ColorVariableElement : TypedVariableElement<Color, ColorField>
    {
        public ColorVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            ColorField field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<Color>>)), VariableUI(typeof(TypedVariableModelColorList))]
    internal class ColorListVariableElement : TypedListVariableElement<Color, Color>
    {
        public ColorListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(ColorField))
        {
        }
    }
#endif
    [VariableUI(typeof(TypedVariableModel<Vector2>)), VariableUI(typeof(TypedVariableModelVector2))]
    internal class Vector2VariableElement : TypedVariableElement<Vector2, Vector2Field>
    {
        public Vector2VariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            Vector2Field field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<Vector2>>)), VariableUI(typeof(TypedVariableModelColorList))]
    internal class Vector2ListVariableElement : TypedListVariableElement<Vector2, Vector2>
    {
        public Vector2ListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(Vector2Field))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<Vector3>)), VariableUI(typeof(TypedVariableModelVector3))]
    internal class Vector3VariableElement : TypedVariableElement<Vector3, Vector3Field>
    {
        public Vector3VariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            Vector3Field field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<Vector3>>)), VariableUI(typeof(TypedVariableModelVector3List))]
    internal class Vector3ListVariableElement : TypedListVariableElement<Vector3, Vector3>
    {
        public Vector3ListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(Vector3Field))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<Vector4>)), VariableUI(typeof(TypedVariableModelVector4))]
    internal class Vector4VariableElement : TypedVariableElement<Vector4, Vector4Field>
    {
        public Vector4VariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            Vector4Field field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<Vector4>>)), VariableUI(typeof(TypedVariableModelVector4List))]
    internal class Vector4ListVariableElement : TypedListVariableElement<Vector4, Vector4>
    {
        public Vector4ListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(Vector4Field))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<Vector2Int>)), VariableUI(typeof(TypedVariableModelVector2Int))]
    internal class Vector2IntVariableElement : TypedVariableElement<Vector2Int, Vector2IntField>
    {
        public Vector2IntVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            Vector2IntField field = m_Field;
            field.size = Size.M;
        }
    }

    [VariableUI(typeof(TypedVariableModel<List<Vector2Int>>)), VariableUI(typeof(TypedVariableModelVector2IntList))]
    internal class Vector2IntListVariableElement : TypedListVariableElement<Vector2Int, Vector2Int>
    {
        public Vector2IntListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(Vector2IntField))
        {
        }
    }

    [VariableUI(typeof(TypedVariableModel<Vector3Int>)), VariableUI(typeof(TypedVariableModelVector3Int))]
    internal class Vector3IntVariableElement : TypedVariableElement<Vector3Int, Vector3IntField>
    {
        public Vector3IntVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            Vector3IntField field = m_Field;
            field.size = Size.M;
        }
    }

    [VariableUI(typeof(TypedVariableModel<List<Vector3Int>>)), VariableUI(typeof(TypedVariableModelVector3IntList))]
    internal class Vector3IntListVariableElement : TypedListVariableElement<Vector3Int, Vector3Int>
    {
        public Vector3IntListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(Vector3IntField))
        {
        }
    }


    [VariableUI(typeof(TypedVariableModel<string>)), VariableUI(typeof(TypedVariableModelString))]
    internal class StringVariableElement : TypedVariableElement<string, TextField>
    {
        public StringVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            TextField field = m_Field;
            field.size = Size.M;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<string>>)), VariableUI(typeof(TypedVariableModelStringList))]
    internal class StringListVariableElement : TypedListVariableElement<string, string>
    {
        public StringListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(TextField))
        {
        }
    }

#if UNITY_EDITOR
    internal class ObjectVariableElement : TypedVariableElement<UnityEngine.Object, UnityEditor.UIElements.ObjectField>
#else
    internal class ObjectVariableElement : TypedVariableElement<UnityEngine.Object, RuntimeObjectField>
#endif
    {
        public ObjectVariableElement(BlackboardView view, VariableModel variableModel)
            : base(view, variableModel)
        {
#if UNITY_EDITOR
            m_Field.objectType = variableModel.Type;
            m_Field.allowSceneObjects = false;
#endif
        }
    }

    [VariableUI(typeof(TypedVariableModel<List<ScriptableObject>>)), VariableUI(typeof(TypedVariableModelScriptableObject))]
    internal class ScriptableObjectListVariableElement : TypedListVariableElement<ScriptableObject, ScriptableObject>
    {
        public ScriptableObjectListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(RuntimeScriptableObjectField))
        {
        }
    }
    [VariableUI(typeof(TypedVariableModelGameObject))]
    internal class GameObjectVariableElement : TypedVariableElement<GameObject, UnityEditor.UIElements.ObjectField>
    {
        public GameObjectVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            m_Field.objectType = variableModel.Type;
            m_Field.allowSceneObjects = false;
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<List<GameObject>>)), VariableUI(typeof(TypedVariableModelGameObjectList))]
    internal class GameObjectListVariableElement : TypedListVariableElement<GameObject, UnityEngine.Object>
    {
#if UNITY_EDITOR
        public GameObjectListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(UnityEditor.UIElements.ObjectField))
#else
        public GameObjectListVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel, typeof(RuntimeGameObjectField))
#endif
        {
        }
    }
    
    [VariableUI(typeof(TypedVariableModel<Enum>)), VariableUI(typeof(TypedVariableModelEnum))]
    internal class EnumVariableElement : TypedVariableElement<Enum, Dropdown>
    {
        public EnumVariableElement(BlackboardView view, VariableModel variableModel) : base(view, variableModel)
        {
            m_Field.size = Size.M;
            Type variableEnumType = variableModel.ObjectValue.GetType();
            Array enumValues = Enum.GetValues(variableEnumType);
            m_Field.bindItem = (item, i) => item.label = Enum.GetName(variableEnumType, enumValues.GetValue(i));
            m_Field.sourceItems = enumValues;
            m_Field.SetValueWithoutNotify(new []{ (int)variableModel.ObjectValue });
        }
    }
}
#endif