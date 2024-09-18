#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    [NodeUI(typeof(StickyNoteModel))]
    internal class StickyNoteUI : NodeUI
    {
        EditableLabel m_EditableLabel;
        new StickyNoteModel Model { get => base.Model as StickyNoteModel; }
        public StickyNoteUI(NodeModel nodeModel) : base(nodeModel)
        {
            AddToClassList("StickyNote");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Tools/Graph/Assets/StickyNoteStylesheet.uss"));
            Add(ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Tools/Graph/Assets/StickyNoteLayout.uxml").CloneTree());

            m_EditableLabel = this.Q<EditableLabel>();
            m_EditableLabel.RegisterValueChangedCallback(OnValueChanged);

            m_EditableLabel.Text = Model.Text;
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            Model.Asset.MarkUndo("Edit sticky note");
            Model.Text = evt.newValue;
        }
    }
}
#endif