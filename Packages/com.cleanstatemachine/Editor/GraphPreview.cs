using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CleanStateMachine
{
    public class GraphPreview : VisualElement
    {
        private List<StateView> _states;
        private List<ConnectionView> _connections;
        private System.Func<StateView, bool> _isStateVisible;
        private System.Func<ConnectionView, bool> _isConnectionVisible;

        private Vector2 _panOffset;
        private float _zoom;
        private Rect _graphScreenRect;

        private bool _isDragging;
        private Vector2 _dragStartMouse;
        private Vector2 _dragOffset;

        private const float Padding = 8f;
        private static readonly Color ViewportBorderColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);
        private static readonly Color ViewportOverlayColor = new Color(0f, 0f, 0f, 0.55f);

        private static readonly Color DefaultStateColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color EntryStateColor = new Color(0.35f, 0.6f, 0.35f, 1f);
        private static readonly Color SubStateMachineColor = new Color(0.7f, 0.45f, 0.2f, 1f);
        private static readonly Color SubEntryStateColor = new Color(0.85f, 0.6f, 0.2f, 1f);
        private static readonly Color ExternalReferenceColor = new Color(0.25f, 0.25f, 0.55f, 1f);
        private static readonly Color ConnectionColor = new Color(0.35f, 0.35f, 0.35f, 1f);

        public Vector2 DragOffset => _dragOffset;

        public GraphPreview()
        {
            pickingMode = PickingMode.Position;
            generateVisualContent += OnGenerateVisualContent;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.button == 0)
            {
                _isDragging = true;
                _dragStartMouse = e.mousePosition;
                this.CaptureMouse();
                e.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!_isDragging) return;

            Vector2 delta = e.mousePosition - _dragStartMouse;
            _dragOffset += delta;
            _dragStartMouse = e.mousePosition;

            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (_isDragging && e.button == 0)
            {
                _isDragging = false;
                this.ReleaseMouse();
                e.StopPropagation();
            }
        }

        public void UpdateView(
            List<StateView> states,
            List<ConnectionView> connections,
            System.Func<StateView, bool> isStateVisible,
            System.Func<ConnectionView, bool> isConnectionVisible,
            Vector2 panOffset,
            float zoom,
            Rect graphScreenRect)
        {
            _states = states;
            _connections = connections;
            _isStateVisible = isStateVisible;
            _isConnectionVisible = isConnectionVisible;
            _panOffset = panOffset;
            _zoom = zoom;
            _graphScreenRect = graphScreenRect;
            MarkDirtyRepaint();
        }

        private Rect ComputeGraphBounds()
        {
            if (_states == null || _states.Count == 0)
                return new Rect(0f, 0f, 1f, 1f);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < _states.Count; i++)
            {
                var state = _states[i];
                if (_isStateVisible != null && !_isStateVisible(state))
                    continue;

                Rect r = state.GetGraphBounds();
                if (r.xMin < minX) minX = r.xMin;
                if (r.yMin < minY) minY = r.yMin;
                if (r.xMax > maxX) maxX = r.xMax;
                if (r.yMax > maxY) maxY = r.yMax;
            }

            if (minX > maxX)
                return new Rect(0f, 0f, 1f, 1f);

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private Rect ComputeViewportGraphRect()
        {
            if (Mathf.Approximately(_zoom, 0f))
                return new Rect(0f, 0f, 0f, 0f);

            Vector2 topLeft = (_graphScreenRect.min - _panOffset) / _zoom;
            Vector2 bottomRight = (_graphScreenRect.max - _panOffset) / _zoom;
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Rect bounds = contentRect;
            if (bounds.width < 1f || bounds.height < 1f || _states == null || _states.Count == 0)
                return;

            Rect previewArea = new Rect(
                bounds.x + Padding,
                bounds.y + Padding,
                bounds.width - Padding * 2f,
                bounds.height - Padding * 2f);

            if (previewArea.width < 1f || previewArea.height < 1f)
                return;

            Rect graphBounds = ComputeGraphBounds();
            Rect viewportGraphRect = ComputeViewportGraphRect();

            float scaleX = previewArea.width / Mathf.Max(0.001f, graphBounds.width);
            float scaleY = previewArea.height / Mathf.Max(0.001f, graphBounds.height);
            float scale = Mathf.Min(scaleX, scaleY);

            float cx = previewArea.x + previewArea.width * 0.5f;
            float cy = previewArea.y + previewArea.height * 0.5f;
            float gcx = graphBounds.x + graphBounds.width * 0.5f;
            float gcy = graphBounds.y + graphBounds.height * 0.5f;

            System.Func<Vector2, Vector2> graphToPreview = (Vector2 gp) =>
            {
                float px = cx + (gp.x - gcx) * scale;
                float py = cy + (gp.y - gcy) * scale;
                return new Vector2(px, py);
            };

            DrawConnections(mgc, graphToPreview);
            DrawStates(mgc, graphToPreview);
            DrawViewportOverlay(mgc, viewportGraphRect, graphToPreview);
        }

        private void DrawConnections(MeshGenerationContext mgc, System.Func<Vector2, Vector2> graphToPreview)
        {
            if (_connections == null) return;

            for (int i = 0; i < _connections.Count; i++)
            {
                var conn = _connections[i];
                if (_isConnectionVisible != null && !_isConnectionVisible(conn))
                    continue;

                Vector2 from = graphToPreview(conn.From.GetCenter());
                Vector2 to = graphToPreview(conn.To.GetCenter());

                Vector3 start = new Vector3(from.x, from.y, 0f);
                Vector3 end = new Vector3(to.x, to.y, 0f);

                if (Vector3.Distance(start, end) < 0.5f)
                    continue;

                DrawThinLine(mgc, start, end, ConnectionColor);
            }
        }

        private void DrawStates(MeshGenerationContext mgc, System.Func<Vector2, Vector2> graphToPreview)
        {
            if (_states == null) return;

            float minSize = 4f;

            for (int i = 0; i < _states.Count; i++)
            {
                var state = _states[i];
                if (_isStateVisible != null && !_isStateVisible(state))
                    continue;

                Rect graphRect = state.GetGraphBounds();
                Vector2 previewTL = graphToPreview(new Vector2(graphRect.x, graphRect.y));
                Vector2 previewBR = graphToPreview(new Vector2(graphRect.xMax, graphRect.yMax));

                float w = previewBR.x - previewTL.x;
                float h = previewBR.y - previewTL.y;
                if (w < 0.5f) w = minSize;
                if (h < 0.5f) h = minSize;

                Color color;
                if (state.IsSubEntry)
                    color = SubEntryStateColor;
                else if (state.IsSubStateMachine)
                    color = SubStateMachineColor;
                else if (state.IsEntry)
                    color = EntryStateColor;
                else if (state.IsExternalReference)
                    color = ExternalReferenceColor;
                else
                    color = DefaultStateColor;

                float halfW = w * 0.5f;
                float halfH = h * 0.5f;
                float cx = previewTL.x + halfW;
                float cy = previewTL.y + halfH;

                Vector3[] verts = new Vector3[4]
                {
                    new Vector3(cx - halfW, cy - halfH, 0f),
                    new Vector3(cx + halfW, cy - halfH, 0f),
                    new Vector3(cx + halfW, cy + halfH, 0f),
                    new Vector3(cx - halfW, cy + halfH, 0f),
                };

                var mesh = mgc.Allocate(4, 6);
                for (int vi = 0; vi < 4; vi++)
                    mesh.SetNextVertex(new Vertex { position = verts[vi], tint = color });
                mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(2);
                mesh.SetNextIndex(0); mesh.SetNextIndex(2); mesh.SetNextIndex(3);
            }
        }

        private void DrawViewportOverlay(
            MeshGenerationContext mgc,
            Rect viewportGraphRect,
            System.Func<Vector2, Vector2> graphToPreview)
        {
            Vector2 vpMin = graphToPreview(new Vector2(viewportGraphRect.xMin, viewportGraphRect.yMin));
            Vector2 vpMax = graphToPreview(new Vector2(viewportGraphRect.xMax, viewportGraphRect.yMax));

            float vpX = vpMin.x;
            float vpY = vpMin.y;
            float vpW = vpMax.x - vpMin.x;
            float vpH = vpMax.y - vpMin.y;

            if (vpW < 1f || vpH < 1f)
                return;

            Rect total = new Rect(contentRect.x, contentRect.y, contentRect.width, contentRect.height);

            Rect clipR = new Rect(vpX, vpY, vpW, vpH);
            clipR.xMin = Mathf.Max(clipR.xMin, total.xMin);
            clipR.xMax = Mathf.Min(clipR.xMax, total.xMax);
            clipR.yMin = Mathf.Max(clipR.yMin, total.yMin);
            clipR.yMax = Mathf.Min(clipR.yMax, total.yMax);

            if (clipR.width < 1f || clipR.height < 1f)
                return;

            float leftX = total.xMin;
            float topY = total.yMin;
            float centerLeft = clipR.xMin;
            float centerRight = clipR.xMax;
            float centerTop = clipR.yMin;
            float centerBottom = clipR.yMax;

            Color ov = ViewportOverlayColor;

            float lw = centerLeft - leftX;
            float rw = total.xMax - centerRight;
            float th = centerTop - topY;
            float bh = total.yMax - centerBottom;

            int rectCount = 0;
            if (lw > 0f) rectCount++;
            if (rw > 0f) rectCount++;
            if (th > 0f) rectCount++;
            if (bh > 0f) rectCount++;

            if (rectCount == 0) return;

            var mesh = mgc.Allocate(rectCount * 4, rectCount * 6);
            int vi = 0;

            if (lw > 0f)
                AddRect(mesh, leftX, topY, lw, total.height, ov, ref vi);
            if (rw > 0f)
                AddRect(mesh, centerRight, topY, rw, total.height, ov, ref vi);
            if (th > 0f)
                AddRect(mesh, centerLeft, topY, centerRight - centerLeft, th, ov, ref vi);
            if (bh > 0f)
                AddRect(mesh, centerLeft, centerBottom, centerRight - centerLeft, bh, ov, ref vi);

            DrawViewportBorder(mgc, clipR);
        }

        private void DrawViewportBorder(MeshGenerationContext mgc, Rect r)
        {
            float thick = 1f;
            var mesh = mgc.Allocate(16, 24);
            int vi = 0;

            AddRect(mesh, r.xMin, r.yMin, r.width, thick, ViewportBorderColor, ref vi);
            AddRect(mesh, r.xMin, r.yMax - thick, r.width, thick, ViewportBorderColor, ref vi);
            AddRect(mesh, r.xMin, r.yMin, thick, r.height, ViewportBorderColor, ref vi);
            AddRect(mesh, r.xMax - thick, r.yMin, thick, r.height, ViewportBorderColor, ref vi);
        }

        private static void DrawThinLine(MeshGenerationContext mgc, Vector3 start, Vector3 end, Color color)
        {
            Vector3 dir = (end - start).normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);
            float halfW = 0.7f;

            Vector3 sli = start + perp * halfW;
            Vector3 sri = start - perp * halfW;
            Vector3 eli = end + perp * halfW;
            Vector3 eri = end - perp * halfW;

            var mesh = mgc.Allocate(4, 6);
            mesh.SetNextVertex(new Vertex { position = sli, tint = color });
            mesh.SetNextVertex(new Vertex { position = sri, tint = color });
            mesh.SetNextVertex(new Vertex { position = eli, tint = color });
            mesh.SetNextVertex(new Vertex { position = eri, tint = color });

            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(2);
            mesh.SetNextIndex(1); mesh.SetNextIndex(3); mesh.SetNextIndex(2);
        }

        private static void AddRect(MeshWriteData mesh, float x, float y, float w, float h, Color color, ref int vi)
        {
            int baseIndex = vi;
            mesh.SetNextVertex(new Vertex { position = new Vector3(x, y, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(x + w, y, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(x, y + h, 0f), tint = color });
            mesh.SetNextVertex(new Vertex { position = new Vector3(x + w, y + h, 0f), tint = color });

            mesh.SetNextIndex((ushort)baseIndex);
            mesh.SetNextIndex((ushort)(baseIndex + 1));
            mesh.SetNextIndex((ushort)(baseIndex + 2));
            mesh.SetNextIndex((ushort)(baseIndex + 1));
            mesh.SetNextIndex((ushort)(baseIndex + 3));
            mesh.SetNextIndex((ushort)(baseIndex + 2));

            vi += 4;
        }
    }
}
