using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CleanStateMachine
{
    public class ConnectionArrowsLayer : VisualElement
    {
        private readonly List<ConnectionView> _connections;
        private readonly ConnectionController _connectionController;
        private float _zoom = 1f;
        private Vector2 _panOffset;

        private static readonly Color ConnectionColor = new Color(0.537f, 0.706f, 0.980f, 0.85f);
        private static readonly Color SelectedColor = new Color(0.537f, 0.706f, 0.980f, 1f);
        private static readonly Color PendingColor = new Color(0.60f, 0.80f, 1.00f, 1.00f);

        private const float ArrowGraphSize = 10f;
        private const float ArrowGraphWidth = 5f;
        private const float BaseWidth = 2f;
        private const float SelectedBaseWidth = 3f;

        public ConnectionArrowsLayer(List<ConnectionView> connections, ConnectionController connectionController)
        {
            _connections = connections;
            _connectionController = connectionController;
            generateVisualContent += OnGenerateVisualContent;
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.overflow = Overflow.Hidden;
        }

        public void UpdateView(float zoom, Vector2 panOffset)
        {
            _zoom = zoom;
            _panOffset = panOffset;
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            DrawAllConnections(mgc);
            DrawPendingConnection(mgc);
        }

        private void DrawAllConnections(MeshGenerationContext mgc)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                var conn = _connections[i];
                GetScreenEndpoints(conn, out Vector3 startPos, out Vector3 endPos);

                bool isActive = conn.IsActive;
                Color color = conn.IsSelected ? SelectedColor : (isActive ? UITheme.ActiveConnection : ConnectionColor);
                float width = Mathf.Max(1f, (conn.IsSelected ? SelectedBaseWidth : BaseWidth) * _zoom);

                DrawLine(mgc, startPos, endPos, color, width);
                DrawMidArrowhead(mgc, startPos, endPos, color, _zoom);

                if (isActive)
                    DrawActiveWave(mgc, conn, startPos, endPos, _zoom);
            }
        }

        private void DrawPendingConnection(MeshGenerationContext mgc)
        {
            if (!_connectionController.IsConnecting) return;

            Vector3 startPos = _connectionController.SourceNode.GetCenter() * _zoom + _panOffset;
            Vector3 endPos = _connectionController.CurrentMouseGraphPos * _zoom + _panOffset;

            float width = Mathf.Max(1f, 1f * _zoom);
            DrawLine(mgc, startPos, endPos, PendingColor, width);

            if (Vector3.Distance(startPos, endPos) > 1f)
                DrawMidArrowhead(mgc, startPos, endPos, PendingColor, _zoom);
        }

        private Vector2 GetOffsetVector(ConnectionView conn)
        {
            if (conn.PerpendicularOffset == 0f) return Vector2.zero;
            Vector2 a = conn.From.GetCenter();
            Vector2 b = conn.To.GetCenter();
            Vector2 dir = (b - a).normalized;
            if (conn.From.GetHashCode() > conn.To.GetHashCode()) dir = -dir;
            return new Vector2(-dir.y, dir.x) * conn.PerpendicularOffset;
        }

        private void GetScreenEndpoints(ConnectionView conn, out Vector3 from, out Vector3 to)
        {
            Vector2 offset = GetOffsetVector(conn) * _zoom;
            from = conn.From.GetCenter() * _zoom + _panOffset + offset;
            to = conn.To.GetCenter() * _zoom + _panOffset + offset;
        }

        private static void DrawLine(MeshGenerationContext mgc, Vector3 start, Vector3 end, Color color, float width)
        {
            Vector3 dir = (end - start).normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);
            float halfW = width * 0.5f;

            var mesh = mgc.Allocate(4, 6);

            mesh.SetNextVertex(new Vertex { position = new Vector3(start.x + perp.x * halfW, start.y + perp.y * halfW, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(start.x - perp.x * halfW, start.y - perp.y * halfW, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(end.x - perp.x * halfW, end.y - perp.y * halfW, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(end.x + perp.x * halfW, end.y + perp.y * halfW, 0f), tint = color });

            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(0);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(3);
        }

        private static void DrawMidArrowhead(MeshGenerationContext mgc, Vector3 start, Vector3 end, Color color, float zoom)
        {
            Vector3 mid = (start + end) * 0.5f;
            Vector3 dir = (end - start).normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

            float arrowSize = Mathf.Max(6f, 10f * zoom);
            float arrowWidth = arrowSize * 0.5f;
            Vector3 basePt = mid - dir * arrowSize;

            var mesh = mgc.Allocate(3, 3);

            mesh.SetNextVertex(new Vertex { position = new Vector3(mid.x, mid.y, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(basePt.x + perp.x * arrowWidth, basePt.y + perp.y * arrowWidth, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(basePt.x - perp.x * arrowWidth, basePt.y - perp.y * arrowWidth, 0f), tint = color });

            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);
        }

        private static void DrawActiveWave(MeshGenerationContext mgc, ConnectionView conn, Vector3 start, Vector3 end, float zoom)
        {
            double elapsed = Time.realtimeSinceStartup - conn.ActivationTime;
            float fade = Mathf.Clamp01(1f - (float)(elapsed / 1.8));
            if (fade <= 0.01f)
            {
                conn.IsActive = false;
                return;
            }
            fade *= fade;

            Vector3 dir = (end - start).normalized;
            float totalLen = Vector3.Distance(start, end);
            if (totalLen < 0.01f) return;

            float speed = 1.5f;
            int circleCount = 5;
            float circleRadius = Mathf.Max(1.5f, 3f * zoom);

            for (int i = 0; i < circleCount; i++)
            {
                float phase = (float)i / circleCount;
                float t = (Time.realtimeSinceStartup * speed + phase) % 1.0f;

                Vector3 pos = start + dir * (t * totalLen);

                Color circleColor = UITheme.ActiveConnectionWave;
                circleColor.a *= fade * (0.5f + 0.3f * Mathf.Sin(i * 2.5f + 1f));

                DrawCircle(mgc, pos, circleRadius, circleColor);
            }

            if (fade < 0.5f)
            {
                Color color = UITheme.ActiveConnection;
                color.a *= fade * 2f;
                float width = Mathf.Max(1f, 2f * zoom);
                DrawLine(mgc, start, end, color, width);
                DrawMidArrowhead(mgc, start, end, color, zoom);
            }
        }

        private static void DrawCircle(MeshGenerationContext mgc, Vector3 center, float radius, Color color)
        {
            int segments = 12;
            int vertCount = segments + 1;
            int indexCount = segments * 3;

            var mesh = mgc.Allocate(vertCount, indexCount);

            mesh.SetNextVertex(new Vertex { position = new Vector3(center.x, center.y, 0f), tint = color });

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                mesh.SetNextVertex(new Vertex
                {
                    position = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, 0f),
                    tint = color
                });
            }

            for (int i = 0; i < segments; i++)
            {
                ushort next = (ushort)((i + 1) % segments);
                mesh.SetNextIndex(0);
                mesh.SetNextIndex((ushort)(i + 1));
                mesh.SetNextIndex((ushort)(next + 1));
            }
        }
    }
}
