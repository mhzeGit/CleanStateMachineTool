using UnityEngine;
using UnityEngine.UIElements;

namespace CleanStateMachine
{
    public class TriggerToggle : VisualElement
    {
        private readonly VisualElement _innerCircle;
        private bool _value;

        public bool Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                UpdateVisual();
                OnValueChanged?.Invoke(_value);
            }
        }

        public event System.Action<bool> OnValueChanged;

        public TriggerToggle(bool initialValue = false)
        {
            _value = initialValue;

            style.width = 16;
            style.height = 16;
            style.borderTopLeftRadius = 8;
            style.borderTopRightRadius = 8;
            style.borderBottomLeftRadius = 8;
            style.borderBottomRightRadius = 8;
            style.borderLeftWidth = 1.5f;
            style.borderRightWidth = 1.5f;
            style.borderTopWidth = 1.5f;
            style.borderBottomWidth = 1.5f;
            style.flexShrink = 0;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            _innerCircle = new VisualElement();
            _innerCircle.style.width = 8;
            _innerCircle.style.height = 8;
            _innerCircle.style.borderTopLeftRadius = 4;
            _innerCircle.style.borderTopRightRadius = 4;
            _innerCircle.style.borderBottomLeftRadius = 4;
            _innerCircle.style.borderBottomRightRadius = 4;
            Add(_innerCircle);

            RegisterCallback<ClickEvent>(_ =>
            {
                Value = !_value;
            });

            UpdateVisual();
        }

        public void SetValueWithoutNotify(bool value)
        {
            if (_value == value) return;
            _value = value;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_value)
            {
                style.borderLeftColor = new Color(0.55f, 0.55f, 0.55f);
                style.borderRightColor = new Color(0.55f, 0.55f, 0.55f);
                style.borderTopColor = new Color(0.55f, 0.55f, 0.55f);
                style.borderBottomColor = new Color(0.55f, 0.55f, 0.55f);
                style.backgroundColor = new Color(0.55f, 0.55f, 0.55f, 0.35f);
                _innerCircle.style.display = DisplayStyle.Flex;
                _innerCircle.style.backgroundColor = Color.white;
            }
            else
            {
                style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
                style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
                style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
                style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
                style.backgroundColor = Color.clear;
                _innerCircle.style.display = DisplayStyle.None;
            }
        }
    }
}
