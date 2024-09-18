#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using Canvas = Unity.AppUI.UI.Canvas;

namespace Unity.Behavior.GraphFramework
{
    internal static class GraphViewSpatialExtensions 
    {
        public static Vector2 WorldPosToLocal(this GraphView graphView, Vector2 position)
        {
            Vector2 local = graphView.Viewport.WorldToLocal(position);
            return local;
        }
        
        public static NodeUI NodeAt(this GraphView graphView, Vector2 pos)
        {
            foreach (NodeUI nodeUI in graphView.ViewState.Nodes)
            {
                if (nodeUI.ContainsPoint(nodeUI.WorldToLocal(pos)))
                {
                    return nodeUI;
                }
            }
            return null;
        }
        
        public static Edge EdgeAt(this GraphView graphView, Vector2 pos)
        {
            foreach (Edge edge in graphView.ViewState.Edges)
            {
                if (edge.ContainsPoint(edge.WorldToLocal(pos)))
                {
                    return edge;
                }
            }
            return null;
        }

        public static void PanToNode(this GraphView graphView, NodeUI nodeUI, Vector2 offset, long duration = 0)
        {
            void OnFocusNodeGeometryChanged(GeometryChangedEvent evt)
            {
                if (evt.target is NodeUI node)
                {
                    node.UnregisterCallback<GeometryChangedEvent>(OnFocusNodeGeometryChanged);
                    graphView.PanToNode(node, offset);
                }
            }

            if (float.IsNaN(offset.x) || float.IsNaN(offset.y))
            {
                offset = Vector2.zero;
            }
            
            Vector2 nodeSize = nodeUI.worldBound.size;
            if (float.IsNaN(nodeSize.x))
            {
                nodeUI.RegisterCallback<GeometryChangedEvent>(OnFocusNodeGeometryChanged);
                return;
            }
            
            Canvas canvas = graphView.Background;
            VisualElement viewport = canvas.Q<VisualElement>("appui-canvas__viewport");
            Vector2 scale = canvas.transform.scale;
            Vector2 viewportCentering = (canvas.layout.width * Vector2.right - nodeSize) * 0.5f;
            Vector2 endPosition = (offset - nodeUI.Position + viewportCentering) * scale;

            if (duration > 0f)
            {
                float accumulated = 0f;
                var startPosition = viewport.transform.position;
                graphView.schedule.Execute((t) =>
                {
                    accumulated += t.deltaTime;
                    float ratio = accumulated / duration;
                    viewport.transform.position = Vector2.Lerp(startPosition, endPosition, ratio);
                }).Every(10).ForDuration(duration);
                graphView.schedule.Execute(() =>
                {
                    viewport.transform.position = endPosition;
                }).ExecuteLater(duration);
            }
            else
            {
                viewport.transform.position = endPosition;
            }
        }
    }
}
#endif