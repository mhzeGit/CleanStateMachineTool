using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CleanStateMachine
{
    public static class MenuDropdown
    {
        private static StyleSheet _styleSheet;

        private static StyleSheet GetStyleSheet()
        {
            if (_styleSheet == null)
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/Editor/CleanStateMachine/Styles/MenuDropdown.uss");
            return _styleSheet;
        }

        public static void Show(VisualElement root, Vector2 screenPosition, Action<IBuilder> build)
        {
            var overlay = new VisualElement();
            overlay.AddToClassList("menu-dropdown-overlay");

            var menu = new VisualElement();
            menu.AddToClassList("menu-dropdown");
            menu.style.left = screenPosition.x;
            menu.style.top = screenPosition.y;

            var ss = GetStyleSheet();
            if (ss != null)
            {
                overlay.styleSheets.Add(ss);
                menu.styleSheets.Add(ss);
            }

            var builder = new Builder(menu, overlay);
            build(builder);

            overlay.Add(menu);

            overlay.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.target == overlay)
                {
                    overlay.RemoveFromHierarchy();
                    evt.StopPropagation();
                }
            });

            root.Add(overlay);
        }

        public interface IBuilder
        {
            void AddItem(string label, Action action);
            void AddSeparator();
            void AddDisabledItem(string label);
        }

        private class Builder : IBuilder
        {
            private readonly VisualElement _menu;
            private readonly VisualElement _overlay;

            public Builder(VisualElement menu, VisualElement overlay)
            {
                _menu = menu;
                _overlay = overlay;
            }

            public void AddItem(string label, Action action)
            {
                var btn = new Button(() =>
                {
                    action?.Invoke();
                    _overlay.RemoveFromHierarchy();
                })
                {
                    text = label
                };
                btn.AddToClassList("menu-dropdown-item");
                _menu.Add(btn);
            }

            public void AddSeparator()
            {
                var sep = new VisualElement();
                sep.AddToClassList("menu-dropdown-separator");
                _menu.Add(sep);
            }

            public void AddDisabledItem(string label)
            {
                var lbl = new Label(label);
                lbl.AddToClassList("menu-dropdown-disabled");
                _menu.Add(lbl);
            }
        }
    }
}
