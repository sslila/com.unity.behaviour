#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Behavior.GraphFramework;
using UnityEngine.UIElements;

namespace Unity.Behavior
{
    internal class BehaviorNodeUI : NodeUI
    {
        public BehaviorNodeUI(NodeModel nodeModel) : base(nodeModel) 
        {
            styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/BehaviorNodeStylesheet.uss"));
            VisualElement badges = new VisualElement() { name = "Badges" };
            ResourceLoadAPI.Load<VisualTreeAsset>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/NodeBadgesLayout.uxml").CloneTree(badges);
            badges.styleSheets.Add(ResourceLoadAPI.Load<StyleSheet>("Packages/com.unity.behavior/Authoring/UI/AssetEditor/Assets/NodeBadgesStylesheet.uss"));
            SelectionBorder.Add(badges);

#if UNITY_EDITOR
            if (nodeModel is BehaviorGraphNodeModel)
            {
                BehaviorAuthoringGraph asset = nodeModel.Asset as BehaviorAuthoringGraph;
                if (asset && asset.DebugInfo != null && asset.DebugInfo.IsNodeBreakpointEnabled(nodeModel.ID))
                {
                    AddToClassList("BreakpointEnabled");
                }
            }
#endif
        }

        internal virtual void InitFromNodeInfo(NodeInfo nodeInfo)
        {
            this.AddNodeIcon(nodeInfo);

            ReflectionElement reflectionElement = this.Q<ReflectionElement>();
            if (reflectionElement == null)
            {
                reflectionElement = new ReflectionElement();
                NodeValueContainer.Add(reflectionElement);
            }
            reflectionElement.CreateFields(nodeInfo);
            reflectionElement.Node = Model as BehaviorGraphNodeModel; // Sets the linkfield visuals 

            Title = nodeInfo.Name;
            if (reflectionElement.IsTwoLineElement)
            {
                EnableInClassList("TwoLineNode", true);
            }
            else
            {
                EnableInClassList("TwoLineNode", false);
                NodeValueContainer.Hide();
            }
        }

        public override void Refresh(bool isDragging)
        {
            base.Refresh(isDragging);
            
            // Keep the linked label prefix updated on Blackboard asset group variables.
            foreach (BaseLinkField field in GetLinkFields())
            {
                Util.UpdateLinkFieldBlackboardPrefixes(field);
            }
        }

        internal List<BaseLinkField> GetLinkFields()
        {
            return this.Query<BaseLinkField>().ToList();
        }

        internal virtual void UpdateLinkFields()
        {
            // Refresh the fields.
            this.Query<BaseLinkField>().ForEach(linkField =>
            {
                linkField.Model = Model;
            });
        }

        internal virtual void UpdateStatus(Node.Status status)
        {
            EnableInClassList("NodeStatus_Uninitialized", status is Node.Status.Uninitialized);
            EnableInClassList("NodeStatus_Running", status is Node.Status.Running);
            EnableInClassList("NodeStatus_Success", status is Node.Status.Success);
            EnableInClassList("NodeStatus_Failure", status is Node.Status.Failure);
            EnableInClassList("NodeStatus_Waiting", status is Node.Status.Waiting);

            DebugIconElement.tooltip = $"Current State: {status}.";

            var firstInputPort = GetFirstInputPort();
            if (firstInputPort != null) {
                bool isRunning = !(status is Node.Status.Uninitialized);
                foreach (Edge edge in firstInputPort.Edges)
                {
                    edge.IsDebugHighlighted = isRunning;
                    edge.MarkDirtyAndRepaint();
                    edge.EnableInClassList("NodeStatus_Running", isRunning);
                    edge.EnableInClassList("NodeStatus_Uninitialized", !isRunning);
                }
            }
        }
    }
}
#endif