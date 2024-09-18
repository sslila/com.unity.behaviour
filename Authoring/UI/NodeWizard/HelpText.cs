using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

namespace Unity.Behavior
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class HelpText : ExVisualElement
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<HelpText, UxmlTraits> {}
#endif
        public string Text
        {
            get => this.Q<Label>().text;
            set => this.Q<Label>().text = value;
        }

        public HelpText()
            : this(null)
        {
        }

        public HelpText(string text)
        {
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/HelpText.uxml").CloneTree(this);
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/NodeWizard/Assets/HelpTextStylesheet.uss"));
            Text = text;
        }
    }
}