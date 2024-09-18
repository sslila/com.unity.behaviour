#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Behavior.GraphFramework
{
    internal class Edge : GraphElement
    {
        static readonly CustomStyleProperty<float> s_EdgeThicknessPropery = new CustomStyleProperty<float>("--edge-thickness");
        public bool IsDebugHighlighted
        {
            get => ClassListContains("DebugHighlight");
            set => EnableInClassList("DebugHighlight", value);
        }
        
        public Port Start { 
            get => m_Start;
            set 
            {
                m_Start?.GetFirstAncestorOfType<GraphElement>().UnregisterCallback<GeometryChangedEvent>(OnLinkMove);
                m_Start = value;
                m_Start?.GetFirstAncestorOfType<GraphElement>().RegisterCallback<GeometryChangedEvent>(OnLinkMove);
                MarkDirtyAndRepaint();
            } 
        }

        public Port End
        {
            get => m_End;
            set
            {
                m_End?.GetFirstAncestorOfType<GraphElement>().UnregisterCallback<GeometryChangedEvent>(OnLinkMove);
                m_End = value;
                m_End?.GetFirstAncestorOfType<GraphElement>().RegisterCallback<GeometryChangedEvent>(OnLinkMove);
                MarkDirtyAndRepaint();
            }
        }
        
        public Vector2 StartPosition 
        { 
            get => m_StartPosition;
            set
            {
                m_StartPosition = value;
                MarkDirtyAndRepaint();
            } 
        }

        public Vector2 EndPosition
        {
            get => m_EndPosition;
            set
            {
                m_EndPosition = value;
                MarkDirtyAndRepaint();
            }
        }
        
        private Vector2 m_StartWorldPosition => m_Start != null
            ? m_Start.LocalToWorld((Vector2)m_Start.transform.position
                                   + new Vector2(m_Start.resolvedStyle.width, 10.0f)/2)
            : m_StartPosition;
        
        private Vector2 m_EndWorldPosition => m_End != null
            ? m_End.LocalToWorld((Vector2)m_End.transform.position
                                 + new Vector2(m_End.resolvedStyle.width, m_End.resolvedStyle.height)/2)
            : m_EndPosition;
        
        internal enum EdgeVisualisationType
        {
            Bezier,
            Sharp,
        }
        
        private Port m_Start;
        private Port m_End;
        private Vector2 m_StartPosition;
        private Vector2 m_EndPosition;
        private GraphView m_GraphView;

        private const int kCircleSegments = 32;
        private const float kVerticalMargins = 30.0f;
        private const int kMaxCircleRadius = 12;
        private const float kPadding = 2.0f;
        private float m_Thickness = 2.0f;
        private float k_MinWidth => m_Thickness + 2 * kPadding;

        // Cached vertex data
        private const int k_NumEdgeSegments = 100;
        private readonly Vertex[] m_Vertices = new Vertex[4*k_NumEdgeSegments];
        private readonly ushort[] m_Indices = new ushort[6*k_NumEdgeSegments];

        internal EdgeVisualisationType EdgeVisualisation { get; set; } = EdgeVisualisationType.Sharp;

        public Edge()
        {
            AddToClassList("Edge");
            InitEdgeDrawData();
            generateVisualContent = OnGenerateVisualContent;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);

            usageHints |= UsageHints.DynamicTransform;

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            ICustomStyle customStyle = evt.customStyle;
            if (customStyle.TryGetValue(s_EdgeThicknessPropery, out float thickness))
            {
                m_Thickness = thickness;
                MarkDirtyAndRepaint();
            }
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            switch (EdgeVisualisation)
            {
                case EdgeVisualisationType.Bezier:
                    return ContainsPointBezier(localPoint);
                case EdgeVisualisationType.Sharp:
                    return ContainsPointSharp(localPoint);
                default:
                    throw new Exception("Unknown EdgeVisualisationType");
            }            
        }

        private bool ContainsPointSharp(Vector2 localPoint)
        {
            Vector2 startPositionLocal = this.WorldToLocal(m_StartWorldPosition);
            Vector2 endPositionLocal = this.WorldToLocal(m_EndWorldPosition);

            if (IsTheEdgeStraight(startPositionLocal, endPositionLocal, m_Thickness))
            {
                return IsPointInLine(localPoint, startPositionLocal, endPositionLocal, k_MinWidth);
            }

            if (startPositionLocal.y > endPositionLocal.y)
            {
                return ContainsPointSharpBottomToTop(localPoint, startPositionLocal, endPositionLocal);
            }
            Vector2 halfwayPoint = startPositionLocal + (endPositionLocal - startPositionLocal) * 0.5f;
            Vector2 halfwayPointStart = new Vector2(startPositionLocal.x, halfwayPoint.y);
            Vector2 halfwayPointEnd = new Vector2(endPositionLocal.x, halfwayPoint.y);
            return IsPointInLine(localPoint, startPositionLocal, halfwayPointStart, k_MinWidth) ||
                   IsPointInLine(localPoint, halfwayPointStart, halfwayPointEnd, k_MinWidth) ||
                   IsPointInLine(localPoint, halfwayPointEnd, endPositionLocal, k_MinWidth);
        }

        private bool ContainsPointSharpBottomToTop(Vector2 localPoint, Vector2 startPositionLocal, Vector2 endPositionLocal)
        {
            float halfwayX = Mathf.Lerp(startPositionLocal.x, endPositionLocal.x, 0.5f);

            // Draw vertical lines and semi circle from the start position.
            return IsPointInLine(localPoint, startPositionLocal, startPositionLocal + new Vector2(0.0f, kVerticalMargins), k_MinWidth) ||
                IsPointInLine(localPoint, endPositionLocal, endPositionLocal - new Vector2(0.0f, kVerticalMargins), k_MinWidth) ||
                IsPointInLine(localPoint, startPositionLocal + new Vector2(0.0f, kVerticalMargins), new Vector2(halfwayX, startPositionLocal.y + kVerticalMargins), k_MinWidth) ||
                IsPointInLine(localPoint, new Vector2(halfwayX, endPositionLocal.y - kVerticalMargins), new Vector2(halfwayX, startPositionLocal.y + kVerticalMargins), k_MinWidth) ||
                IsPointInLine(localPoint, endPositionLocal - new Vector2(0.0f, kVerticalMargins), new Vector2(halfwayX, endPositionLocal.y - kVerticalMargins), k_MinWidth);
        }

        private bool IsPointInLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float lineThickness)
        {
            lineThickness *= 0.5f;
            float minX = Mathf.Min(lineStart.x, lineEnd.x) - lineThickness;
            float maxX = Mathf.Max(lineStart.x, lineEnd.x) + lineThickness;
            float minY = Mathf.Min(lineStart.y, lineEnd.y) - lineThickness;
            float maxY = Mathf.Max(lineStart.y, lineEnd.y) + lineThickness;
            return point.x >= minX &&       
                   point.x <= maxX &&
                   point.y >= minY &&
                   point.y <= maxY;
        }

        private bool ContainsPointBezier(Vector2 localPoint)
        {
            Vector3 startPosition = this.WorldToLocal(m_Start != null ? m_Start.worldBound.center : m_StartPosition);
            Vector3 targetPosition = this.WorldToLocal(m_End != null ? m_End.worldBound.center : m_EndPosition);
            GetControlPoints(startPosition, targetPosition, out Vector3 control1, out Vector3 control2);

            return SubdivideAndTestBezierPoint(localPoint, startPosition, targetPosition, control1, control2, 0.0f, 1.0f);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_Start?.GetFirstAncestorOfType<GraphElement>().UnregisterCallback<GeometryChangedEvent>(OnLinkMove);
            m_End?.GetFirstAncestorOfType<GraphElement>().UnregisterCallback<GeometryChangedEvent>(OnLinkMove);
            m_Start?.GetFirstAncestorOfType<GraphElement>().RegisterCallback<GeometryChangedEvent>(OnLinkMove);
            m_End?.GetFirstAncestorOfType<GraphElement>().RegisterCallback<GeometryChangedEvent>(OnLinkMove);
            m_GraphView = GetFirstAncestorOfType<GraphView>();
            
            SetStyle();
            MarkDirtyAndRepaint();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            m_Start?.GetFirstAncestorOfType<GraphElement>()?.UnregisterCallback<GeometryChangedEvent>(OnLinkMove);
            m_End?.GetFirstAncestorOfType<GraphElement>()?.UnregisterCallback<GeometryChangedEvent>(OnLinkMove);
        }

        private void OnGenerateVisualContent(MeshGenerationContext obj)
        {
            Color32 currentColor = resolvedStyle.color;
            Vector2 startPositionWorld = m_StartWorldPosition;
            Vector2 endPositionWorld = m_EndWorldPosition;
            Vector2 startPositionLocal = this.WorldToLocal(startPositionWorld);
            Vector2 endPositionLocal = this.WorldToLocal(endPositionWorld);


            switch (EdgeVisualisation)
            {
                case EdgeVisualisationType.Bezier:
                    DrawBezier(startPositionLocal, endPositionLocal, obj, currentColor, m_Thickness);
                    break;

                case EdgeVisualisationType.Sharp:
                    DrawSharpEdge(startPositionLocal, endPositionLocal, obj, currentColor, m_Thickness);
                    break;
            }
        }

        private static bool IsTheEdgeStraight(Vector2 startPosition, Vector2 endPosition, float thickness)
        {
            thickness *= 0.5f;
            return startPosition.x >= endPosition.x - thickness && startPosition.x <= endPosition.x + thickness;
        }

        private void DrawSharpEdge(Vector2 startPosition, Vector2 endPosition, MeshGenerationContext obj, Color32 color, float thickness)
        {
            // Draw the arrow at the end.
            Vector2 arrowEndPosition = endPosition - Vector2.up;
            DrawLine(arrowEndPosition + new Vector2(-8, -8), arrowEndPosition, obj, color, thickness);
            DrawLine(arrowEndPosition + new Vector2(+8, -8), arrowEndPosition, obj, color, thickness);

            // Draw the triangle at the start
            DrawTriangle(startPosition, obj, color, 12 /* Triangle Size */);

            if (startPosition.y > endPosition.y - (kVerticalMargins + kMaxCircleRadius))
            {
                DrawSharpEdgeBottomToTop(startPosition, endPosition, obj, color, thickness);
                return;
            }

            if (IsTheEdgeStraight(startPosition, endPosition, thickness))
            {
                DrawLine(startPosition, endPosition, obj, color, thickness);
                return;
            }

            Vector2 halfwayPoint = startPosition + (endPosition - startPosition) * 0.5f;
            Vector2 halfwayPointStart = new Vector2(startPosition.x, halfwayPoint.y);
            Vector2 halfwayPointEnd = new Vector2(endPosition.x, halfwayPoint.y);
            float circleRadius = Mathf.Min(kMaxCircleRadius, Mathf.Min(Mathf.Abs(endPosition.y - startPosition.y) * 0.5f, Mathf.Abs(endPosition.x - startPosition.x) * 0.5f));

            float halfWaySign = Mathf.Sign(halfwayPointEnd.x - halfwayPointStart.x);
            float circleOffset1 = halfWaySign > 0 ? Mathf.PI * 0.5f * 3 : 0.0f;
            float circleOffset2 = halfWaySign > 0 ? Mathf.PI * 0.5f : Mathf.PI * 0.5f * 2;

            DrawLine(startPosition, halfwayPointStart - new Vector2(0.0f, circleRadius), obj, color, thickness);
            DrawQuarterCircle(halfwayPointStart + new Vector2(circleRadius * halfWaySign, -circleRadius), obj, color, circleRadius, thickness, kCircleSegments, circleOffset1);
            DrawLine(halfwayPointStart + new Vector2(circleRadius * halfWaySign, 0.0f), halfwayPointEnd - new Vector2(circleRadius * halfWaySign, 0.0f), obj, color, thickness);
            DrawQuarterCircle(halfwayPointEnd + new Vector2(circleRadius * -halfWaySign, circleRadius), obj, color, circleRadius, thickness, kCircleSegments, circleOffset2);
            DrawLine(halfwayPointEnd + new Vector2(0.0f, circleRadius), endPosition, obj, color, thickness);
        }

        private void DrawSharpEdgeBottomToTop(Vector2 startPosition, Vector2 endPosition, MeshGenerationContext obj, Color32 color, float thickness)
        {
            float circleRadius = Mathf.Min(kMaxCircleRadius, Mathf.Abs(endPosition.x - startPosition.x) * 0.25f);
            float halfWaySign = Mathf.Sign(endPosition.x - startPosition.x);
            float circleOffset1 = halfWaySign > 0 ? Mathf.PI * 0.5f * 3 : 0.0f;
            float circleOffset2 = halfWaySign > 0 ? Mathf.PI * 0.5f : Mathf.PI * 0.5f * 2;

            float halfwayX = Mathf.Lerp(startPosition.x, endPosition.x, 0.5f);
            Vector2 bottomSemiCircleCenter = new Vector2(halfwayX - circleRadius * halfWaySign, startPosition.y + kVerticalMargins - circleRadius);
            Vector2 topSemiCircleCenter = new Vector2(halfwayX + circleRadius * halfWaySign, endPosition.y - kVerticalMargins + circleRadius);

            // Draw vertical lines and semi circle from the start position.
            DrawLine(startPosition, startPosition + new Vector2(0.0f, kVerticalMargins - circleRadius), obj, color, thickness);
            DrawQuarterCircle(startPosition + new Vector2(0.0f, kVerticalMargins) + new Vector2(circleRadius * halfWaySign, -circleRadius), obj, color, circleRadius, thickness, kCircleSegments, circleOffset1);

            // Draw vertical line and semi circle from the end position.
            DrawLine(endPosition, endPosition - new Vector2(0.0f, kVerticalMargins - circleRadius), obj, color, thickness);
            DrawQuarterCircle(endPosition - new Vector2(0.0f, kVerticalMargins) + new Vector2(circleRadius * -halfWaySign, circleRadius), obj, color, circleRadius, thickness, kCircleSegments, circleOffset2);


            // Draw a vertical line at the halfway X position.
            DrawLine(new Vector2(halfwayX, endPosition.y - kVerticalMargins + circleRadius), new Vector2(halfwayX, startPosition.y + kVerticalMargins - circleRadius), obj, color, thickness);

            // Draw a horizontal line from the start vertical line to the halfway X position.
            DrawLine(startPosition + new Vector2(circleRadius * halfWaySign, kVerticalMargins), new Vector2(halfwayX - circleRadius * halfWaySign, startPosition.y + kVerticalMargins), obj, color, thickness);

            // Draw a horizontal line from the end vertical line to the halfway X position.
            DrawLine(endPosition - new Vector2(circleRadius * halfWaySign, kVerticalMargins), new Vector2(halfwayX + circleRadius * halfWaySign, endPosition.y - kVerticalMargins), obj, color, thickness);

            // Draw the final 2 connecting semi circles.
            DrawQuarterCircle(bottomSemiCircleCenter, obj, color, circleRadius, thickness, kCircleSegments, halfWaySign > 0 ? Mathf.PI * 0.5f * 4 : Mathf.PI * 0.5f * 3);
            DrawQuarterCircle(topSemiCircleCenter, obj, color, circleRadius, thickness, kCircleSegments, halfWaySign > 0 ? Mathf.PI * 0.5f * 2: Mathf.PI * 0.5f);

        }

        private void DrawTriangle(Vector2 startPosition, MeshGenerationContext obj, Color32 color, float size)
        {
            MeshWriteData mesh = obj.Allocate(3, 6);
            mesh.SetNextVertex(new Vertex() { position = startPosition - new Vector2(size * 0.5f, 0), tint = color });
            mesh.SetNextVertex(new Vertex() { position = startPosition + new Vector2(size * 0.5f, 0), tint = color });
            mesh.SetNextVertex(new Vertex() { position = startPosition + new Vector2(0, size), tint = color });

            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);

            mesh.SetNextIndex(2);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(0);
        }

        private void DrawQuarterCircle(Vector2 circleCenter, MeshGenerationContext obj, Color32 color, float radius, float thickness, int numSegments, float circleOffset = 0.0f)
        {
            numSegments = Mathf.Max(numSegments, 2);
            float halfThickness = thickness * 0.5f;
            float innerRadius = radius - halfThickness;
            float outerRadius = radius + halfThickness;
            
            MeshWriteData mesh = obj.Allocate(numSegments * 2, (numSegments-1) * 12);
            for (int i = 0; i < numSegments; ++i)
            {
                float angle = circleOffset + i * Mathf.PI * 0.5f / (numSegments - 1);
                float SinAngle = Mathf.Sin(angle);
                float cosAngle = Mathf.Cos(angle);

                Vector3 innerPoint = new Vector3(circleCenter.x + SinAngle * innerRadius, circleCenter.y + cosAngle * innerRadius, Vertex.nearZ);
                Vector3 outerPoint = new Vector3(circleCenter.x + SinAngle * outerRadius, circleCenter.y + cosAngle * outerRadius, Vertex.nearZ);
                mesh.SetNextVertex(new Vertex() { position = innerPoint, tint = color });
                mesh.SetNextVertex(new Vertex() { position = outerPoint, tint = color });

                if (i == 0)
                {
                    continue;
                }

                int currentIndex = i * 2 - 2;
                mesh.SetNextIndex((ushort)(currentIndex));
                mesh.SetNextIndex((ushort)(currentIndex + 1));
                mesh.SetNextIndex((ushort)(currentIndex + 2));

                mesh.SetNextIndex((ushort)(currentIndex + 2));
                mesh.SetNextIndex((ushort)(currentIndex + 1));
                mesh.SetNextIndex((ushort)(currentIndex));

                mesh.SetNextIndex((ushort)(currentIndex + 2));
                mesh.SetNextIndex((ushort)(currentIndex + 3));
                mesh.SetNextIndex((ushort)(currentIndex + 1));

                mesh.SetNextIndex((ushort)(currentIndex + 1));
                mesh.SetNextIndex((ushort)(currentIndex + 3));
                mesh.SetNextIndex((ushort)(currentIndex + 2));

            }
        }

        private void DrawLine(Vector2 startPosition, Vector2 endPosition, MeshGenerationContext obj, Color32 color, float thickness)
        {
            float halfThickness = thickness * 0.5f;
            MeshWriteData mesh = obj.Allocate(4, 12);
            Vector3 perpendicularDirection = Vector2.Perpendicular((endPosition- startPosition).normalized);
            Vector3 perpendicularOffset = perpendicularDirection * halfThickness;
            mesh.SetNextVertex(new Vertex() { position = new Vector3(startPosition.x, startPosition.y, Vertex.nearZ) - perpendicularOffset, tint = color});
            mesh.SetNextVertex(new Vertex() { position = new Vector3(startPosition.x, startPosition.y, Vertex.nearZ) + perpendicularOffset, tint = color });
            mesh.SetNextVertex(new Vertex() { position = new Vector3(endPosition.x, endPosition.y, Vertex.nearZ) - perpendicularOffset, tint = color });
            mesh.SetNextVertex(new Vertex() { position = new Vector3(endPosition.x, endPosition.y, Vertex.nearZ) + perpendicularOffset, tint = color });

            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);

            mesh.SetNextIndex(2);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(0);


            mesh.SetNextIndex(2);
            mesh.SetNextIndex(3);
            mesh.SetNextIndex(1);

            mesh.SetNextIndex(1);
            mesh.SetNextIndex(3);
            mesh.SetNextIndex(2);

        }

        private void OnLinkMove(GeometryChangedEvent evt)
        {
            SetStyle();

            MarkDirtyAndRepaint();
        }

        private void SetStyle()
        {
            Vector2 startPositionWorld = m_StartWorldPosition;
            Vector2 endPositionWorld = m_EndWorldPosition;
            Vector2 center = (startPositionWorld + endPositionWorld) / 2;
            Vector3 scale = m_GraphView != null ? m_GraphView.Viewport.transform.scale : Vector3.one;
            float width = Math.Max(Math.Abs(endPositionWorld.x - startPositionWorld.x)/scale.x, k_MinWidth) + k_MinWidth;
            float height = Math.Max(Math.Abs(endPositionWorld.y - startPositionWorld.y)/scale.y, k_MinWidth) + k_MinWidth;
            this.transform.position = m_GraphView != null ? m_GraphView.Viewport.WorldToLocal(center - new Vector2(width, height) / 2) : center;
            this.style.width = width;
            this.style.height = height;
        }

        private void InitEdgeDrawData()
        {
            for (int i = 0; i < k_NumEdgeSegments; ++i)
            {
                int indexOffset = i*4;
                m_Vertices[indexOffset + 0] = new Vertex();
                m_Vertices[indexOffset + 1] = new Vertex();
                m_Vertices[indexOffset + 2] = new Vertex();
                m_Vertices[indexOffset + 3] = new Vertex();

                m_Indices[i * 6 + 0] = (ushort)(indexOffset + 2);
                m_Indices[i * 6 + 1] = (ushort)(indexOffset + 1);
                m_Indices[i * 6 + 2] = (ushort)(indexOffset + 0);
                m_Indices[i * 6 + 3] = (ushort)(indexOffset + 2);
                m_Indices[i * 6 + 4] = (ushort)(indexOffset + 3);
                m_Indices[i * 6 + 5] = (ushort)(indexOffset + 1);
            }
        }

        private void DrawBezier(Vector3 startPosition, Vector3 targetPosition, MeshGenerationContext obj, Color32 color, float thickness)
        {
            float halfThickness = thickness * 0.5f;
            GetControlPoints(startPosition, targetPosition, out Vector3 control1, out Vector3 control2);

            MeshWriteData mesh = obj.Allocate(k_NumEdgeSegments * 4, k_NumEdgeSegments * 6);
            float prevPositionX = startPosition.x;
            float prevPositionY = startPosition.y;
            for (int i = 0; i < k_NumEdgeSegments; ++i)
            {
                CalculateCubicBezierPoint((i+1) / (float)(k_NumEdgeSegments), startPosition, control1, control2, targetPosition, out var nextPositionX, out var nextPositionY);
                
                float pX = nextPositionX - prevPositionX;
                float pY = nextPositionY - prevPositionY;
                float pHalfLength = Mathf.Sqrt(pX * pX + pY * pY);
                float pHalfX = -pY/pHalfLength * halfThickness;
                float pHalfY = pX/pHalfLength * halfThickness;

                int indexOffset = i * 4;

                m_Vertices[indexOffset + 0].position.x = prevPositionX - pHalfX;
                m_Vertices[indexOffset + 0].position.y = prevPositionY - pHalfY;
                m_Vertices[indexOffset + 1].position.x = prevPositionX + pHalfX;
                m_Vertices[indexOffset + 1].position.y = prevPositionY + pHalfY;
                m_Vertices[indexOffset + 2].position.x = nextPositionX - pHalfX;
                m_Vertices[indexOffset + 2].position.y = nextPositionY - pHalfY;
                m_Vertices[indexOffset + 3].position.x = nextPositionX + pHalfX;
                m_Vertices[indexOffset + 3].position.y = nextPositionY + pHalfY;

                m_Vertices[indexOffset + 0].tint = color;
                m_Vertices[indexOffset + 1].tint = color;
                m_Vertices[indexOffset + 2].tint = color;
                m_Vertices[indexOffset + 3].tint = color;

                prevPositionX = nextPositionX;
                prevPositionY = nextPositionY;
            }

            mesh.SetAllVertices(m_Vertices);
            mesh.SetAllIndices(m_Indices);
        }
        
        private static void GetControlPoints(Vector3 start, Vector3 end, out Vector3 control1, out Vector3 control2)
        {
            float heightDifference = end.y - start.y;
            Vector3 controlPointOffset = Vector3.up * Mathf.Clamp(heightDifference, 10f, 100f);
            control1 = start + controlPointOffset;
            control2 = end - controlPointOffset;
        }

        private void CalculateCubicBezierPoint(in float t, in Vector3 startPoint, in Vector3 controlPoint0, in Vector3 controlPoint1, in Vector3 endPoint, out float nextX, out float nextY)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            nextX = uuu * startPoint.x + 3 * uu * t * controlPoint0.x + 3 * u * tt * controlPoint1.x + ttt * endPoint.x;
            nextY = uuu * startPoint.y + 3 * uu * t * controlPoint0.y + 3 * u * tt * controlPoint1.y + ttt * endPoint.y;
        }

        private bool SubdivideAndTestBezierPoint(Vector2 localPoint, Vector2 startPosition, Vector2 targetPosition, Vector2 control1, Vector2 control2, float min, float max)
        {
            float thresholdSquared = (m_Thickness + kPadding) * (m_Thickness + kPadding);

            while (true)
            {
                float halfLength = (max - min) * 0.5f;
                float localStart = min;
                float midPoint = min + halfLength;
                bool test1 = TestBezierPoint(localPoint, startPosition, targetPosition, control1, control2, localStart, midPoint, out float radiusSquared1);

                float localEnd = max;
                bool test2 = TestBezierPoint(localPoint, startPosition, targetPosition, control1, control2, midPoint, localEnd, out float radiusSquared2);

                if (test1 && test2)
                {
                    if (Mathf.Approximately(min, max))
                    {
                        return true;
                    }
                    return SubdivideAndTestBezierPoint(localPoint, startPosition, targetPosition, control1, control2, localStart, midPoint) ||
                        SubdivideAndTestBezierPoint(localPoint, startPosition, targetPosition, control1, control2, midPoint, localEnd);
                }

                if (test1)
                {
                    if (radiusSquared1 <= (thresholdSquared))
                    {
                        return true;
                    }
                    max = midPoint;
                }
                else if (test2)
                {
                    if (radiusSquared2 <= (thresholdSquared))
                    {
                        return true;
                    }
                    min = midPoint;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool TestBezierPoint(Vector2 localPoint, Vector2 startPosition, Vector2 targetPosition, Vector2 control1, Vector2 control2, float min, float max, out float outRadiusSquared)
        {
            CalculateCubicBezierPoint(min, startPosition, control1, control2, targetPosition, out float p0x, out float p0y);
            CalculateCubicBezierPoint(max, startPosition, control1, control2, targetPosition, out float p1x, out float p1y);

            Vector2 size = new Vector2(p1x-p0x, p1y-p0y);
            Vector2 center = new Vector2(p0x, p0y) + size * 0.5f;
            float radius = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y));
            float radiusSquared = radius * radius;
            outRadiusSquared = radiusSquared;
            if (Vector2.SqrMagnitude(localPoint - center) <= (radiusSquared + (m_Thickness + kPadding) * (m_Thickness + kPadding)))
            {
                return true;
            }
            return false;
        }

        public bool IsEdgeInRect(Rect rect)
        {
            Vector2 startPosition = m_StartWorldPosition;
            Vector2 endPosition = m_EndWorldPosition;
            float thickness = m_Thickness;

            if (startPosition.y > endPosition.y)
            {
                Vector2 temp = startPosition;
                startPosition = endPosition;
                endPosition = temp;
            }

            if (EdgeVisualisation == EdgeVisualisationType.Bezier)
            {
                // Todo: Make this work correctly for bezier edges.
                return rect.Overlaps(Rect.MinMaxRect(Mathf.Min(startPosition.x, endPosition.x), Mathf.Min(startPosition.y, endPosition.y), Mathf.Max(startPosition.x, endPosition.x), Mathf.Max(startPosition.y, endPosition.y)));
            }

            if (IsTheEdgeStraight(startPosition, endPosition, thickness))
            {
                return rect.Overlaps(Rect.MinMaxRect(Mathf.Min(startPosition.x, endPosition.x), Mathf.Min(startPosition.y, endPosition.y), Mathf.Max(startPosition.x, endPosition.x), Mathf.Max(startPosition.y, endPosition.y)));
            }

            if (m_StartWorldPosition.y > m_EndWorldPosition.y - (kVerticalMargins + kMaxCircleRadius))
            {
                return IsEdgeInRectSharpBottomToTop(rect, m_StartWorldPosition, m_EndWorldPosition);
            }
            
            // Test against three line segments:
            // 1. From startPosition straight down to halfwayPointY
            // 2. Holding y constant, then over to endPosition.x
            // 3. Holding x constant, then down all the way to endPosition.y.
            float halfwayPointY = (endPosition.y + startPosition.y) * 0.5f;
            return
                rect.Overlaps(Rect.MinMaxRect(startPosition.x, startPosition.y, startPosition.x, halfwayPointY), true) ||
                rect.Overlaps(Rect.MinMaxRect(startPosition.x,  halfwayPointY,  endPosition.x, halfwayPointY), true) ||
                rect.Overlaps(Rect.MinMaxRect(endPosition.x, halfwayPointY, endPosition.x, endPosition.y), true);
        }

        private bool IsEdgeInRectSharpBottomToTop(Rect rect, Vector2 startPosition, Vector2 endPosition)
        {
            float halfwayX = Mathf.Lerp(m_StartWorldPosition.x, m_EndWorldPosition.x, 0.5f);

            return rect.Overlaps(Rect.MinMaxRect(startPosition.x, startPosition.y, startPosition.x, startPosition.y + kVerticalMargins), true) ||
                rect.Overlaps(Rect.MinMaxRect(endPosition.x, endPosition.y, endPosition.x, endPosition.y - kVerticalMargins), true) ||
                rect.Overlaps(Rect.MinMaxRect(startPosition.x, startPosition.y + kVerticalMargins, halfwayX, startPosition.y + kVerticalMargins), true) ||
                rect.Overlaps(Rect.MinMaxRect(halfwayX, endPosition.y - kVerticalMargins, halfwayX, startPosition.y + kVerticalMargins), true) ||
                rect.Overlaps(Rect.MinMaxRect(endPosition.x, endPosition.y - kVerticalMargins, halfwayX, endPosition.y - kVerticalMargins), true);
        }
    }
}
#endif