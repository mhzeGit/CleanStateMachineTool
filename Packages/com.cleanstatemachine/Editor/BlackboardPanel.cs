using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CleanStateMachine
{
    public class BlackboardPanel : VisualElement
    {
        private readonly CleanStateMachineWindow _window;
        private List<BlackboardVariable> _variables;
        private List<BlackboardEvent> _events;
        private readonly ScrollView _scrollView;
        private readonly VisualElement _variablesContainer;
        private readonly VisualElement _eventsContainer;
        private readonly Button _variablesTab;
        private readonly Button _eventsTab;
        private readonly Button _addButton;

        private readonly List<VisualElement> _variableRows = new();
        private readonly List<VisualElement> _eventRows = new();

        private int _editingIndex = -1;
        private int _selectedIndex = -1;
        private int _lastClickIndex = -1;
        private float _lastClickTime;
        private const float DoubleClickTime = 0.35f;
        private bool _isMouseOver;
        private bool _isEventsTab;
        private int _dragStartIndex = -1;
        private int _dragIndex = -1;
        private bool _isDragging;
        private Vector2 _dragMouseStartPos;
        private bool _dragPastThreshold;
        private const float DragThreshold = 5f;
        private const float AutoScrollEdgeThreshold = 20f;
        private const float AutoScrollSpeed = 30f;
        private const float RowHeight = 30f;

        public BlackboardPanel(CleanStateMachineWindow window)
        {
            _window = window;
            AddToClassList("blackboard-panel");

            var header = new VisualElement();
            header.AddToClassList("panel-header");
            header.style.flexDirection = FlexDirection.Column;
            header.style.height = StyleKeyword.Auto;
            header.style.alignItems = Align.Stretch;

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;

            var title = new Label("Blackboard");
            title.AddToClassList("panel-title");
            titleRow.Add(title);

            _addButton = new Button();
            _addButton.text = "+";
            _addButton.AddToClassList("add-button");
            _addButton.clicked += OnAddClicked;
            titleRow.Add(_addButton);

            header.Add(titleRow);

            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;
            tabRow.style.marginTop = 4;

            var tabBar = new VisualElement();
            tabBar.AddToClassList("blackboard-tab-bar");
            tabBar.style.flexGrow = 1;

            _variablesTab = new Button(() => SetActiveTab(false));
            _variablesTab.text = "Variables";
            _variablesTab.AddToClassList("blackboard-tab");
            _variablesTab.AddToClassList("blackboard-tab-active");

            _eventsTab = new Button(() => SetActiveTab(true));
            _eventsTab.text = "Events";
            _eventsTab.AddToClassList("blackboard-tab");

            tabBar.Add(_variablesTab);
            tabBar.Add(_eventsTab);
            tabRow.Add(tabBar);

            header.Add(tabRow);

            Add(header);

            _scrollView = new ScrollView();
            _scrollView.AddToClassList("blackboard-scroll");
            Add(_scrollView);

            _variablesContainer = new VisualElement();
            _eventsContainer = new VisualElement();
            _eventsContainer.style.display = DisplayStyle.None;
            _scrollView.Add(_variablesContainer);
            _scrollView.Add(_eventsContainer);

            focusable = true;
            RegisterCallback<MouseEnterEvent>(e => _isMouseOver = true);
            RegisterCallback<MouseLeaveEvent>(e => _isMouseOver = false);
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        public void UpdateVariables(List<BlackboardVariable> variables)
        {
            _variables = variables;
            RebuildVariables();
        }

        public void UpdateEvents(List<BlackboardEvent> events)
        {
            _events = events;
            RebuildEvents();
        }

        public void SelectVariable(int index)
        {
            if (index < 0 || _variableRows == null || index >= _variableRows.Count) return;
            ClearRowSelection();
            _selectedIndex = index;
            _variableRows[index].AddToClassList("variable-row-selected");
            _scrollView.ScrollTo(_variableRows[index]);
        }

        public void SelectEvent(int index)
        {
            if (index < 0 || _eventRows == null || index >= _eventRows.Count) return;
            ClearRowSelection();
            _selectedIndex = index;
            _eventRows[index].AddToClassList("variable-row-selected");
            _scrollView.ScrollTo(_eventRows[index]);
        }

        private void SetActiveTab(bool isEvents)
        {
            _isEventsTab = isEvents;
            _variablesTab.EnableInClassList("blackboard-tab-active", !isEvents);
            _eventsTab.EnableInClassList("blackboard-tab-active", isEvents);
            _variablesContainer.style.display = isEvents ? DisplayStyle.None : DisplayStyle.Flex;
            _eventsContainer.style.display = isEvents ? DisplayStyle.Flex : DisplayStyle.None;
            ClearRowSelection();
            _selectedIndex = -1;
            _editingIndex = -1;
        }

        private void RebuildVariables()
        {
            _variablesContainer.Clear();
            _variableRows.Clear();

            if (_variables == null) return;

            for (int i = 0; i < _variables.Count; i++)
            {
                var row = CreateVariableRow(_variables[i], i);
                _variablesContainer.Add(row);
                _variableRows.Add(row);
            }
        }

        private void RebuildEvents()
        {
            _eventsContainer.Clear();
            _eventRows.Clear();

            if (_events == null) return;

            for (int i = 0; i < _events.Count; i++)
            {
                var row = CreateEventRow(_events[i], i);
                _eventsContainer.Add(row);
                _eventRows.Add(row);
            }
        }

        private VisualElement CreateVariableRow(BlackboardVariable variable, int index)
        {
            var row = new VisualElement();
            row.AddToClassList("variable-row");
            row.userData = index;

            var handle = new DragHandle();
            handle.RegisterCallback<MouseDownEvent>(OnHandleDown);
            row.Add(handle);

            var nameContainer = new VisualElement();
            nameContainer.AddToClassList("variable-name");

            var nameLabel = new Label(variable.Name);
            nameLabel.AddToClassList("variable-name-label");

            var nameInput = new TextField();
            nameInput.AddToClassList("variable-name-input");
            nameInput.value = variable.Name;
            nameInput.style.display = DisplayStyle.None;

            nameInput.RegisterCallback<KeyDownEvent>(e =>
            {
                if (_editingIndex != index || _isEventsTab) return;
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNameEdit(index);
                    e.StopPropagation();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    CancelNameEdit(index);
                    e.StopPropagation();
                }
            });

            nameInput.RegisterCallback<FocusOutEvent>(e =>
            {
                if (_editingIndex == index && !_isEventsTab)
                    CommitNameEdit(index);
            });

            nameContainer.Add(nameLabel);
            nameContainer.Add(nameInput);
            row.Add(nameContainer);

            var rightGroup = new VisualElement();
            rightGroup.AddToClassList("variable-right");

            if (variable.Type == BlackboardVariableType.Trigger)
            {
                var toggleContainer = new VisualElement();
                toggleContainer.AddToClassList("variable-value-toggle");
                var triggerToggle = new TriggerToggle(variable.BoolValue);
                triggerToggle.RegisterCallback<ClickEvent>(_ =>
                {
                    string oldStr = variable.StringValue;
                    variable.BoolValue = triggerToggle.Value;
                    var cmd = new ModifyBlackboardVariableCommand(variable, oldStr, variable.StringValue);
                    _window.UndoRedoSystem.Execute(cmd);
                    _window.NotifySidePanelChanged();
                });
                toggleContainer.Add(triggerToggle);
                rightGroup.Add(toggleContainer);
            }
            else if (variable.Type == BlackboardVariableType.Bool)
            {
                var toggleContainer = new VisualElement();
                toggleContainer.AddToClassList("variable-value-toggle");
                var toggle = new Toggle();
                toggle.value = variable.BoolValue;
                toggle.RegisterValueChangedCallback(e =>
                {
                    var cmd = new ModifyBlackboardVariableCommand(
                        variable, e.previousValue.ToString(), e.newValue.ToString());
                    _window.UndoRedoSystem.Execute(cmd);
                    _window.NotifySidePanelChanged();
                });
                toggleContainer.Add(toggle);
                rightGroup.Add(toggleContainer);
            }
            else
            {
                switch (variable.Type)
                {
                    case BlackboardVariableType.Int:
                    {
                        var intField = new IntegerField();
                        intField.AddToClassList("variable-value");
                        intField.value = variable.IntValue;
                        intField.RegisterValueChangedCallback(e =>
                        {
                            var cmd = new ModifyBlackboardVariableCommand(
                                variable, e.previousValue.ToString(), e.newValue.ToString());
                            _window.UndoRedoSystem.Execute(cmd);
                            _window.NotifySidePanelChanged();
                        });
                        rightGroup.Add(intField);
                        break;
                    }
                    case BlackboardVariableType.Float:
                    {
                        var floatField = new FloatField();
                        floatField.AddToClassList("variable-value");
                        floatField.value = variable.FloatValue;
                        floatField.RegisterValueChangedCallback(e =>
                        {
                            var cmd = new ModifyBlackboardVariableCommand(
                                variable, e.previousValue.ToString("G"), e.newValue.ToString("G"));
                            _window.UndoRedoSystem.Execute(cmd);
                            _window.NotifySidePanelChanged();
                        });
                        rightGroup.Add(floatField);
                        break;
                    }
                    case BlackboardVariableType.String:
                    {
                        var valueField = new TextField();
                        valueField.AddToClassList("variable-value");
                        valueField.value = variable.StringValue;
                        valueField.RegisterValueChangedCallback(e =>
                        {
                            var cmd = new ModifyBlackboardVariableCommand(
                                variable, e.previousValue, e.newValue);
                            _window.UndoRedoSystem.Execute(cmd);
                            _window.NotifySidePanelChanged();
                        });
                        rightGroup.Add(valueField);
                        break;
                    }
                }
            }

            row.Add(rightGroup);

            row.RegisterCallback<ContextClickEvent>(e =>
            {
                int idx = (int)row.userData;
                MenuDropdown.Show(_window.rootVisualElement, _window.rootVisualElement.WorldToLocal(e.mousePosition), menu =>
                {
                    menu.AddItem("Delete Variable", new Color(0.85f, 0.2f, 0.2f), () =>
                    {
                        if (_variables == null || idx < 0 || idx >= _variables.Count) return;
                        var cmd = new DeleteBlackboardVariableCommand(_variables, idx);
                        _window.UndoRedoSystem.Execute(cmd);
                        RebuildVariables();
                    });
                });
                e.StopPropagation();
            });

            row.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    if (_editingIndex < 0)
                    {
                        float now = (float)EditorApplication.timeSinceStartup;
                        if (_lastClickIndex == index && (now - _lastClickTime) < DoubleClickTime)
                        {
                            _lastClickIndex = -1;
                            StartNameEdit(index);
                            e.StopPropagation();
                            return;
                        }
                        _lastClickIndex = index;
                        _lastClickTime = now;
                    }

                    ClearRowSelection();
                    row.AddToClassList("variable-row-selected");
                    _selectedIndex = index;
                    Focus();
                }
            });

            return row;
        }

        private VisualElement CreateEventRow(BlackboardEvent evt, int index)
        {
            var row = new VisualElement();
            row.AddToClassList("variable-row");
            row.userData = index;

            var handle = new DragHandle();
            handle.RegisterCallback<MouseDownEvent>(OnHandleDown);
            row.Add(handle);

            var nameContainer = new VisualElement();
            nameContainer.AddToClassList("variable-name");

            var nameLabel = new Label(evt.Name);
            nameLabel.AddToClassList("variable-name-label");

            var nameInput = new TextField();
            nameInput.AddToClassList("variable-name-input");
            nameInput.value = evt.Name;
            nameInput.style.display = DisplayStyle.None;

            nameInput.RegisterCallback<KeyDownEvent>(e =>
            {
                if (_editingIndex != index || !_isEventsTab) return;
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    CommitNameEdit(index);
                    e.StopPropagation();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    CancelNameEdit(index);
                    e.StopPropagation();
                }
            });

            nameInput.RegisterCallback<FocusOutEvent>(e =>
            {
                if (_editingIndex == index && _isEventsTab)
                    CommitNameEdit(index);
            });

            nameContainer.Add(nameLabel);
            nameContainer.Add(nameInput);
            row.Add(nameContainer);

            var rightGroup = new VisualElement();
            rightGroup.AddToClassList("variable-right");

            var badge = new Label("ArgEvent");
            badge.AddToClassList("variable-badge");
            rightGroup.Add(badge);

            row.Add(rightGroup);

            row.RegisterCallback<ContextClickEvent>(e =>
            {
                int idx = (int)row.userData;
                MenuDropdown.Show(_window.rootVisualElement, _window.rootVisualElement.WorldToLocal(e.mousePosition), menu =>
                {
                    menu.AddItem("Delete Event", new Color(0.85f, 0.2f, 0.2f), () =>
                    {
                        if (_events == null || idx < 0 || idx >= _events.Count) return;
                        var cmd = new DeleteBlackboardEventCommand(_events, idx);
                        _window.UndoRedoSystem.Execute(cmd);
                        RebuildEvents();
                    });
                });
                e.StopPropagation();
            });

            row.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == 0)
                {
                    if (_editingIndex < 0)
                    {
                        float now = (float)EditorApplication.timeSinceStartup;
                        if (_lastClickIndex == index && (now - _lastClickTime) < DoubleClickTime)
                        {
                            _lastClickIndex = -1;
                            StartNameEdit(index);
                            e.StopPropagation();
                            return;
                        }
                        _lastClickIndex = index;
                        _lastClickTime = now;
                    }

                    ClearRowSelection();
                    row.AddToClassList("variable-row-selected");
                    _selectedIndex = index;
                    Focus();
                }
            });

            return row;
        }

        private void ClearRowSelection()
        {
            foreach (var r in _variableRows)
                r.RemoveFromClassList("variable-row-selected");
            foreach (var r in _eventRows)
                r.RemoveFromClassList("variable-row-selected");
        }

        private void StartNameEdit(int index)
        {
            bool isEvents = _isEventsTab;
            var rows = isEvents ? _eventRows : _variableRows;
            var list = isEvents ? (_events as System.Collections.IList) : (_variables as System.Collections.IList);

            if (index < 0 || list == null || index >= list.Count)
                return;

            var row = rows[index];
            var nameLabel = row.Q<Label>(className: "variable-name-label");
            var nameInput = row.Q<TextField>(className: "variable-name-input");
            if (nameLabel == null || nameInput == null) return;

            _editingIndex = index;
            var item = list[index];
            string currentName = isEvents ? ((BlackboardEvent)item).Name : ((BlackboardVariable)item).Name;
            nameInput.value = currentName;
            nameLabel.style.display = DisplayStyle.None;
            nameInput.style.display = DisplayStyle.Flex;
            nameInput.schedule.Execute(() =>
            {
                nameInput.Focus();
                nameInput.SelectAll();
            }).StartingIn(0);
        }

        private void CommitNameEdit(int index)
        {
            if (_editingIndex != index) return;

            bool isEvents = _isEventsTab;
            var rows = isEvents ? _eventRows : _variableRows;
            var list = isEvents ? (_events as System.Collections.IList) : (_variables as System.Collections.IList);

            if (index < 0 || list == null || index >= list.Count)
                return;

            var row = rows[index];
            var nameLabel = row.Q<Label>(className: "variable-name-label");
            var nameInput = row.Q<TextField>(className: "variable-name-input");
            if (nameLabel == null || nameInput == null) return;

            string newName = nameInput.value;
            var item = list[index];

            if (isEvents)
            {
                var evt = (BlackboardEvent)item;
                if (!string.IsNullOrEmpty(newName) && newName != evt.Name)
                {
                    string oldName = evt.Name;
                    var cmd = new RenameBlackboardEventCommand(evt, oldName, newName);
                    _window.UndoRedoSystem.Execute(cmd);
                    _window.NotifySidePanelChanged();
                }
                nameLabel.text = evt.Name;
            }
            else
            {
                var variable = (BlackboardVariable)item;
                if (!string.IsNullOrEmpty(newName) && newName != variable.Name)
                {
                    string oldName = variable.Name;
                    var cmd = new RenameBlackboardVariableCommand(variable, oldName, newName);
                    _window.UndoRedoSystem.Execute(cmd);
                    _window.NotifySidePanelChanged();
                }
                nameLabel.text = variable.Name;
            }

            _editingIndex = -1;
            nameLabel.style.display = DisplayStyle.Flex;
            nameInput.style.display = DisplayStyle.None;
        }

        private void CancelNameEdit(int index)
        {
            if (_editingIndex != index) return;

            bool isEvents = _isEventsTab;
            var rows = isEvents ? _eventRows : _variableRows;
            var list = isEvents ? (_events as System.Collections.IList) : (_variables as System.Collections.IList);

            if (index < 0 || list == null || index >= list.Count)
                return;

            var row = rows[index];
            var nameLabel = row.Q<Label>(className: "variable-name-label");
            var nameInput = row.Q<TextField>(className: "variable-name-input");
            if (nameLabel == null || nameInput == null) return;

            _editingIndex = -1;
            nameLabel.style.display = DisplayStyle.Flex;
            nameInput.style.display = DisplayStyle.None;
        }

        private void OnHandleDown(MouseDownEvent evt)
        {
            int count = _isEventsTab ? (_events?.Count ?? 0) : (_variables?.Count ?? 0);
            if (count <= 1) return;

            var handle = evt.currentTarget as VisualElement;
            int index = (int)handle.parent.userData;
            ClearRowSelection();
            _selectedIndex = -1;
            _isDragging = true;
            _dragPastThreshold = false;
            _dragStartIndex = index;
            _dragIndex = index;
            _dragMouseStartPos = evt.mousePosition;
            var rows = _isEventsTab ? _eventRows : _variableRows;
            if (index >= 0 && index < rows.Count)
                rows[index].AddToClassList("variable-row-drag");
            this.RegisterCallback<MouseMoveEvent>(OnDragMove);
            this.RegisterCallback<MouseUpEvent>(OnDragUp);
            evt.StopPropagation();
        }

        private void OnDragMove(MouseMoveEvent evt)
        {
            if (!_isDragging) return;

            var list = _isEventsTab ? _events : (_variables as System.Collections.IList);
            if (list == null) return;

            if (!_dragPastThreshold)
            {
                if (Vector2.Distance(evt.mousePosition, _dragMouseStartPos) < DragThreshold)
                    return;
                _dragPastThreshold = true;
            }

            Vector2 scrollViewLocal = _scrollView.WorldToLocal(evt.mousePosition);
            float viewHeight = _scrollView.resolvedStyle.height;
            if (scrollViewLocal.y < AutoScrollEdgeThreshold)
                _scrollView.scrollOffset = new Vector2(0, Mathf.Max(0, _scrollView.scrollOffset.y - AutoScrollSpeed));
            else if (scrollViewLocal.y > viewHeight - AutoScrollEdgeThreshold)
                _scrollView.scrollOffset = new Vector2(0, _scrollView.scrollOffset.y + AutoScrollSpeed);

            Vector2 contentLocal = _scrollView.contentContainer.WorldToLocal(evt.mousePosition);
            int targetIndex = Mathf.Clamp(
                Mathf.FloorToInt(contentLocal.y / RowHeight),
                0, list.Count - 1);

            var rows = _isEventsTab ? _eventRows : _variableRows;
            var container = _isEventsTab ? _eventsContainer : _variablesContainer;

            if (targetIndex == _dragIndex) return;

            var row = rows[_dragIndex];
            container.Remove(row);

            if (targetIndex >= container.childCount)
                container.Add(row);
            else
                container.Insert(targetIndex, row);

            rows.RemoveAt(_dragIndex);
            rows.Insert(targetIndex, row);

            _dragIndex = targetIndex;
            evt.StopPropagation();
        }

        private void OnDragUp(MouseUpEvent evt)
        {
            _isDragging = false;
            _dragPastThreshold = false;
            this.UnregisterCallback<MouseMoveEvent>(OnDragMove);
            this.UnregisterCallback<MouseUpEvent>(OnDragUp);

            var rows = _isEventsTab ? _eventRows : _variableRows;
            if (_dragIndex >= 0 && _dragIndex < rows.Count)
                rows[_dragIndex].RemoveFromClassList("variable-row-drag");

            if (_dragStartIndex >= 0 && _dragStartIndex != _dragIndex)
            {
                if (_isEventsTab && _events != null)
                {
                    var item = _events[_dragStartIndex];
                    _events.RemoveAt(_dragStartIndex);
                    _events.Insert(_dragIndex, item);
                }
                else if (!_isEventsTab && _variables != null)
                {
                    var item = _variables[_dragStartIndex];
                    _variables.RemoveAt(_dragStartIndex);
                    _variables.Insert(_dragIndex, item);
                }

                for (int i = 0; i < rows.Count; i++)
                    rows[i].userData = i;

                _window.NotifySidePanelChanged();
            }

            _dragStartIndex = -1;
            _dragIndex = -1;
            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.S && e.ctrlKey)
            {
                _window.OnSaveCommand();
                e.StopPropagation();
                return;
            }

            if (e.keyCode == KeyCode.F2)
            {
                if (_editingIndex < 0 && _selectedIndex >= 0)
                {
                    StartNameEdit(_selectedIndex);
                    e.StopPropagation();
                    return;
                }
            }

            if (e.keyCode != KeyCode.Delete && e.keyCode != KeyCode.Backspace) return;
            if (!_isMouseOver) return;
            if (_editingIndex >= 0) return;

            var focused = focusController?.focusedElement as VisualElement;
            if (focused != null && focused != this && this.Contains(focused))
                return;

            if (_selectedIndex < 0) return;

            if (_isEventsTab)
            {
                if (_events == null || _selectedIndex >= _events.Count) return;
                var cmd = new DeleteBlackboardEventCommand(_events, _selectedIndex);
                _window.UndoRedoSystem.Execute(cmd);
            }
            else
            {
                if (_variables == null || _selectedIndex >= _variables.Count) return;
                var cmd = new DeleteBlackboardVariableCommand(_variables, _selectedIndex);
                _window.UndoRedoSystem.Execute(cmd);
            }

            _selectedIndex = -1;
            if (_isEventsTab)
                RebuildEvents();
            else
                RebuildVariables();
            e.StopPropagation();
        }

        private void OnAddClicked()
        {
            if (_isEventsTab)
                AddEvent();
            else
                ShowAddVariableMenu();
        }

        private void ShowAddVariableMenu()
        {
            var pos = _window.rootVisualElement.WorldToLocal(
                new Vector2(_addButton.worldBound.x, _addButton.worldBound.y + _addButton.worldBound.height));
            MenuDropdown.Show(_window.rootVisualElement, pos, menu =>
            {
                foreach (BlackboardVariableType type in System.Enum.GetValues(typeof(BlackboardVariableType)))
                {
                    BlackboardVariableType capturedType = type;
                    string label = ObjectNames.NicifyVariableName(type.ToString());
                    menu.AddItem(label, () => AddVariable(capturedType));
                }
            });
        }

        private void AddVariable(BlackboardVariableType type)
        {
            if (_variables == null) return;

            var v = new BlackboardVariable
            {
                Name = GetUniqueVariableName("New Variable"),
                Type = type,
                StringValue = type switch
                {
                    BlackboardVariableType.Bool => "False",
                    BlackboardVariableType.String => "",
                    BlackboardVariableType.Trigger => "False",
                    _ => "0"
                }
            };
            _variables.Add(v);
            _window.NotifySidePanelChanged();
            RebuildVariables();

            if (_variableRows.Count > 0)
                _scrollView.scrollOffset = new Vector2(0, float.MaxValue);
        }

        private void AddEvent()
        {
            if (_events == null) return;

            var e = new BlackboardEvent
            {
                Name = GetUniqueEventName("New Event")
            };
            _events.Add(e);
            _window.NotifySidePanelChanged();
            RebuildEvents();

            if (_eventRows.Count > 0)
                _scrollView.scrollOffset = new Vector2(0, float.MaxValue);
        }

        private string GetUniqueVariableName(string baseName)
        {
            if (_variables == null) return baseName;
            if (!_variables.Exists(x => x.Name == baseName))
                return baseName;

            for (int i = 1; i < 1000; i++)
            {
                string candidate = $"{baseName} {i}";
                if (!_variables.Exists(x => x.Name == candidate))
                    return candidate;
            }
            return baseName;
        }

        private string GetUniqueEventName(string baseName)
        {
            if (_events == null) return baseName;
            if (!_events.Exists(x => x.Name == baseName))
                return baseName;

            for (int i = 1; i < 1000; i++)
            {
                string candidate = $"{baseName} {i}";
                if (!_events.Exists(x => x.Name == candidate))
                    return candidate;
            }
            return baseName;
        }
    }
}
