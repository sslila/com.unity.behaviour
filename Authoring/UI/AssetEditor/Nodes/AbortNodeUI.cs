#if UNITY_EDITOR
using System.Linq;
using Unity.Behavior.GraphFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    [NodeUI(typeof(AbortNodeModel))]
    internal class AbortNodeUI : ConditionalNodeUI
    {
        private VisualElement m_ConditionFieldContainer;
        private AbortNodeModel m_AbortNodeModel => Model as AbortNodeModel;

        public AbortNodeUI(NodeModel nodeModel) : base(nodeModel)
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/ConditionNodeStylesheet.uss"));
            AddToClassList("Modifier");
            AddToClassList("TwoLineNode");
        }

        private void UpdateNodeTitle()
        {
            if (m_AbortNodeModel.ConditionModels.Count > 1)
            {
                Title = !m_AbortNodeModel.RequiresAllConditionsTrue ? $"{m_AbortNodeModel.ModelAbortType.ToString()} If Any Are True" : $"{m_AbortNodeModel.ModelAbortType.ToString()} If All Are True";   
            }
            else
            {
                Title = $"{m_AbortNodeModel.ModelAbortType.ToString()} If";
            }
        }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);
            UpdateNodeTitle();
        }
    }
}
#endif