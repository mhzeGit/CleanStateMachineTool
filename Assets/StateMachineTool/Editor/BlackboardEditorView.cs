using System;
using System.Collections.Generic;
using System.Linq;
using StateMachineTool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace StateMachineTool.Editor
{
    public class BlackboardEditorView : VisualElement
    {
        private StateMachineAsset asset;
        private ListView variableListView;
        private ListView eventListView;
        private VisualElement variableEditPanel;
        private VisualElement eventEditPanel;

        private BlackboardVariable selectedVariable;
        private BlackboardEvent selectedEvent;

        public System.Action OnChanged;

        public BlackboardEditorView()
        {
            style.backgroundColor = new UnityEngine.Color(0.15f, 0.15f, 0.16f, 1f);
            style.paddingBottom = 4;

            BuildUI();
        }

        public void LoadAsset(StateMachineAsset targetAsset)
        {
            asset = targetAsset;
            if (asset == null) return;
            Refresh();
        }

        // ====== Build ======

        private void BuildUI()
        {
            var header = new Label("BLACKBOARD");
            header.style.backgroundColor = new UnityEngine.Color(0.11f, 0.11f, 0.12f, 1f);
            header.style.color = new UnityEngine.Color(0.78f, 0.78f, 0.78f, 1f);
            header.style.fontSize = 12;
            header.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            header.style.paddingLeft = 12;
            header.style.paddingRight = 12;
            header.style.paddingTop = 6;
            header.style.paddingBottom = 6;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new UnityEngine.Color(0.2f, 0.2f, 0.21f, 1f);
            Add(header);

            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;

            // ==== Variables ====
            var varSectionLabel = new Label("Variables");
            varSectionLabel.style.color = new UnityEngine.Color(0.6f, 0.6f, 0.65f, 1f);
            varSectionLabel.style.fontSize = 11;
            varSectionLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            varSectionLabel.style.paddingLeft = 12;
            varSectionLabel.style.paddingRight = 12;
            varSectionLabel.style.paddingTop = 8;
            varSectionLabel.style.paddingBottom = 2;
            scrollView.Add(varSectionLabel);

            var addVarBtn = new Button(() => AddVariable()) { text = "+ Add Variable" };
            addVarBtn.style.fontSize = 10;
            addVarBtn.style.height = 20;
            addVarBtn.style.marginLeft = 12;
            addVarBtn.style.marginRight = 12;
            addVarBtn.style.marginBottom = 4;
            addVarBtn.style.backgroundColor = new UnityEngine.Color(0.25f, 0.25f, 0.28f, 0.6f);
            addVarBtn.style.color = new UnityEngine.Color(0.7f, 0.78f, 0.85f, 1f);
            addVarBtn.style.borderTopLeftRadius = 3; addVarBtn.style.borderTopRightRadius = 3;
            addVarBtn.style.borderBottomLeftRadius = 3; addVarBtn.style.borderBottomRightRadius = 3;
            scrollView.Add(addVarBtn);

            variableListView = MakeListView();
            variableListView.selectionChanged += OnVariableSelectionChanged;
            var varListContainer = new VisualElement();
            varListContainer.style.marginLeft = 8;
            varListContainer.style.marginRight = 8;
            varListContainer.Add(variableListView);
            scrollView.Add(varListContainer);

            variableEditPanel = new VisualElement();
            variableEditPanel.style.display = DisplayStyle.None;
            variableEditPanel.style.marginLeft = 8;
            variableEditPanel.style.marginRight = 8;
            variableEditPanel.style.marginTop = 4;
            variableEditPanel.style.marginBottom = 4;
            variableEditPanel.style.paddingLeft = 8;
            variableEditPanel.style.paddingRight = 8;
            variableEditPanel.style.paddingTop = 6;
            variableEditPanel.style.paddingBottom = 6;
            variableEditPanel.style.backgroundColor = new UnityEngine.Color(0.18f, 0.18f, 0.2f, 1f);
            variableEditPanel.style.borderTopLeftRadius = 4; variableEditPanel.style.borderTopRightRadius = 4;
            variableEditPanel.style.borderBottomLeftRadius = 4; variableEditPanel.style.borderBottomRightRadius = 4;
            scrollView.Add(variableEditPanel);

            // ==== Events ====
            var evtSectionLabel = new Label("Events");
            evtSectionLabel.style.color = new UnityEngine.Color(0.6f, 0.6f, 0.65f, 1f);
            evtSectionLabel.style.fontSize = 11;
            evtSectionLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            evtSectionLabel.style.paddingLeft = 12;
            evtSectionLabel.style.paddingRight = 12;
            evtSectionLabel.style.paddingTop = 12;
            evtSectionLabel.style.paddingBottom = 2;
            scrollView.Add(evtSectionLabel);

            var addEvtBtn = new Button(() => AddEvent()) { text = "+ Add Event" };
            addEvtBtn.style.fontSize = 10;
            addEvtBtn.style.height = 20;
            addEvtBtn.style.marginLeft = 12;
            addEvtBtn.style.marginRight = 12;
            addEvtBtn.style.marginBottom = 4;
            addEvtBtn.style.backgroundColor = new UnityEngine.Color(0.25f, 0.25f, 0.28f, 0.6f);
            addEvtBtn.style.color = new UnityEngine.Color(0.7f, 0.78f, 0.85f, 1f);
            addEvtBtn.style.borderTopLeftRadius = 3; addEvtBtn.style.borderTopRightRadius = 3;
            addEvtBtn.style.borderBottomLeftRadius = 3; addEvtBtn.style.borderBottomRightRadius = 3;
            scrollView.Add(addEvtBtn);

            eventListView = MakeListView();
            eventListView.selectionChanged += OnEventSelectionChanged;
            var evtListContainer = new VisualElement();
            evtListContainer.style.marginLeft = 8;
            evtListContainer.style.marginRight = 8;
            evtListContainer.Add(eventListView);
            scrollView.Add(evtListContainer);

            eventEditPanel = new VisualElement();
            eventEditPanel.style.display = DisplayStyle.None;
            eventEditPanel.style.marginLeft = 8;
            eventEditPanel.style.marginRight = 8;
            eventEditPanel.style.marginTop = 4;
            eventEditPanel.style.marginBottom = 4;
            eventEditPanel.style.paddingLeft = 8;
            eventEditPanel.style.paddingRight = 8;
            eventEditPanel.style.paddingTop = 6;
            eventEditPanel.style.paddingBottom = 6;
            eventEditPanel.style.backgroundColor = new UnityEngine.Color(0.18f, 0.18f, 0.2f, 1f);
            eventEditPanel.style.borderTopLeftRadius = 4; eventEditPanel.style.borderTopRightRadius = 4;
            eventEditPanel.style.borderBottomLeftRadius = 4; eventEditPanel.style.borderBottomRightRadius = 4;
            scrollView.Add(eventEditPanel);

            Add(scrollView);
        }

        private ListView MakeListView()
        {
            var lv = new ListView();
            lv.fixedItemHeight = 22;
            lv.selectionType = SelectionType.Single;
            lv.makeItem = () =>
            {
                var label = new Label();
                label.style.fontSize = 11;
                label.style.color = new UnityEngine.Color(0.78f, 0.78f, 0.82f, 1f);
                label.style.paddingLeft = 8;
                label.style.paddingTop = 2;
                label.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
                return label;
            };
            return lv;
        }

        // ====== Refresh ======

        public void Refresh()
        {
            if (asset == null) return;

            var vars = asset.graphData.blackboard.variables;
            variableListView.itemsSource = vars;
            variableListView.bindItem = (elem, i) =>
            {
                if (i < 0 || i >= vars.Count) return;
                var v = vars[i];
                ((Label)elem).text = $"{v.key} ({v.type})";
            };
            variableListView.RefreshItems();

            var evts = asset.graphData.blackboard.events;
            eventListView.itemsSource = evts;
            eventListView.bindItem = (elem, i) =>
            {
                if (i < 0 || i >= evts.Count) return;
                var e = evts[i];
                ((Label)elem).text = e.key;
            };
            eventListView.RefreshItems();
        }

        // ====== Add ======

        private void AddVariable()
        {
            if (asset == null) return;
            Undo.RecordObject(asset, "Add Variable");
            var v = new BlackboardVariable
            {
                key = $"var{asset.graphData.blackboard.variables.Count}",
                type = BlackboardValueType.Bool
            };
            asset.graphData.blackboard.variables.Add(v);
            EditorUtility.SetDirty(asset);
            Refresh();
            variableListView.selectedIndex = asset.graphData.blackboard.variables.Count - 1;
            OnChanged?.Invoke();
        }

        private void AddEvent()
        {
            if (asset == null) return;
            Undo.RecordObject(asset, "Add Event");
            var evt = new BlackboardEvent
            {
                key = $"event{asset.graphData.blackboard.events.Count}",
                displayName = $"Event {asset.graphData.blackboard.events.Count}"
            };
            asset.graphData.blackboard.events.Add(evt);
            EditorUtility.SetDirty(asset);
            Refresh();
            eventListView.selectedIndex = asset.graphData.blackboard.events.Count - 1;
            OnChanged?.Invoke();
        }

        // ====== Selection ======

        private void OnVariableSelectionChanged(IEnumerable<object> sel)
        {
            selectedVariable = sel?.FirstOrDefault() as BlackboardVariable;
            selectedEvent = null;
            eventListView?.SetSelectionWithoutNotify(new int[0]);
            BuildVariableEditor();
        }

        private void OnEventSelectionChanged(IEnumerable<object> sel)
        {
            selectedEvent = sel?.FirstOrDefault() as BlackboardEvent;
            selectedVariable = null;
            variableListView?.SetSelectionWithoutNotify(new int[0]);
            BuildEventEditor();
        }

        private void BuildVariableEditor()
        {
            variableEditPanel.Clear();
            if (selectedVariable == null) { variableEditPanel.style.display = DisplayStyle.None; return; }
            variableEditPanel.style.display = DisplayStyle.Flex;

            var title = new Label($"Variable: {selectedVariable.key}");
            title.style.fontSize = 12;
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.color = new UnityEngine.Color(0.75f, 0.75f, 0.82f, 1f);
            title.style.paddingBottom = 4;
            title.style.marginBottom = 4;
            title.style.borderBottomWidth = 1;
            title.style.borderBottomColor = new UnityEngine.Color(0.25f, 0.25f, 0.28f, 1f);
            variableEditPanel.Add(title);

            var keyField = new TextField("Key") { value = selectedVariable.key };
            keyField.RegisterValueChangedCallback(evt =>
            {
                selectedVariable.key = evt.newValue;
                EditorUtility.SetDirty(asset);
                Refresh();
            });
            variableEditPanel.Add(keyField);

            var typeField = new EnumField("Type", selectedVariable.type);
            typeField.RegisterValueChangedCallback(evt =>
            {
                selectedVariable.type = (BlackboardValueType)evt.newValue;
                EditorUtility.SetDirty(asset);
                Refresh();
            });
            variableEditPanel.Add(typeField);

            BuildValueField(selectedVariable);

            var deleteBtn = new Button(() =>
            {
                asset.graphData.blackboard.variables.Remove(selectedVariable);
                selectedVariable = null;
                variableEditPanel.style.display = DisplayStyle.None;
                EditorUtility.SetDirty(asset);
                Refresh();
                OnChanged?.Invoke();
            }) { text = "Delete" };
            deleteBtn.style.marginTop = 6;
            deleteBtn.style.height = 22;
            deleteBtn.style.fontSize = 10;
            deleteBtn.style.color = new UnityEngine.Color(0.92f, 0.55f, 0.55f, 1f);
            deleteBtn.style.backgroundColor = new UnityEngine.Color(0.35f, 0.1f, 0.1f, 0.5f);
            deleteBtn.style.borderTopLeftRadius = 3; deleteBtn.style.borderTopRightRadius = 3;
            deleteBtn.style.borderBottomLeftRadius = 3; deleteBtn.style.borderBottomRightRadius = 3;
            variableEditPanel.Add(deleteBtn);
        }

        private void BuildValueField(BlackboardVariable variable)
        {
            switch (variable.type)
            {
                case BlackboardValueType.Int:
                    var intField = new IntegerField("Default") { value = variable.intValue };
                    intField.RegisterValueChangedCallback(evt => { variable.intValue = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(intField);
                    break;
                case BlackboardValueType.Float:
                    var floatField = new FloatField("Default") { value = variable.floatValue };
                    floatField.RegisterValueChangedCallback(evt => { variable.floatValue = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(floatField);
                    break;
                case BlackboardValueType.Bool:
                    var toggle = new Toggle("Default") { value = variable.boolValue };
                    toggle.RegisterValueChangedCallback(evt => { variable.boolValue = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(toggle);
                    break;
                case BlackboardValueType.String:
                    var strField = new TextField("Default") { value = variable.stringValue };
                    strField.RegisterValueChangedCallback(evt => { variable.stringValue = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(strField);
                    break;
                case BlackboardValueType.Vector2:
                    var v2Field = new Vector2Field("Default") { value = variable.vector2Value };
                    v2Field.RegisterValueChangedCallback(evt => { variable.vector2Value = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(v2Field);
                    break;
                case BlackboardValueType.Vector3:
                    var v3Field = new Vector3Field("Default") { value = variable.vector3Value };
                    v3Field.RegisterValueChangedCallback(evt => { variable.vector3Value = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(v3Field);
                    break;
                case BlackboardValueType.Object:
                    var objField = new ObjectField("Default") { objectType = typeof(UnityEngine.Object), value = variable.objectValue };
                    objField.RegisterValueChangedCallback(evt => { variable.objectValue = evt.newValue; EditorUtility.SetDirty(asset); });
                    variableEditPanel.Add(objField);
                    break;
            }
        }

        private void BuildEventEditor()
        {
            eventEditPanel.Clear();
            if (selectedEvent == null) { eventEditPanel.style.display = DisplayStyle.None; return; }
            eventEditPanel.style.display = DisplayStyle.Flex;

            var title = new Label($"Event: {selectedEvent.key}");
            title.style.fontSize = 12;
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.color = new UnityEngine.Color(0.75f, 0.75f, 0.82f, 1f);
            title.style.paddingBottom = 4;
            title.style.marginBottom = 4;
            title.style.borderBottomWidth = 1;
            title.style.borderBottomColor = new UnityEngine.Color(0.25f, 0.25f, 0.28f, 1f);
            eventEditPanel.Add(title);

            var keyField = new TextField("Key") { value = selectedEvent.key };
            keyField.RegisterValueChangedCallback(evt =>
            {
                selectedEvent.key = evt.newValue;
                EditorUtility.SetDirty(asset);
                Refresh();
            });
            eventEditPanel.Add(keyField);

            var nameField = new TextField("Display Name") { value = selectedEvent.displayName };
            nameField.RegisterValueChangedCallback(evt =>
            {
                selectedEvent.displayName = evt.newValue;
                EditorUtility.SetDirty(asset);
                Refresh();
            });
            eventEditPanel.Add(nameField);

            var deleteBtn = new Button(() =>
            {
                asset.graphData.blackboard.events.Remove(selectedEvent);
                selectedEvent = null;
                eventEditPanel.style.display = DisplayStyle.None;
                EditorUtility.SetDirty(asset);
                Refresh();
                OnChanged?.Invoke();
            }) { text = "Delete" };
            deleteBtn.style.marginTop = 6;
            deleteBtn.style.height = 22;
            deleteBtn.style.fontSize = 10;
            deleteBtn.style.color = new UnityEngine.Color(0.92f, 0.55f, 0.55f, 1f);
            deleteBtn.style.backgroundColor = new UnityEngine.Color(0.35f, 0.1f, 0.1f, 0.5f);
            deleteBtn.style.borderTopLeftRadius = 3; deleteBtn.style.borderTopRightRadius = 3;
            deleteBtn.style.borderBottomLeftRadius = 3; deleteBtn.style.borderBottomRightRadius = 3;
            eventEditPanel.Add(deleteBtn);
        }
    }
}
