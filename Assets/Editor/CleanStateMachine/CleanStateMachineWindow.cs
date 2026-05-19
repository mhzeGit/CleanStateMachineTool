using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public class CleanStateMachineWindow : EditorWindow
    {
        [MenuItem("Tools/CleanStateMachine")]
        public static void ShowWindow()
        {
            var window = CreateWindow<CleanStateMachineWindow>();
            window.titleContent = new GUIContent("CleanStateMachine");
            window.Show();
        }

        [SerializeField] private Vector2 _panOffset;
        [SerializeField] private float _zoom = 1f;

        private GraphView _graphView;
        private GraphPanController _panController;
        private GraphContextMenu _contextMenu;
        private SelectionController _selectionController;
        private DragController _dragController;
        private SelectionBox _selectionBox;
        private ConnectionController _connectionController;

        private readonly List<StateView> _states = new();
        private readonly List<ConnectionView> _connections = new();

        private void OnEnable()
        {
            wantsMouseMove = true;
            _graphView = new GraphView();
            _panController = new GraphPanController();
            _contextMenu = new GraphContextMenu();
            _selectionController = new SelectionController();
            _dragController = new DragController();
            _selectionBox = new SelectionBox();
            _connectionController = new ConnectionController();

            _contextMenu.CreateStateRequested += OnCreateStateRequested;
            _contextMenu.ConnectRequested += OnConnectRequested;
            _connectionController.ConnectionCompleted += OnConnectionCompleted;
        }

        private void OnDisable()
        {
            _contextMenu.CreateStateRequested -= OnCreateStateRequested;
            _contextMenu.ConnectRequested -= OnConnectRequested;
            _connectionController.ConnectionCompleted -= OnConnectionCompleted;
        }

        private void OnGUI()
        {
            if (position.width < 1f || position.height < 1f)
                return;

            var rect = new Rect(0f, 0f, position.width, position.height);
            var e = Event.current;

            _panController.HandleInput(rect, ref _panOffset, ref _zoom);

            if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
            {
                _connectionController.Cancel();
                Vector2 graphMousePosition = (e.mousePosition - _panOffset) / _zoom;
                StateView hitState = HitTestState(graphMousePosition);
                _contextMenu.Show(graphMousePosition, hitState);
                e.Use();
            }

            if (_connectionController.IsConnecting)
            {
                HandleConnectingInput(rect);
            }
            else
            {
                HandleLeftClickInteraction(rect);
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                if (_connectionController.IsConnecting)
                {
                    _connectionController.Cancel();
                    e.Use();
                    Repaint();
                }
            }

            _graphView.Draw(rect, _panOffset, _zoom);
            DrawConnections();
            DrawStates();
            DrawSelectionOverlays();
            _connectionController.DrawPending(_zoom, _panOffset);
            _selectionBox.DrawScreen(_zoom, _panOffset);

            if (_panController.IsPanning || _dragController.IsActive || _selectionBox.IsActive || _connectionController.IsConnecting)
                Repaint();
        }

        private void HandleConnectingInput(Rect viewRect)
        {
            var e = Event.current;
            Vector2 graphMousePos = (e.mousePosition - _panOffset) / _zoom;

            switch (e.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    _connectionController.UpdatePending(graphMousePos);
                    Repaint();
                    e.Use();
                    break;

                case EventType.MouseDown when e.button == 0 && viewRect.Contains(e.mousePosition):
                    if (!_connectionController.TryComplete(graphMousePos, _states))
                    {
                        _connectionController.Cancel();
                    }
                    e.Use();
                    Repaint();
                    break;
            }
        }

        private void HandleLeftClickInteraction(Rect viewRect)
        {
            var e = Event.current;
            if (e.button != 0)
                return;

            Vector2 graphMousePos = (e.mousePosition - _panOffset) / _zoom;

            switch (e.type)
            {
                case EventType.MouseDown when viewRect.Contains(e.mousePosition):
                    OnLeftMouseDown(graphMousePos, e);
                    break;

                case EventType.MouseDrag:
                    OnLeftMouseDrag(graphMousePos, e);
                    break;

                case EventType.MouseUp:
                    OnLeftMouseUp(graphMousePos, e);
                    break;
            }
        }

        private void OnLeftMouseDown(Vector2 graphPos, Event e)
        {
            ISelectable hit = HitTest(graphPos);

            if (hit != null)
            {
                if (e.shift)
                {
                    _selectionController.Toggle(hit);
                }
                else if (!_selectionController.IsSelected(hit))
                {
                    _selectionController.SelectOnly(hit);
                }

                _dragController.StartDrag(graphPos, _selectionController.Selected);
            }
            else
            {
                if (!e.shift)
                    _selectionController.Clear();

                _selectionBox.Start(graphPos);
            }

            e.Use();
        }

        private void OnLeftMouseDrag(Vector2 graphPos, Event e)
        {
            if (_dragController.IsActive)
            {
                _dragController.UpdateDrag(graphPos, _zoom);
            }
            else if (_selectionBox.IsActive)
            {
                _selectionBox.Update(graphPos);
            }

            e.Use();
        }

        private void OnLeftMouseUp(Vector2 graphPos, Event e)
        {
            if (_dragController.IsActive)
            {
                _dragController.EndDrag();
            }
            else if (_selectionBox.IsActive)
            {
                if (!e.shift)
                    _selectionController.Clear();

                Rect selectionGraphRect = _selectionBox.GetGraphRect();
                for (int i = 0; i < _states.Count; i++)
                {
                    if (selectionGraphRect.Overlaps(_states[i].GetGraphBounds()))
                    {
                        _selectionController.Select(_states[i]);
                    }
                }

                _selectionBox.End();
            }

            e.Use();
        }

        private ISelectable HitTest(Vector2 graphPos)
        {
            for (int i = _states.Count - 1; i >= 0; i--)
            {
                if (_states[i].ContainsPoint(graphPos))
                    return _states[i];
            }

            return null;
        }

        private StateView HitTestState(Vector2 graphPos)
        {
            for (int i = _states.Count - 1; i >= 0; i--)
            {
                if (_states[i].ContainsPoint(graphPos))
                    return _states[i];
            }

            return null;
        }

        private void DrawSelectionOverlays()
        {
            var selected = _selectionController.Selected;
            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].DrawSelectionOverlay(_zoom, _panOffset);
            }
        }

        private void DrawStates()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _states[i].Draw(_zoom, _panOffset);
            }
        }

        private void DrawConnections()
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                _connections[i].Draw(_zoom, _panOffset);
            }
        }

        private void OnCreateStateRequested(Vector2 graphMousePosition)
        {
            var state = new StateView(graphMousePosition);
            _states.Add(state);
            Repaint();
        }

        private void OnConnectRequested(StateView source)
        {
            _connectionController.StartConnection(source);
            Repaint();
        }

        private void OnConnectionCompleted(StateView from, StateView to)
        {
            _connections.Add(new ConnectionView(from, to));
            Repaint();
        }
    }
}
