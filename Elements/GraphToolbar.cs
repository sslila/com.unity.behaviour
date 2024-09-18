using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
#if ENABLE_UXML_UI_SERIALIZATION
    [UxmlElement]
#endif
    internal partial class GraphToolbar : VisualElement
    {
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<GraphToolbar, UxmlTraits> {}
#endif
        public override VisualElement contentContainer => this;
        public ActionButton OpenAssetButton => this.Q<ActionButton>("OpenAssetButton");
        public Text AssetTitle => this.Q<Text>("AssetTitleElement");

        public GraphToolbar()
        {
            AddToClassList("GraphToolbar");
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Elements/Assets/GraphToolbarStylesheet.uss"));
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Elements/Assets/GraphToolbarLayout.uxml").CloneTree(this);
        }
    }
}