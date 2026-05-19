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

        private void OnEnable()
        {
            wantsMouseMove = true;
            _graphView = new GraphView();
            _panController = new GraphPanController();
            _contextMenu = new GraphContextMenu();

            _contextMenu.CreateStateRequested += OnCreateStateRequested;
        }

        private void OnDisable()
        {
            _contextMenu.CreateStateRequested -= OnCreateStateRequested;
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
                Vector2 graphMousePosition = (e.mousePosition - _panOffset) / _zoom;
                _contextMenu.Show(graphMousePosition);
                e.Use();
            }

            _graphView.Draw(rect, _panOffset, _zoom);

            if (_panController.IsPanning)
                Repaint();
        }

        private void OnCreateStateRequested(Vector2 graphMousePosition)
        {
            Debug.Log($"Create State at graph position: {graphMousePosition}");
        }
    }
}
