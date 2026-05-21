using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public class StateClassEditor
    {
        private readonly HashSet<string> _expandedSections = new HashSet<string>();
        private readonly HashSet<int> _expandedEvents = new HashSet<int>();
        private int _nextEventId = 1;

        public event Action Changed;

        public StateClassEditor()
        {
            _expandedSections.Add("OnStateEnter");
        }

        public void Draw(Rect rect, StateClassData stateClass, ref Vector2 scrollPos)
        {
            if (stateClass == null) return;

            EnsureEventIds(stateClass);

            float totalHeight = ComputeTotalHeight(stateClass);
            Rect viewRect = new Rect(0f, 0f, rect.width - 14f, Mathf.Max(totalHeight, rect.height));
            scrollPos = GUI.BeginScrollView(rect, scrollPos, viewRect);

            float y = 0f;
            float w = viewRect.width;

            for (int i = 0; i < stateClass.Sections.Count; i++)
            {
                var section = stateClass.Sections[i];
                DrawSection(ref y, w, section, i);
                y += 6f;
            }

            if (stateClass.Sections.Count == 0)
            {
                Rect emptyRect = new Rect(0f, y, w, UITheme.RowHeight * 2f);
                GUI.Label(emptyRect, "No sections defined", UITheme.EmptyStyle);
            }

            GUI.EndScrollView();
        }

        private void DrawSection(ref float y, float width,
            StateSectionData section, int sectionIndex)
        {
            bool isExpanded = _expandedSections.Contains(section.SectionName);

            // --- Section foldout header ---
            DrawSectionFoldoutHeader(ref y, width, section.SectionName, isExpanded);

            if (!isExpanded)
            {
                UITheme.DrawSectionDivider(y, width);
                y += 6f;
                return;
            }

            // --- Add event button ---
            DrawAddEventButton(ref y, width, section, sectionIndex);

            if (section.Events.Count == 0)
            {
                Rect emptyRect = new Rect(0f, y, width, UITheme.RowHeight);
                var emptyStyle = new GUIStyle(UITheme.SecondaryStyle)
                {
                    normal = { textColor = UITheme.TextMuted },
                    fontStyle = FontStyle.Italic,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(emptyRect, "No events — click + to add", emptyStyle);
                y += UITheme.RowHeight;
            }
            else
            {
                for (int i = 0; i < section.Events.Count; i++)
                {
                    var evt = section.Events[i];
                    int eventId = evt.EditorId;
                    bool eventExpanded = _expandedEvents.Contains(eventId);
                    DrawEventRow(ref y, width, section, i, evt, eventId, eventExpanded);
                }
            }

            y += 4f;
            UITheme.DrawSectionDivider(y, width);
            y += 6f;
        }

        private void DrawSectionFoldoutHeader(ref float y, float width, string sectionName, bool isExpanded)
        {
            Rect headerRect = new Rect(0f, y, width, UITheme.RowHeight);
            EditorGUI.DrawRect(headerRect, UITheme.RowEven);

            string label = (isExpanded ? "▼ " : "▶ ") + sectionName;
            Rect foldoutRect = new Rect(8f, headerRect.y, headerRect.width - 16f, headerRect.height);

            if (GUI.Button(foldoutRect, label, UITheme.FoldoutHeaderStyle))
            {
                if (isExpanded)
                    _expandedSections.Remove(sectionName);
                else
                    _expandedSections.Add(sectionName);
            }

            y += UITheme.RowHeight;
        }

        private void DrawAddEventButton(ref float y, float width, StateSectionData section, int sectionIndex)
        {
            Rect addRect = new Rect(0f, y, width, UITheme.RowHeight);
            EditorGUI.DrawRect(addRect, UITheme.RowOdd);

            Rect btnRect = new Rect(8f, addRect.y + 4f, 24f, addRect.height - 8f);
            bool btnHover = btnRect.Contains(Event.current.mousePosition);
            UITheme.DrawSmallButton(btnRect, btnHover);

            var btnStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.TextColor }
            };

            if (GUI.Button(btnRect, "+", btnStyle))
            {
                ShowAddEventMenu(section, btnRect);
            }

            Rect labelRect = new Rect(btnRect.xMax + 8f, addRect.y, width - btnRect.xMax - 16f, addRect.height);
            GUI.Label(labelRect, "Add Event", new GUIStyle(UITheme.SecondaryStyle)
            {
                normal = { textColor = UITheme.TextSecondary },
                fontSize = 11,
                fontStyle = FontStyle.Bold
            });

            y += UITheme.RowHeight;
        }

        private void ShowAddEventMenu(StateSectionData section, Rect buttonRect)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Debug Log"), false,
                () => { var evt = CreateEvent(StateMachineEventType.DebugLog); AssignEventId(evt); section.Events.Add(evt); Changed?.Invoke(); });
            menu.AddItem(new GUIContent("Wait"), false,
                () => { var evt = CreateEvent(StateMachineEventType.Wait); AssignEventId(evt); section.Events.Add(evt); Changed?.Invoke(); });
            menu.AddItem(new GUIContent("Unity Event"), false,
                () => { var evt = CreateEvent(StateMachineEventType.UnityEvent); AssignEventId(evt); section.Events.Add(evt); Changed?.Invoke(); });
            menu.AddItem(new GUIContent("Custom"), false,
                () => { var evt = CreateEvent(StateMachineEventType.Custom); AssignEventId(evt); section.Events.Add(evt); Changed?.Invoke(); });
            menu.DropDown(buttonRect);
        }

        private static StateMachineEventData CreateEvent(StateMachineEventType type)
        {
            var evt = new StateMachineEventData
            {
                Type = type,
                DebugMessage = "",
                WaitDuration = 0.5f,
                UnityEventCallbacks = new List<UnityEventCallbackData>(),
                CustomText = ""
            };
            return evt;
        }

        private int AssignEventId(StateMachineEventData evt)
        {
            evt.EditorId = _nextEventId++;
            return evt.EditorId;
        }

        private void EnsureEventIds(StateClassData stateClass)
        {
            for (int i = 0; i < stateClass.Sections.Count; i++)
            {
                for (int j = 0; j < stateClass.Sections[i].Events.Count; j++)
                {
                    var evt = stateClass.Sections[i].Events[j];
                    if (evt.EditorId == 0)
                        evt.EditorId = _nextEventId++;
                }
            }
        }

        private void DrawEventRow(ref float y, float width, StateSectionData section,
            int index, StateMachineEventData evt, int eventId, bool isExpanded)
        {
            float rowHeight = isExpanded ? GetExpandedEventHeight(evt) : UITheme.RowHeight;
            float rowW = width;

            Rect rowRect = new Rect(0f, y, rowW, rowHeight);
            EditorGUI.DrawRect(rowRect, index % 2 == 0 ? UITheme.RowEven : UITheme.RowOdd);

            // Event type badge
            string typeBadge = GetEventTypeBadge(evt.Type);
            Rect badgeRect = new Rect(8f, rowRect.y + 6f, 60f, UITheme.RowHeight - 12f);
            EditorGUI.DrawRect(badgeRect, UITheme.TypeBadgeBg);
            var badgeStyle = new GUIStyle(UITheme.TypeBadgeStyle) { fontSize = 8 };
            GUI.Label(badgeRect, typeBadge, badgeStyle);

            // Summary / expand button
            Rect expandRect = new Rect(76f, rowRect.y, rowW - 100f, UITheme.RowHeight);
            string arrow = isExpanded ? " ▼" : " ▶";
            var summaryStyle = new GUIStyle(UITheme.SecondaryStyle)
            {
                clipping = TextClipping.Clip,
                padding = new RectOffset(4, 0, 0, 0)
            };

            if (GUI.Button(expandRect, arrow + GetEventLabel(evt), summaryStyle))
            {
                if (isExpanded)
                    _expandedEvents.Remove(eventId);
                else
                    _expandedEvents.Add(eventId);
            }

            // Delete button
            Rect deleteRect = new Rect(rowW - 24f, rowRect.y + 6f, 16f, UITheme.RowHeight - 12f);
            if (GUI.Button(deleteRect, "✕", UITheme.DeleteButtonStyle))
            {
                section.Events.RemoveAt(index);
                _expandedEvents.Remove(eventId);
                Changed?.Invoke();
            }

            y += UITheme.RowHeight;

            if (isExpanded)
            {
                DrawEventFields(ref y, width, 16f, evt);
            }
        }

        private void DrawEventFields(ref float y, float width, float leftMargin, StateMachineEventData evt)
        {
            float fieldWidth = width - leftMargin;

            switch (evt.Type)
            {
                case StateMachineEventType.DebugLog:
                    DrawTextField(ref y, leftMargin, fieldWidth, "Message", ref evt.DebugMessage);
                    y += 4f;
                    break;

                case StateMachineEventType.Wait:
                    DrawFloatField(ref y, leftMargin, fieldWidth, "Duration (s)", ref evt.WaitDuration);
                    y += 4f;
                    break;

                case StateMachineEventType.UnityEvent:
                    DrawUnityEventFields(ref y, width, leftMargin, fieldWidth, evt);
                    y += 4f;
                    break;

                case StateMachineEventType.Custom:
                    DrawTextField(ref y, leftMargin, fieldWidth, "Custom Text", ref evt.CustomText);
                    y += 4f;
                    break;
            }
        }

        private void DrawTextField(ref float y, float margin, float fieldWidth,
            string label, ref string value)
        {
            Rect labelRect = new Rect(margin, y, 76f, UITheme.RowHeight);
            GUI.Label(labelRect, label, UITheme.LabelStyle);

            Rect fieldRect = new Rect(margin + 80f, y + 4f, fieldWidth - 88f, UITheme.RowHeight - 8f);
            EditorGUI.DrawRect(fieldRect, UITheme.FieldBg);
            string newValue = GUI.TextField(fieldRect, value ?? "", UITheme.RowFieldStyle);
            if (newValue != value)
            {
                value = newValue;
                Changed?.Invoke();
            }
            y += UITheme.RowHeight;
        }

        private void DrawFloatField(ref float y, float margin, float fieldWidth,
            string label, ref float value)
        {
            Rect labelRect = new Rect(margin, y, 76f, UITheme.RowHeight);
            GUI.Label(labelRect, label, UITheme.LabelStyle);

            Rect fieldRect = new Rect(margin + 80f, y + 4f, fieldWidth - 88f, UITheme.RowHeight - 8f);
            EditorGUI.DrawRect(fieldRect, UITheme.FieldBg);
            float newValue = EditorGUI.FloatField(fieldRect, value);
            if (!Mathf.Approximately(newValue, value))
            {
                value = newValue;
                Changed?.Invoke();
            }
            y += UITheme.RowHeight;
        }

        private void DrawUnityEventFields(ref float y, float width, float margin, float fieldWidth,
            StateMachineEventData evt)
        {
            if (evt.UnityEventCallbacks == null)
                evt.UnityEventCallbacks = new List<UnityEventCallbackData>();

            Rect addRect = new Rect(margin, y, fieldWidth, UITheme.RowHeight);
            Rect addBtnRect = new Rect(addRect.x, addRect.y + 4f, 24f, addRect.height - 8f);
            bool btnHover = addBtnRect.Contains(Event.current.mousePosition);
            UITheme.DrawSmallButton(addBtnRect, btnHover);

            var btnStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = UITheme.TextColor }
            };

            if (GUI.Button(addBtnRect, "+", btnStyle))
            {
                evt.UnityEventCallbacks.Add(new UnityEventCallbackData());
                Changed?.Invoke();
            }

            GUI.Label(new Rect(addBtnRect.xMax + 8f, addRect.y, addRect.width - 32f, addRect.height),
                "Callbacks", new GUIStyle(UITheme.SecondaryStyle)
                {
                    normal = { textColor = UITheme.TextSecondary },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                });
            y += UITheme.RowHeight;

            for (int i = 0; i < evt.UnityEventCallbacks.Count; i++)
            {
                var cb = evt.UnityEventCallbacks[i];
                DrawUnityCallbackRow(ref y, width, margin,
                    fieldWidth, evt.UnityEventCallbacks, i, cb);
            }
        }

        private void DrawUnityCallbackRow(ref float y, float width, float indent, float fieldWidth,
            List<UnityEventCallbackData> callbacks, int index, UnityEventCallbackData cb)
        {
            float rowX = indent;
            float rowW = fieldWidth - 8f;
            Rect bgRect = new Rect(rowX, y, rowW, UITheme.RowHeight);
            EditorGUI.DrawRect(bgRect, index % 2 == 0 ? UITheme.RowEven : UITheme.RowOdd);

            Rect targetRect = new Rect(rowX + 4f, y + 4f, rowW * 0.44f - 4f, UITheme.RowHeight - 8f);
            var newTarget = EditorGUI.ObjectField(targetRect, cb.Target, typeof(UnityEngine.Object), true);
            if (newTarget != cb.Target)
            {
                cb.Target = newTarget;
                Changed?.Invoke();
            }

            Rect methodRect = new Rect(rowX + rowW * 0.44f + 4f, y + 4f, rowW * 0.40f - 4f, UITheme.RowHeight - 8f);
            EditorGUI.DrawRect(methodRect, UITheme.FieldBg);
            string newMethod = GUI.TextField(methodRect, cb.MethodName ?? "", UITheme.RowFieldStyle);
            if (newMethod != cb.MethodName)
            {
                cb.MethodName = newMethod;
                Changed?.Invoke();
            }

            Rect delRect = new Rect(rowX + rowW - 20f, y + 4f, 16f, UITheme.RowHeight - 8f);
            if (GUI.Button(delRect, "✕", UITheme.DeleteButtonStyle))
            {
                callbacks.RemoveAt(index);
                Changed?.Invoke();
            }

            y += UITheme.RowHeight;
        }

        private static string GetEventTypeBadge(StateMachineEventType type)
        {
            return type switch
            {
                StateMachineEventType.DebugLog => "LOG",
                StateMachineEventType.Wait => "WAIT",
                StateMachineEventType.UnityEvent => "UNITY",
                StateMachineEventType.Custom => "CUSTOM",
                _ => "?"
            };
        }

        private static string GetEventLabel(StateMachineEventData evt)
        {
            switch (evt.Type)
            {
                case StateMachineEventType.DebugLog:
                    return $" {Truncate(evt.DebugMessage, 24)}";
                case StateMachineEventType.Wait:
                    return $" {evt.WaitDuration:F1}s";
                case StateMachineEventType.UnityEvent:
                    int count = evt.UnityEventCallbacks?.Count ?? 0;
                    return $" ({count} callback{(count != 1 ? "s" : "")})";
                case StateMachineEventType.Custom:
                    return $" \"{Truncate(evt.CustomText, 22)}\"";
                default:
                    return " Unknown";
            }
        }

        private float GetExpandedEventHeight(StateMachineEventData evt)
        {
            float h = UITheme.RowHeight;
            switch (evt.Type)
            {
                case StateMachineEventType.DebugLog:
                    h += UITheme.RowHeight + 4f;
                    break;
                case StateMachineEventType.Wait:
                    h += UITheme.RowHeight + 4f;
                    break;
                case StateMachineEventType.UnityEvent:
                    h += UITheme.RowHeight;
                    if (evt.UnityEventCallbacks != null)
                        h += evt.UnityEventCallbacks.Count * UITheme.RowHeight;
                    h += 4f;
                    break;
                case StateMachineEventType.Custom:
                    h += UITheme.RowHeight + 4f;
                    break;
            }
            return h;
        }

        private float ComputeSectionContentHeight(StateSectionData section)
        {
            if (!_expandedSections.Contains(section.SectionName))
                return 0f;

            float h = UITheme.RowHeight; // Add button
            
            if (section.Events.Count == 0)
            {
                h += UITheme.RowHeight;
            }
            else
            {
                for (int j = 0; j < section.Events.Count; j++)
                {
                    int eventId = section.Events[j].EditorId;
                    h += _expandedEvents.Contains(eventId)
                        ? GetExpandedEventHeight(section.Events[j])
                        : UITheme.RowHeight;
                }
            }
            return h;
        }

        private float ComputeTotalHeight(StateClassData stateClass)
        {
            float h = 0f;
            for (int i = 0; i < stateClass.Sections.Count; i++)
            {
                var section = stateClass.Sections[i];
                float contentH = ComputeSectionContentHeight(section);
                h += UITheme.RowHeight + contentH + 10f; // Foldout header + content + divider space
            }
            return h + 20f;
        }

        private static string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen) + "...";
        }
    }
}
