#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Behavior.GraphFramework;

#if ENABLE_UXML_UI_SERIALIZATION
[UxmlElement]
#endif
internal partial class CreateNodeButton : VisualElement
{
#if !ENABLE_UXML_UI_SERIALIZATION
        internal new class UxmlFactory : UxmlFactory<CreateNodeButton, UxmlTraits> {}
#endif
    public CreateNodeButton()
    {
        AddToClassList("CreateNodeButton");
        styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/CreateNodeButtonStylesheet.uss"));
        ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/CreateNodeButtonLayout.uxml").CloneTree(this);
    }
}

#endif