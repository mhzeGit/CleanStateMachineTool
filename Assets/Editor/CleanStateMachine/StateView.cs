using UnityEngine;

namespace CleanStateMachine
{
    public class StateView
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public string Name { get; set; }

        private static GUIStyle _style;
        private static readonly Color FillColor = new Color(0.18f, 0.18f, 0.20f);
        private const float DefaultWidth = 160f;
        private const float DefaultHeight = 60f;
        private const int TexWidth = 24;
        private const int TexHeight = 24;
        private const int CornerRadius = 8;

        public StateView(Vector2 position, string name = "State")
        {
            Position = position;
            Size = new Vector2(DefaultWidth, DefaultHeight);
            Name = name;
        }

        public void Draw(float zoom, Vector2 panOffset)
        {
            if (_style == null)
                _style = CreateStyle();

            Vector2 screenPos = Position * zoom + panOffset;
            Vector2 scaledSize = Size * zoom;

            var rect = new Rect(screenPos.x, screenPos.y, scaledSize.x, scaledSize.y);

            int fontSize = Mathf.Max(10, Mathf.RoundToInt(12 * zoom));
            _style.fontSize = fontSize;

            GUI.Box(rect, Name, _style);
        }

        private static GUIStyle CreateStyle()
        {
            var texture = GenerateTexture();
            texture.hideFlags = HideFlags.HideAndDontSave;

            return new GUIStyle
            {
                normal =
                {
                    background = texture,
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(CornerRadius, CornerRadius, CornerRadius, CornerRadius),
                padding = new RectOffset(4, 4, 4, 4)
            };
        }

        private static Texture2D GenerateTexture()
        {
            var tex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = Color.clear;
            int r = CornerRadius;

            for (int y = 0; y < TexHeight; y++)
            {
                for (int x = 0; x < TexWidth; x++)
                {
                    Color color = FillColor;

                    if (x < r && y < r)
                    {
                        float dx = r - x - 0.5f;
                        float dy = r - y - 0.5f;
                        if (dx * dx + dy * dy > r * r)
                            color = transparent;
                    }
                    else if (x >= TexWidth - r && y < r)
                    {
                        float dx = x - (TexWidth - r) + 0.5f;
                        float dy = r - y - 0.5f;
                        if (dx * dx + dy * dy > r * r)
                            color = transparent;
                    }
                    else if (x < r && y >= TexHeight - r)
                    {
                        float dx = r - x - 0.5f;
                        float dy = y - (TexHeight - r) + 0.5f;
                        if (dx * dx + dy * dy > r * r)
                            color = transparent;
                    }
                    else if (x >= TexWidth - r && y >= TexHeight - r)
                    {
                        float dx = x - (TexWidth - r) + 0.5f;
                        float dy = y - (TexHeight - r) + 0.5f;
                        if (dx * dx + dy * dy > r * r)
                            color = transparent;
                    }

                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
