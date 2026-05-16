using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using SM = StateMachineTool.Runtime;

namespace StateMachineTool.Editor
{
    public class StateMachineGraphView : GraphView
    {
        public SM.StateMachineAsset Asset { get; private set; }
        public System.Action<SM.StateData> OnStateSelected;
        public System.Action<SM.TransitionData> OnTransitionSelected;
        public System.Action OnGraphChanged;

        private Vector2 lastMousePosition;
        private bool isBuilding;

        public StateMachineGraphView()
        {
            this.StretchToParentSize();

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            var minimap = new MiniMap { anchored = true };
            minimap.SetPosition(new Rect(10, 10, 200, 140));
            Add(minimap);

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);

            RegisterCallback<MouseMoveEvent>(e => lastMousePosition = e.localMousePosition);

            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace)
                {
                    DeleteSelection();
                    evt.StopPropagation();
                }
            });

            graphViewChanged += OnGraphViewChanged;

            nodeCreationRequest = ctx =>
            {
                if (Asset == null) return;
                var pos = contentViewContainer.WorldToLocal(ctx.screenMousePosition);
                CreateState($"State {Asset.graphData.states.Count}", pos);
            };

            style.flexGrow = 1;
        }

        // ====== Asset Loading ======

        public void LoadAsset(SM.StateMachineAsset asset)
        {
            Asset = asset;
            if (Asset == null) return;
            isBuilding = true;
            ClearUserElements();
            BuildElements();
            isBuilding = false;
        }

        public void RefreshGraph()
        {
            if (Asset == null) return;
            isBuilding = true;
            ClearUserElements();
            BuildElements();
            isBuilding = false;
        }

        private void ClearUserElements()
        {
            foreach (var e in edges.ToList()) RemoveElement(e);
            foreach (var n in nodes.ToList()) RemoveElement(n);
        }

        private void BuildElements()
        {
            foreach (var s in Asset.graphData.states) AddStateNode(s);
            foreach (var t in Asset.graphData.transitions) AddTransitionEdge(t);
        }

        // ====== State Node Management ======

        public StateNodeView CreateState(string name, Vector2 worldPos)
        {
            if (Asset == null) return null;
            Undo.RecordObject(Asset, "Create State");
            var data = new SM.StateData(name, worldPos);
            Asset.graphData.states.Add(data);
            EditorUtility.SetDirty(Asset);
            var node = AddStateNode(data);
            OnGraphChanged?.Invoke();
            return node;
        }

        public StateNodeView CreateEntryState(string name, Vector2 worldPos)
        {
            if (Asset == null) return null;
            Undo.RecordObject(Asset, "Create Entry State");
            var data = new SM.StateData(name, worldPos, SM.StateType.Entry);
            Asset.graphData.states.Add(data);
            Asset.graphData.entryStateId = data.id;
            EditorUtility.SetDirty(Asset);
            var node = AddStateNode(data);
            OnGraphChanged?.Invoke();
            return node;
        }

        private StateNodeView AddStateNode(SM.StateData data)
        {
            var node = new StateNodeView(data.id, data.displayName, data.stateType, data.position);
            node.RegisterCallback<PointerDownEvent>(_ => OnStateSelected?.Invoke(data));
            AddElement(node);
            return node;
        }

        // ====== Transition Management ======

        private void AddTransitionEdge(SM.TransitionData data)
        {
            var fromNode = GetNodeById(data.fromStateId);
            var toNode = GetNodeById(data.toStateId);
            if (fromNode == null || toNode == null || fromNode.OutputPort == null || toNode.InputPort == null) return;

            var edge = new TransitionEdgeView(data.id);
            edge.output = fromNode.OutputPort;
            edge.input = toNode.InputPort;
            edge.output.Connect(edge);
            edge.input.Connect(edge);
            var capturedId = data.id;
            edge.RegisterCallback<PointerDownEvent>(_ =>
            {
                var t = Asset.graphData.transitions.FirstOrDefault(x => x.id == capturedId);
                if (t != null) OnTransitionSelected?.Invoke(t);
            });
            AddElement(edge);
        }

        public StateNodeView GetNodeById(string stateId)
        {
            return nodes.OfType<StateNodeView>().FirstOrDefault(n => n.StateId == stateId);
        }

        // ====== Port Compatibility ======

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node
            ).ToList();
        }

        // ====== Graph Changes ======

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (isBuilding) return change;

            if (change.elementsToRemove != null)
            {
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is TransitionEdgeView te)
                    {
                        Asset.graphData.transitions.RemoveAll(t => t.id == te.TransitionId);
                    }
                    else if (elem is StateNodeView sn)
                    {
                        Asset.graphData.transitions.RemoveAll(t => t.fromStateId == sn.StateId || t.toStateId == sn.StateId);
                        Asset.graphData.states.RemoveAll(s => s.id == sn.StateId);
                        if (Asset.graphData.entryStateId == sn.StateId)
                            Asset.graphData.entryStateId = null;
                    }
                }
                EditorUtility.SetDirty(Asset);
                OnGraphChanged?.Invoke();
            }

            if (change.edgesToCreate != null)
            {
                for (int i = change.edgesToCreate.Count - 1; i >= 0; i--)
                {
                    var edge = change.edgesToCreate[i];
                    if (edge.output?.node is StateNodeView fromNode &&
                        edge.input?.node is StateNodeView toNode &&
                        fromNode.StateId != toNode.StateId)
                    {
                        Undo.RecordObject(Asset, "Create Transition");
                        var fromState = Asset.GetState(fromNode.StateId);
                        var toState = Asset.GetState(toNode.StateId);
                        if (fromState != null && toState != null)
                        {
                            var data = new SM.TransitionData(fromState.id, toState.id)
                            {
                                displayName = $"{fromState.displayName} -> {toState.displayName}"
                            };
                            Asset.graphData.transitions.Add(data);

                            var te = new TransitionEdgeView(data.id);
                            te.output = edge.output;
                            te.input = edge.input;
                            var capturedId = data.id;
                            te.RegisterCallback<PointerDownEvent>(_ =>
                            {
                                var t = Asset.graphData.transitions.FirstOrDefault(x => x.id == capturedId);
                                if (t != null) OnTransitionSelected?.Invoke(t);
                            });
                            change.edgesToCreate[i] = te;
                        }
                    }
                }
                EditorUtility.SetDirty(Asset);
                OnGraphChanged?.Invoke();
            }

            if (change.movedElements != null)
            {
                foreach (var elem in change.movedElements)
                {
                    if (elem is StateNodeView sn)
                    {
                        var data = Asset.GetState(sn.StateId);
                        if (data != null) data.position = sn.GetPosition().position;
                    }
                }
                EditorUtility.SetDirty(Asset);
            }

            return change;
        }

        // ====== Context Menu ======

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Asset == null) return;

            evt.menu.AppendAction("Add State", _ =>
            {
                var pos = viewTransform.matrix.inverse.MultiplyPoint(lastMousePosition);
                CreateState($"State {Asset.graphData.states.Count}", pos);
            });

            evt.menu.AppendAction("Add Entry State", _ =>
            {
                var pos = viewTransform.matrix.inverse.MultiplyPoint(lastMousePosition);
                CreateEntryState("Entry", pos);
            });

            evt.menu.AppendSeparator();
        }
    }
}
