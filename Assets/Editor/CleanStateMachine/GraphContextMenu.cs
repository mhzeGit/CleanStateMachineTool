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

        public event Action<Vector2> CreateStateRequested;
        public event Action<StateView> ConnectRequested;

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

        public void Show(Vector2 graphMousePosition, StateView contextNode = null)
        {
            _contextNode = contextNode;

            var menu = new GenericMenu();

            AddDefaultItems(menu, graphMousePosition);
            AddProviderItems(menu, graphMousePosition);

            menu.ShowAsContext();
        }

        private void AddDefaultItems(GenericMenu menu, Vector2 graphMousePosition)
        {
            menu.AddItem(new GUIContent("Create State"), false, () => CreateStateRequested?.Invoke(graphMousePosition));
            menu.AddSeparator(string.Empty);
            menu.AddDisabledItem(new GUIContent("Copy"));
            menu.AddDisabledItem(new GUIContent("Paste"));
            menu.AddDisabledItem(new GUIContent("Delete"));
            menu.AddSeparator(string.Empty);

            if (_contextNode != null)
            {
                StateView captured = _contextNode;
                menu.AddItem(new GUIContent("Connect"), false, () => ConnectRequested?.Invoke(captured));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Connect"));
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
