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

            style.width = 20;
            style.height = 20;
            style.borderTopLeftRadius = 10;
            style.borderTopRightRadius = 10;
            style.borderBottomLeftRadius = 10;
            style.borderBottomRightRadius = 10;
            style.borderLeftWidth = 2;
            style.borderRightWidth = 2;
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
            style.flexShrink = 0;
            style.alignItems = Align.Center;
            style.justifyContent = Justify.Center;
            _innerCircle = new VisualElement();
            _innerCircle.style.width = 10;
            _innerCircle.style.height = 10;
            _innerCircle.style.borderTopLeftRadius = 5;
            _innerCircle.style.borderTopRightRadius = 5;
            _innerCircle.style.borderBottomLeftRadius = 5;
            _innerCircle.style.borderBottomRightRadius = 5;
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
                style.borderLeftColor = new Color(0.7f, 0.7f, 0.7f);
                style.borderRightColor = new Color(0.7f, 0.7f, 0.7f);
                style.borderTopColor = new Color(0.7f, 0.7f, 0.7f);
                style.borderBottomColor = new Color(0.7f, 0.7f, 0.7f);
                style.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);
                _innerCircle.style.display = DisplayStyle.Flex;
                _innerCircle.style.backgroundColor = Color.white;
            }
            else
            {
                style.borderLeftColor = new Color(0.45f, 0.45f, 0.45f);
                style.borderRightColor = new Color(0.45f, 0.45f, 0.45f);
                style.borderTopColor = new Color(0.45f, 0.45f, 0.45f);
                style.borderBottomColor = new Color(0.45f, 0.45f, 0.45f);
                style.backgroundColor = Color.clear;
                _innerCircle.style.display = DisplayStyle.None;
            }
        }
    }
}
