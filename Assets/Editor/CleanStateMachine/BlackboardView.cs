using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public class BlackboardView
    {
        private Vector2 _scrollPos;

        public event System.Action VariablesChanged;

        public void Draw(Rect rect, List<BlackboardVariable> variables)
        {
            if (variables == null)
                return;

            var e = Event.current;

            UITheme.DrawPanelBackground(rect);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, UITheme.HeaderHeight);
            DrawHeader(headerRect, variables);

            Rect listRect = new Rect(
                rect.x,
                rect.y + UITheme.HeaderHeight,
                rect.width,
                rect.height - UITheme.HeaderHeight
            );
            DrawVariableList(listRect, variables);

            if (listRect.Contains(e.mousePosition))
                EditorGUIUtility.AddCursorRect(listRect, MouseCursor.Text);
        }

        private void DrawHeader(Rect rect, List<BlackboardVariable> variables)
        {
            UITheme.DrawHeaderBackground(rect);

            float toggleSize = 20f;
            float addSize = 24f;
            float gap = 8f;
            float rightEdge = rect.x + rect.width;

            Rect addRect = new Rect(
                rightEdge - toggleSize - addSize - gap,
                rect.y + (rect.height - addSize) * 0.5f,
                addSize,
                addSize
            );

            Rect labelRect = new Rect(rect.x, rect.y, addRect.x - rect.x - 4f, rect.height);
            GUI.Label(labelRect, "Blackboard", UITheme.HeaderStyle);

            bool hover = addRect.Contains(Event.current.mousePosition);
            UITheme.DrawSmallButton(addRect, hover);

            if (GUI.Button(addRect, "+", UITheme.CloseButtonStyle))
            {
                var menu = new GenericMenu();
                foreach (BlackboardVariableType type in Enum.GetValues(typeof(BlackboardVariableType)))
                {
                    BlackboardVariableType captured = type;
                    string label = ObjectNames.NicifyVariableName(type.ToString());
                    menu.AddItem(new GUIContent(label), false, () =>
                    {
                        variables.Add(new BlackboardVariable
                        {
                            Name = GetUniqueName(variables, "New Variable"),
                            Type = captured
                        });
                        ResetValueForType(variables[variables.Count - 1]);
                        VariablesChanged?.Invoke();
                        _scrollPos.y = float.MaxValue;
                    });
                }
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private void DrawVariableList(Rect rect, List<BlackboardVariable> variables)
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                DefocusTextField();

            float totalHeight = variables.Count * UITheme.RowHeight;
            Rect viewRect = new Rect(0f, 0f, rect.width - 14f, totalHeight);

            _scrollPos = GUI.BeginScrollView(rect, _scrollPos, viewRect);

            int deleteIndex = -1;

            for (int i = 0; i < variables.Count; i++)
            {
                Rect rowRect = new Rect(0f, i * UITheme.RowHeight, viewRect.width, UITheme.RowHeight);
                if (DrawVariableRow(rowRect, variables[i], i))
                {
                    deleteIndex = i;
                }
            }

            if (deleteIndex >= 0)
            {
                variables.RemoveAt(deleteIndex);
                VariablesChanged?.Invoke();
            }

            if (e.type == EventType.KeyDown &&
                (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
                DefocusTextField();

            GUI.EndScrollView();
        }

        private static void DefocusTextField()
        {
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;
            EditorGUIUtility.editingTextField = false;
            GUI.FocusControl("");
        }

        // Returns true if delete was clicked
        private bool DrawVariableRow(Rect rect, BlackboardVariable variable, int index)
        {
            Color rowBg = index % 2 == 0 ? UITheme.RowEven : UITheme.RowOdd;
            EditorGUI.DrawRect(rect, rowBg);

            float pad = 8f;
            float innerW = rect.width - pad * 2f;
            float fieldH = rect.height - 8f;
            float fieldY = rect.y + 4f;

            float typeW = 54f;
            float delW = 16f;
            float gap = 4f;

            Rect typeRect = new Rect(rect.x + pad, fieldY, typeW, fieldH);
            EditorGUI.DrawRect(typeRect, UITheme.TypeBadgeBg);
            GUI.Label(typeRect, variable.Type.ToString().ToUpper(), UITheme.TypeBadgeStyle);

            float remainingW = innerW - typeW - delW - gap * 2f;
            float nameW = remainingW * 0.45f;
            float valueW = remainingW * 0.55f;

            Rect nameRect = new Rect(typeRect.xMax + gap, fieldY, nameW, fieldH);
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.TextField(nameRect, variable.Name, UITheme.RowFieldStyle);
            if (EditorGUI.EndChangeCheck())
            {
                variable.Name = newName;
                VariablesChanged?.Invoke();
            }

            Rect valueRect = new Rect(nameRect.xMax + gap, fieldY, valueW, fieldH);
            DrawValueField(valueRect, variable);

            Rect delRect = new Rect(valueRect.xMax + gap, fieldY, delW, fieldH);
            if (GUI.Button(delRect, "✕", UITheme.DeleteButtonStyle))
            {
                return true;
            }

            return false;
        }

        private static void ResetValueForType(BlackboardVariable v)
        {
            v.StringValue = v.Type switch
            {
                BlackboardVariableType.Bool => "False",
                BlackboardVariableType.Int => "0",
                BlackboardVariableType.Float => "0",
                BlackboardVariableType.String => "",
                BlackboardVariableType.Vector2 => "0,0",
                BlackboardVariableType.Vector3 => "0,0,0",
                _ => "0"
            };
        }

        private void DrawValueField(Rect rect, BlackboardVariable variable)
        {
            switch (variable.Type)
            {
                case BlackboardVariableType.Bool:
                {
                    bool val = variable.BoolValue;
                    float toggleWidth = 16f;
                    Rect toggleRect = new Rect(
                        rect.x + (rect.width - toggleWidth) * 0.5f,
                        rect.y + (rect.height - 16f) * 0.5f,
                        toggleWidth,
                        16f
                    );
                    bool result = EditorGUI.Toggle(toggleRect, val);
                    if (result != val)
                    {
                        variable.BoolValue = result;
                        GUI.changed = true;
                    }
                    break;
                }
                case BlackboardVariableType.Int:
                {
                    int val = variable.IntValue;
                    int result = EditorGUI.IntField(rect, val);
                    if (result != val)
                    {
                        variable.IntValue = result;
                        GUI.changed = true;
                    }
                    break;
                }
                case BlackboardVariableType.Float:
                {
                    float val = variable.FloatValue;
                    float result = EditorGUI.FloatField(rect, val);
                    if (Mathf.Abs(result - val) > 1e-6f)
                    {
                        variable.FloatValue = result;
                        GUI.changed = true;
                    }
                    break;
                }
                case BlackboardVariableType.String:
                {
                    string result = EditorGUI.TextField(rect, variable.StringValue, UITheme.RowFieldStyle);
                    if (result != variable.StringValue)
                    {
                        variable.StringValue = result;
                        GUI.changed = true;
                    }
                    break;
                }
                case BlackboardVariableType.Vector2:
                {
                    Vector2 val = variable.Vector2Value;
                    Vector2 result = EditorGUI.Vector2Field(rect, GUIContent.none, val);
                    if (result != val)
                    {
                        variable.Vector2Value = result;
                        GUI.changed = true;
                    }
                    break;
                }
                case BlackboardVariableType.Vector3:
                {
                    Vector3 val = variable.Vector3Value;
                    Vector3 result = EditorGUI.Vector3Field(rect, GUIContent.none, val);
                    if (result != val)
                    {
                        variable.Vector3Value = result;
                        GUI.changed = true;
                    }
                    break;
                }
            }
        }

        private static string GetUniqueName(List<BlackboardVariable> variables, string baseName)
        {
            if (!VariableExists(variables, baseName))
                return baseName;

            for (int i = 1; i < 1000; i++)
            {
                string candidate = $"{baseName} {i}";
                if (!VariableExists(variables, candidate))
                    return candidate;
            }

            return baseName;
        }

        private static bool VariableExists(List<BlackboardVariable> variables, string name)
        {
            for (int i = 0; i < variables.Count; i++)
                if (variables[i].Name == name)
                    return true;
            return false;
        }
    }
}
