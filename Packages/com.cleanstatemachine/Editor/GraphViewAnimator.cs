using UnityEngine;

namespace CleanStateMachine
{
    internal class GraphViewAnimator
    {
        private readonly CleanStateMachineWindow _window;

        public GraphViewAnimator(CleanStateMachineWindow window)
        {
            _window = window;
        }

        public void StartSmoothFocusOnContent(Rect contentBounds)
        {
            if (contentBounds.width < 0.001f || contentBounds.height < 0.001f)
                return;

            float sideW = _window.ShowSidePanel ? _window.SidePanelWidth : CleanStateMachineWindow.CollapsedPanelWidth;
            const float barH = 24f;
            Rect graphRect = new Rect(0f, barH, _window.position.width - sideW, _window.position.height - barH);
            if (graphRect.width < 1f || graphRect.height < 1f)
                return;

            const float padding = 0.12f;
            float availableWidth = graphRect.width * (1f - 2f * padding);
            float availableHeight = graphRect.height * (1f - 2f * padding);

            float zoomX = availableWidth / contentBounds.width;
            float zoomY = availableHeight / contentBounds.height;
            float targetZoom = Mathf.Min(zoomX, zoomY);
            targetZoom = Mathf.Clamp(targetZoom, 0.1f, 5f);

            Vector2 contentCenter = contentBounds.center;
            Vector2 viewportCenter = graphRect.center;
            Vector2 targetPan = viewportCenter - contentCenter * targetZoom;

            _window.AnimFromPan = _window.PanOffset;
            _window.AnimToPan = targetPan;
            _window.AnimFromZoom = _window.Zoom;
            _window.AnimToZoom = targetZoom;
            _window.AnimStartTime = UnityEditor.EditorApplication.timeSinceStartup;
            _window.IsAnimatingView = true;
        }

        public bool UpdateAnimation(ref Vector2 panOffset, ref float zoom, GraphPanController panController)
        {
            if (!_window.IsAnimatingView) return false;

            if (panController.UserInteractedThisFrame || panController.IsPanning)
            {
                _window.IsAnimatingView = false;
                return false;
            }

            float t = (float)(UnityEditor.EditorApplication.timeSinceStartup - _window.AnimStartTime) / CleanStateMachineWindow.AnimDuration;
            if (t >= 1f)
            {
                panOffset = _window.AnimToPan;
                zoom = _window.AnimToZoom;
                _window.IsAnimatingView = false;
            }
            else
            {
                float smoothT = t * t * (3f - 2f * t);
                panOffset = Vector2.Lerp(_window.AnimFromPan, _window.AnimToPan, smoothT);
                zoom = Mathf.Lerp(_window.AnimFromZoom, _window.AnimToZoom, smoothT);
            }

            return true;
        }
    }
}
