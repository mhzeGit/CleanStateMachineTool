using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public class GraphContextMenu
    {
        private readonly List<IContextMenuProvider> _providers = new();

        private StateView _contextNode;
        private CommentGroupView _contextGroup;

        public event Action<Vector2> CreateStateRequested;
        public event Action<StateView> ConnectRequested;
        public event Action<CommentGroupView> UngroupRequested;
        public event Action CopyRequested;
        public event Action PasteRequested;
        public event Action DeleteRequested;

        public void AddProvider(IContextMenuProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (!_providers.Contains(provider))
                _providers.Add(provider);
        }

        public void RemoveProvider(IContextMenuProvider provider)
        {
            _providers.Remove(provider);
        }

        public void Show(Vector2 graphMousePosition, StateView contextNode = null, CommentGroupView contextGroup = null, bool hasSelection = false, bool hasClipboard = false)
        {
            _contextNode = contextNode;
            _contextGroup = contextGroup;

            var menu = new GenericMenu();

            AddDefaultItems(menu, graphMousePosition, hasSelection, hasClipboard);
            AddProviderItems(menu, graphMousePosition);

            menu.ShowAsContext();
        }

        private void AddDefaultItems(GenericMenu menu, Vector2 graphMousePosition, bool hasSelection, bool hasClipboard)
        {
            menu.AddItem(new GUIContent("Create State"), false, () => CreateStateRequested?.Invoke(graphMousePosition));
            menu.AddSeparator(string.Empty);

            if (hasSelection)
                menu.AddItem(new GUIContent("Copy"), false, () => CopyRequested?.Invoke());
            else
                menu.AddDisabledItem(new GUIContent("Copy"));

            if (hasClipboard)
                menu.AddItem(new GUIContent("Paste"), false, () => PasteRequested?.Invoke());
            else
                menu.AddDisabledItem(new GUIContent("Paste"));

            if (hasSelection)
                menu.AddItem(new GUIContent("Delete"), false, () => DeleteRequested?.Invoke());
            else
                menu.AddDisabledItem(new GUIContent("Delete"));

            if (_contextGroup != null)
            {
                menu.AddSeparator(string.Empty);
                CommentGroupView captured = _contextGroup;
                menu.AddItem(new GUIContent("Ungroup"), false, () => UngroupRequested?.Invoke(captured));
            }

            if (_contextNode != null)
            {
                menu.AddSeparator(string.Empty);
                StateView captured = _contextNode;
                menu.AddItem(new GUIContent("Connect"), false, () => ConnectRequested?.Invoke(captured));
            }
        }

        private void AddProviderItems(GenericMenu menu, Vector2 graphMousePosition)
        {
            for (int i = 0; i < _providers.Count; i++)
            {
                _providers[i].AddItemsToMenu(menu, graphMousePosition);
            }
        }
    }
}
