using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using SM = StateMachineTool.Runtime;

namespace StateMachineTool.Editor
{
    public class StateMachineEditorWindow : EditorWindow
    {
        private SM.StateMachineAsset currentAsset;
        private StateMachineGraphView graphView;
        private BlackboardEditorView blackboardView;
        private StateInspectorView inspectorView;
        private TwoPaneSplitView mainSplit;
        private TwoPaneSplitView sideSplit;
        private Label assetLabel;
        private Button addStateBtn, addEntryBtn;

        public static void Open() => GetWindow<StateMachineEditorWindow>("State Machine");

        public static void OpenAsset(SM.StateMachineAsset asset)
        {
            var w = GetWindow<StateMachineEditorWindow>("State Machine");
            w.LoadAsset(asset);
        }

        void OnEnable()
        {
            BuildUI();
            LoadUSS();
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject is SM.StateMachineAsset a) LoadAsset(a);
        }

        // ====== Build ======

        void BuildUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            BuildToolbar(root);
            BuildPanels(root);
        }

        void BuildToolbar(VisualElement root)
        {
            var bar = new VisualElement();
            bar.style.flexDirection = FlexDirection.Row;
            bar.style.alignItems = Align.Center;
            bar.style.height = 28;
            bar.style.minHeight = 28;
            bar.style.backgroundColor = new Color(0.14f, 0.14f, 0.15f, 1f);
            bar.style.paddingLeft = 10;
            bar.style.paddingRight = 10;
            bar.style.borderBottomWidth = 1;
            bar.style.borderBottomColor = new Color(0.22f, 0.22f, 0.23f, 1f);
            root.Add(bar);

            assetLabel = new Label("No asset loaded");
            assetLabel.style.color = new Color(0.75f, 0.75f, 0.78f, 1f);
            assetLabel.style.fontSize = 12;
            assetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            assetLabel.style.minWidth = 120;
            bar.Add(assetLabel);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            bar.Add(spacer);

            addStateBtn = MakeToolbarButton("+ State", () =>
            {
                graphView?.CreateState($"State {currentAsset?.graphData.states.Count ?? 0}", Vector2.zero);
            });
            bar.Add(addStateBtn);

            addEntryBtn = MakeToolbarButton("+ Entry", () =>
            {
                graphView?.CreateEntryState("Entry", new Vector2(100, 0));
            });
            bar.Add(addEntryBtn);

            var saveBtn = MakeToolbarButton("Save", () =>
            {
                if (currentAsset != null) { EditorUtility.SetDirty(currentAsset); AssetDatabase.SaveAssets(); }
            });
            bar.Add(saveBtn);

            var refreshBtn = MakeToolbarButton("Refresh", () =>
            {
                graphView?.RefreshGraph();
                blackboardView?.Refresh();
            });
            bar.Add(refreshBtn);

            // Hide asset-dependent buttons initially
            SetAssetButtonsVisible(false);
        }

        Button MakeToolbarButton(string text, System.Action onClick)
        {
            var btn = new Button(onClick) { text = text };
            btn.style.height = 22;
            btn.style.fontSize = 11;
            btn.style.paddingLeft = 10;
            btn.style.paddingRight = 10;
            btn.style.marginLeft = 4;
            btn.style.backgroundColor = new Color(0.25f, 0.25f, 0.28f, 0.8f);
            btn.style.color = new Color(0.8f, 0.8f, 0.85f, 1f);
            btn.style.borderTopLeftRadius = 3; btn.style.borderTopRightRadius = 3;
            btn.style.borderBottomLeftRadius = 3; btn.style.borderBottomRightRadius = 3;
            btn.style.borderTopWidth = 1; btn.style.borderBottomWidth = 1;
            btn.style.borderLeftWidth = 1; btn.style.borderRightWidth = 1;
            btn.style.borderTopColor = new Color(0.35f, 0.35f, 0.38f, 0.3f);
            btn.style.borderBottomColor = new Color(0.35f, 0.35f, 0.38f, 0.3f);
            btn.style.borderLeftColor = new Color(0.35f, 0.35f, 0.38f, 0.3f);
            btn.style.borderRightColor = new Color(0.35f, 0.35f, 0.38f, 0.3f);
            return btn;
        }

        void BuildPanels(VisualElement root)
        {
            // Main horizontal split: left (blackboard+graph) | right (inspector)
            // Pane 1 (inspector) is fixed-width
            mainSplit = new TwoPaneSplitView(1, 280, TwoPaneSplitViewOrientation.Horizontal);
            mainSplit.style.flexGrow = 1;

            // Side vertical split: top (blackboard) | bottom (graph)
            // Pane 0 (blackboard) is fixed-height
            sideSplit = new TwoPaneSplitView(0, 220, TwoPaneSplitViewOrientation.Vertical);

            blackboardView = new BlackboardEditorView();
            blackboardView.OnChanged += OnDataChanged;
            sideSplit.Add(blackboardView);

            graphView = new StateMachineGraphView();
            graphView.OnStateSelected += d => inspectorView?.ShowState(d);
            graphView.OnTransitionSelected += t => inspectorView?.ShowTransition(t);
            graphView.OnGraphChanged += OnDataChanged;
            sideSplit.Add(graphView);

            mainSplit.Add(sideSplit);

            inspectorView = new StateInspectorView();
            inspectorView.OnChanged += OnDataChanged;
            inspectorView.style.borderLeftWidth = 1;
            inspectorView.style.borderLeftColor = new Color(0.22f, 0.22f, 0.23f, 1f);
            mainSplit.Add(inspectorView);

            root.Add(mainSplit);
        }

        // ====== Asset ======

        public void LoadAsset(SM.StateMachineAsset asset)
        {
            currentAsset = asset;
            graphView?.LoadAsset(asset);
            blackboardView?.LoadAsset(asset);
            inspectorView?.LoadAsset(asset);
            assetLabel.text = asset != null ? asset.name : "No asset loaded";
            SetAssetButtonsVisible(asset != null);
        }

        void SetAssetButtonsVisible(bool visible)
        {
            var d = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (addStateBtn != null) addStateBtn.style.display = d;
            if (addEntryBtn != null) addEntryBtn.style.display = d;
        }

        void OnDataChanged()
        {
            if (currentAsset != null)
            {
                EditorUtility.SetDirty(currentAsset);
                blackboardView?.Refresh();
                // Sync node titles from data
                if (graphView != null && currentAsset != null)
                {
                    foreach (var s in currentAsset.graphData.states)
                        graphView.GetNodeById(s.id)?.SetTitle(s.displayName);
                }
            }
        }

        void LoadUSS()
        {
            var ss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/StateMachineTool/Editor/StateMachineStyles.uss");
            if (ss != null) rootVisualElement.styleSheets.Add(ss);
        }
    }
}
