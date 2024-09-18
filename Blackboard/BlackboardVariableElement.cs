#if UNITY_EDITOR
using Unity.AppUI.UI;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.UIElements;
using ContextMenu = Unity.Behavior.GraphFramework.ContextMenu;
using TextField = Unity.AppUI.UI.TextField;

internal class BlackboardVariableElement : VisualElement
{
    public static readonly SerializableGUID k_ReservedID = new SerializableGUID(1, 0);

    public override VisualElement contentContainer => m_Content;

    private VariableModel m_VariableModel;
    public VariableModel VariableModel
    {
        get => m_VariableModel;
        internal set => m_VariableModel = value;
    }

    public string Name
    {
        get => m_NameLabel.text;
        set
        {
            m_NameLabel.text = value;
            m_NameField.SetValueWithoutNotify(value);
        }
    }

    public string VariableType
    {
        get => m_VariableTypeLabel.text;
        set => m_VariableTypeLabel.text = value;
    }

    internal delegate void NameChangedCallback(string name, VariableModel variable);
    internal NameChangedCallback OnNameChanged;

    protected BlackboardView m_View;
    private VisualElement m_Content;
    private VisualElement m_InfoTitle;
    // private VisualElement m_Icon;
    private Label m_NameLabel;
    private Label m_VariableTypeLabel;
    private TextField m_NameField;
    private bool m_TitleEditing;

    internal bool IsEditable { get; set; } = true;

    public BlackboardVariableElement()
    {
        AddToClassList("BlackboardVariable");
        AddToClassList("Collapsed");
        styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Blackboard/Assets/BlackboardVariableStylesheet.uss"));
        ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Blackboard/Assets/BlackboardVariableLayout.uxml").CloneTree(this);

        m_NameLabel = this.Q<Label>("Name");
        m_NameField = this.Q<TextField>("NameField");
        m_VariableTypeLabel = this.Q<Label>("VariableType");
        m_Content = this.Q("Content");
        m_InfoTitle = this.Q("Info");
        // m_Icon = this.Q("Icon");
        m_NameField.size = Size.S;
        IsEditable = true;

        RegisterCallbacks();
    }

    public BlackboardVariableElement(string name) : this()
    {
        Name = name;
    }

    public BlackboardVariableElement(BlackboardView view, VariableModel variableModel) : this(variableModel.Name)
    {
        m_View = view;
        m_VariableModel = variableModel;
    }

    private void RegisterCallbacks()
    {
        RegisterCallback<PointerUpEvent>(OnPointerDown);
        m_NameLabel.RegisterCallback<PointerDownEvent>(OnNameLabelPointerDown);
        m_InfoTitle.RegisterCallback<PointerUpEvent>(OnTitlePointerUp);
        m_NameField.RegisterValueChangedCallback(OnNameFieldValueChanged);
        m_NameField.RegisterCallback<KeyDownEvent>(e =>
        {
            switch (e.keyCode)
            {
                case KeyCode.Return:
                    m_NameField.Blur();
                    ToggleTitleFields();
                    break;
                case KeyCode.Escape:
                    m_NameField.SetValueWithoutNotify(Name);
                    ToggleTitleFields();
                    break;
            }
        });
    }

    private void OnNameLabelPointerDown(PointerDownEvent evt)
    {
        if (m_TitleEditing)
        {
            return;
        }
        if (evt.clickCount == 2 && evt.button == 0)
        {
            ToggleTitleFields();
            evt.StopImmediatePropagation();
        }
    }

    private void OnNameFieldValueChanged(ChangeEvent<string> evt)
    {
        Name = m_NameField.value;
        OnNameChanged?.Invoke(m_NameField.value, m_VariableModel);
    }

    private void OnPointerDown(PointerUpEvent evt)
    {
        if (m_TitleEditing)
        {
            return;
        }
        if (evt.clickCount == 1 && evt.button == 1)
        {
            ContextMenu menu = new ContextMenu(this);
            if (!IsEditable)
            {
                return;
            }

            if (VariableModel.ID != k_ReservedID)
            {
                menu.AddItem("Delete", OnDelete);
            }
            else
            {
                menu.AddDisabledItem("Delete");
            }
            menu.AddItem("Rename", ToggleTitleFields);
            menu.AddItem("Copy GUID", OnCopyGUID);
            menu.AddSeparator();
            if (VariableModel.IsExposed)
            {
                menu.AddItemCheckmarked("Expose", OnToggleExposeVariable);
            }
            else
            {
                menu.AddItem("Expose", OnToggleExposeVariable);
            }
            if (VariableModel.ID == k_ReservedID)
            {
                menu.AddDisabledItem("Shared");
            }
            else
            {
                if (VariableModel.IsShared)
                {
                    menu.AddItemCheckmarked("Shared", OnToggleSharedVariable);
                }
                else
                {
                    menu.AddItem("Shared", OnToggleSharedVariable);
                }
            }
            
            menu.Show();
        }
    }

    private void OnCopyGUID()
    {
        (ulong, ulong) guidParts = m_VariableModel.ID.ToParts();
        string copyString = $"Variable: { m_VariableModel.Name }\n" +
            $"GUID Parts: { guidParts.Item1.ToString() }, { guidParts.Item2.ToString() }\n" +
            $"GUID String: {m_VariableModel.ID.ToString()}";

        GUIUtility.systemCopyBuffer = copyString;
    }

    private void OnToggleExposeVariable()
    {
        if (VariableModel.IsExposed)
        {
            m_View.Asset.MarkUndo($"Unexpose variable: {VariableModel.Name}.");
        }
        else
        {
            m_View.Asset.MarkUndo($"Expose variable: {VariableModel.Name}.");
        }
        VariableModel.IsExposed = !VariableModel.IsExposed;
    }

    private void OnToggleSharedVariable()
    {
        if (VariableModel.IsShared)
        {
            m_View.Asset.MarkUndo($"Make variable not global: {VariableModel.Name}.");
        }
        else
        {
            m_View.Asset.MarkUndo($"Make variable global: {VariableModel.Name}.");
        }
        VariableModel.IsShared = !VariableModel.IsShared;
    }

    private void OnTitlePointerUp(PointerUpEvent evt)
    {
        if (evt.clickCount == 1 && evt.button == 0 && !m_TitleEditing)
        {
            if (ClassListContains("Expanded"))
            {
                Collapse();
            }
            else
            {
                Expand();
            }
            evt.StopPropagation();
        }
    }

    private void Collapse()
    {
        RemoveFromClassList("Expanded");
        AddToClassList("Collapsed");
    }

    internal void Expand()
    {
        RemoveFromClassList("Collapsed");
        AddToClassList("Expanded");
    }

    private void OnDelete()
    {
        m_View.DeleteVariable(m_VariableModel);
    }

    private void OnNameFieldFocusOut(BlurEvent evt)
    {
        ToggleTitleFields();
    }

    public void ToggleTitleFields()
    {
        if (!IsEditable)
        {
            return;
        }
        
        m_TitleEditing = !m_TitleEditing;

        if (m_TitleEditing)
        {
            schedule.Execute(() =>
            {
                m_NameField.SetValueWithoutNotify(Name);
                AddToClassList("Editing");

                m_NameField.style.display = DisplayStyle.Flex;
                m_NameLabel.style.display = DisplayStyle.None;
                m_NameField.RegisterCallback<BlurEvent>(OnNameFieldFocusOut);
                m_NameField.Focus();
                m_NameField.Q<UnityEngine.UIElements.TextField>().SelectAll();
            });
        }
        else
        {
            m_NameField.UnregisterCallback<BlurEvent>(OnNameFieldFocusOut);
            m_NameLabel.style.display = DisplayStyle.Flex;
            m_NameField.style.display = DisplayStyle.None;
            RemoveFromClassList("Editing");
        }
    }

    public void MarkUndo(string description)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(m_View.Asset, description);
#endif
    }
}
#endif