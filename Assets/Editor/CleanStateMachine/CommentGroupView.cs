using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CleanStateMachine
{
    public class CommentGroupView : VisualElement, ISelectable
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                UpdateBorderStyle();
            }
        }

        private Vector2 _fallbackPosition;
        private readonly List<StateView> _members = new();

        public string Label { get; set; }
        public IReadOnlyList<StateView> Members => _members;

        private const float PadH = 20f;
        private const float PadTop = 30f;
        private const float PadBot = 15f;
        private const float CRadius = 12f;

        private static readonly Color BgCol = new Color(0.18f, 0.18f, 0.18f, 0.25f);
        private static readonly Color BorderCol = new Color(0.40f, 0.40f, 0.40f, 0.35f);
        private static readonly Color SelBorderCol = new Color(0.70f, 0.70f, 0.70f, 0.80f);

        private readonly VisualElement _header;
        private readonly Label _label;

        private float _lastZoom = 1f;

        public CommentGroupView(IEnumerable<StateView> members, string label = "Comment Group")
        {
            Label = label;
            _members.AddRange(members);

            pickingMode = PickingMode.Ignore;
            style.position = UnityEngine.UIElements.Position.Absolute;
            style.overflow = Overflow.Hidden;
            style.backgroundColor = BgCol;

            _header = new VisualElement();
            _header.AddToClassList("comment-group__header");
            Add(_header);

            _label = new Label(Label);
            _label.AddToClassList("comment-group__label");
            _header.Add(_label);

            UpdateBorderStyle();
        }

        private Rect GetMembersBounds()
        {
            if (_members.Count == 0)
                return new Rect(_fallbackPosition.x, _fallbackPosition.y, 160f, 40f);

            float xMin = float.MaxValue, xMax = float.MinValue;
            float yMin = float.MaxValue, yMax = float.MinValue;
            for (int i = 0; i < _members.Count; i++)
            {
                Rect r = _members[i].GetGraphBounds();
                if (r.xMin < xMin) xMin = r.xMin;
                if (r.xMax > xMax) xMax = r.xMax;
                if (r.yMin < yMin) yMin = r.yMin;
                if (r.yMax > yMax) yMax = r.yMax;
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public Vector2 Position
        {
            get
            {
                Rect b = GetMembersBounds();
                return new Vector2(b.x - PadH, b.y - PadTop);
            }
            set
            {
                if (_members.Count == 0)
                {
                    _fallbackPosition = value;
                    return;
                }

                Vector2 current = Position;
                Vector2 delta = value - current;
                if (delta.sqrMagnitude < 0.0001f) return;

                for (int i = 0; i < _members.Count; i++)
                    _members[i].Position += delta;
            }
        }

        public Vector2 Size
        {
            get
            {
                Rect b = GetMembersBounds();
                return new Vector2(b.width + PadH * 2f, b.height + PadTop + PadBot);
            }
        }

        public Rect GetGraphBounds()
        {
            return new Rect(Position.x, Position.y, Size.x, Size.y);
        }

        public bool ContainsPoint(Vector2 p)
        {
            Rect b = GetGraphBounds();
            if (!b.Contains(p)) return false;

            float r = CRadius;
            if (p.x < b.x + r && p.y < b.y + r) { float dx = p.x - (b.x + r); float dy = p.y - (b.y + r); return dx * dx + dy * dy <= r * r; }
            if (p.x > b.xMax - r && p.y < b.y + r) { float dx = p.x - (b.xMax - r); float dy = p.y - (b.y + r); return dx * dx + dy * dy <= r * r; }
            if (p.x < b.x + r && p.y > b.yMax - r) { float dx = p.x - (b.x + r); float dy = p.y - (b.yMax - r); return dx * dx + dy * dy <= r * r; }
            if (p.x > b.xMax - r && p.y > b.yMax - r) { float dx = p.x - (b.xMax - r); float dy = p.y - (b.yMax - r); return dx * dx + dy * dy <= r * r; }
            return true;
        }

        public void UpdateScreenPosition(float zoom, Vector2 panOffset)
        {
            _lastZoom = zoom;

            Rect b = GetGraphBounds();
            Vector2 sp = b.position * zoom + panOffset;
            Vector2 ss = b.size * zoom;

            style.left = sp.x;
            style.top = sp.y;
            style.width = ss.x;
            style.height = ss.y;

            int r = Mathf.Max(1, Mathf.RoundToInt(CRadius * zoom));
            style.borderTopLeftRadius = r;
            style.borderTopRightRadius = r;
            style.borderBottomLeftRadius = r;
            style.borderBottomRightRadius = r;

            UpdateBorderStyle();

            float headerH = Mathf.Max(1f, 24f * zoom);
            _header.style.height = headerH;
            _label.style.fontSize = Mathf.RoundToInt(11f * zoom);
        }

        private void UpdateBorderStyle()
        {
            int bw = Mathf.Max(1, Mathf.RoundToInt((_isSelected ? 2f : 1f) * _lastZoom));
            style.borderLeftWidth = bw;
            style.borderRightWidth = bw;
            style.borderTopWidth = bw;
            style.borderBottomWidth = bw;

            Color bc = _isSelected ? SelBorderCol : BorderCol;
            style.borderLeftColor = bc;
            style.borderRightColor = bc;
            style.borderTopColor = bc;
            style.borderBottomColor = bc;
        }

        public void DrawSelectionOverlay(float zoom, Vector2 panOffset)
        {
        }
    }
}
