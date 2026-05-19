using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public class ConnectionView
    {
        public StateView From { get; }
        public StateView To { get; }

        private static readonly Color ConnectionColor = new Color(0.60f, 0.80f, 1.00f, 0.90f);

        public ConnectionView(StateView from, StateView to)
        {
            From = from;
            To = to;
        }

        public void Draw(float zoom, Vector2 panOffset)
        {
            Vector3 startPos = From.GetCenter() * zoom + panOffset;
            Vector3 endPos = To.GetCenter() * zoom + panOffset;

            Vector3 dir = (endPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPos);
            float tangentStrength = Mathf.Max(distance * 0.5f, 50f);
            Vector3 startTan = startPos + dir * tangentStrength;
            Vector3 endTan = endPos - dir * tangentStrength;

            Handles.DrawBezier(startPos, endPos, startTan, endTan, ConnectionColor, null, 3f);
            DrawArrowhead(endPos, endTan - endPos, ConnectionColor, zoom);
        }

        private static void DrawArrowhead(Vector3 tip, Vector3 tangent, Color color, float zoom)
        {
            float size = Mathf.Max(8f, 12f * zoom);
            Vector3 dir = tangent.normalized;
            Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

            Vector3 p1 = tip;
            Vector3 p2 = tip - dir * size + perp * (size * 0.4f);
            Vector3 p3 = tip - dir * size - perp * (size * 0.4f);

            Color prev = Handles.color;
            Handles.color = color;
            Handles.DrawAAConvexPolygon(p1, p2, p3);
            Handles.color = prev;
        }
    }
}
