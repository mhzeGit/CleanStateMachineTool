using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public class TransitionConditionEditor
    {
        private readonly HashSet<int> _expandedConditions = new HashSet<int>();

        public event Action Changed;

        public void Draw(Rect rect, List<TransitionCondition> conditions,
            List<BlackboardVariable> blackboardVariables, ref Vector2 scrollPos)
        {
            if (conditions == null) return;

            float totalHeight = ComputeTotalHeight(conditions);
            Rect viewRect = new Rect(0f, 0f, rect.width - 14f, Mathf.Max(totalHeight, rect.height));
            scrollPos = GUI.BeginScrollView(rect, scrollPos, viewRect);

            float y = 0f;
            float w = viewRect.width;

            DrawAddButton(ref y, w, conditions);

            if (conditions.Count == 0)
            {
                Rect emptyRect = new Rect(0f, y, w, UITheme.RowHeight);
                var emptyStyle = new GUIStyle(UITheme.SecondaryStyle)
                {
                    normal = { textColor = UITheme.TextMuted },
                    fontStyle = FontStyle.Italic,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(emptyRect, "No conditions — click + to add", emptyStyle);
                y += UITheme.RowHeight;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];
                bool isExpanded = _expandedConditions.Contains(i);
                DrawConditionRow(ref y, w, conditions, i, condition, blackboardVariables, isExpanded);
            }

            GUI.EndScrollView();
        }

        private void DrawAddButton(ref float y, float width, List<TransitionCondition> conditions)
        {
            Rect addRect = new Rect(0f, y, width, UITheme.RowHeight);
            EditorGUI.DrawRect(addRect, UITheme.RowEven);

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
                conditions.Add(new TransitionCondition());
                Changed?.Invoke();
            }

            Rect labelRect = new Rect(btnRect.xMax + 8f, addRect.y, width - btnRect.xMax - 16f, addRect.height);
            GUI.Label(labelRect, "Add Condition", new GUIStyle(UITheme.SecondaryStyle)
            {
                normal = { textColor = UITheme.TextSecondary },
                fontSize = 11,
                fontStyle = FontStyle.Bold
            });

            y += UITheme.RowHeight;
            UITheme.DrawSectionDivider(y, width);
            y += 4f;
        }

        private void DrawConditionRow(ref float y, float width, List<TransitionCondition> conditions,
            int index, TransitionCondition condition, List<BlackboardVariable> blackboardVariables,
            bool isExpanded)
        {
            float rowW = width;
            float rowHeight = isExpanded ? GetExpandedHeight() : UITheme.RowHeight;

            Rect rowRect = new Rect(0f, y, rowW, rowHeight);
            EditorGUI.DrawRect(rowRect, index % 2 == 0 ? UITheme.RowEven : UITheme.RowOdd);

            // Condition type badge
            Rect badgeRect = new Rect(8f, rowRect.y + 6f, 50f, UITheme.RowHeight - 12f);
            EditorGUI.DrawRect(badgeRect, UITheme.TypeBadgeBg);
            var badgeStyle = new GUIStyle(UITheme.TypeBadgeStyle) { fontSize = 8 };
            string condLabel = "IF";
            GUI.Label(badgeRect, condLabel, badgeStyle);

            // Summary / expand
            Rect expandRect = new Rect(64f, rowRect.y, rowW - 90f, UITheme.RowHeight);
            string arrow = isExpanded ? " ▼" : " ▶";
            var summaryStyle = new GUIStyle(UITheme.SecondaryStyle)
            {
                clipping = TextClipping.Clip,
                padding = new RectOffset(4, 0, 0, 0)
            };

            if (GUI.Button(expandRect, arrow + GetConditionSummary(condition), summaryStyle))
            {
                if (isExpanded)
                    _expandedConditions.Remove(index);
                else
                    _expandedConditions.Add(index);
            }

            // Delete
            Rect deleteRect = new Rect(rowW - 24f, rowRect.y + 6f, 16f, UITheme.RowHeight - 12f);
            if (GUI.Button(deleteRect, "✕", UITheme.DeleteButtonStyle))
            {
                conditions.RemoveAt(index);
                RebuildExpandedAfterDeletion(index);
                Changed?.Invoke();
            }

            y += UITheme.RowHeight;

            if (isExpanded)
            {
                DrawConditionFields(ref y, width, 12f, condition, blackboardVariables);
            }
        }

        private void DrawConditionFields(ref float y, float width, float margin,
            TransitionCondition condition, List<BlackboardVariable> blackboardVariables)
        {
            float fieldWidth = width - margin;

            DrawVariableSelector(ref y, margin, fieldWidth, condition, blackboardVariables);

            BlackboardVariable variable = FindVariable(condition.BlackboardVariableName, blackboardVariables);
            if (variable != null)
            {
                DrawComparisonSelector(ref y, margin, fieldWidth, condition, variable);
                DrawValueField(ref y, margin, fieldWidth, condition, variable);
            }
            else
            {
                Rect msgRect = new Rect(margin, y, fieldWidth, UITheme.RowHeight);
                var msgStyle = new GUIStyle(UITheme.LabelStyle)
                {
                    normal = { textColor = UITheme.Warning }
                };
                GUI.Label(msgRect, "Select a variable first", msgStyle);
                y += UITheme.RowHeight;
            }
            
            y += 4f;
        }

        private void DrawVariableSelector(ref float y, float margin, float fieldWidth,
            TransitionCondition condition, List<BlackboardVariable> blackboardVariables)
        {
            Rect labelRect = new Rect(margin, y, 76f, UITheme.RowHeight);
            GUI.Label(labelRect, "Variable", UITheme.LabelStyle);

            Rect selectorRect = new Rect(margin + 80f, y + 4f,
                fieldWidth - 88f, UITheme.RowHeight - 8f);

            string currentName = condition.BlackboardVariableName ?? "";
            int selectedIndex = -1;
            for (int i = 0; i < blackboardVariables.Count; i++)
            {
                if (blackboardVariables[i].Name == currentName)
                {
                    selectedIndex = i;
                    break;
                }
            }

            string[] names = new string[blackboardVariables.Count + 1];
            names[0] = "(none)";
            for (int i = 0; i < blackboardVariables.Count; i++)
                names[i + 1] = blackboardVariables[i].Name;

            int newIndex = EditorGUI.Popup(selectorRect, selectedIndex + 1, names) - 1;
            string newName = newIndex >= 0 ? blackboardVariables[newIndex].Name : "";

            if (newName != currentName)
            {
                condition.BlackboardVariableName = newName;
                if (newIndex >= 0)
                    condition.Comparison = GetDefaultComparison(blackboardVariables[newIndex].Type);
                Changed?.Invoke();
            }

            y += UITheme.RowHeight;
        }

        private void DrawComparisonSelector(ref float y, float margin, float fieldWidth,
            TransitionCondition condition, BlackboardVariable variable)
        {
            Rect labelRect = new Rect(margin, y, 76f, UITheme.RowHeight);
            GUI.Label(labelRect, "Compare", UITheme.LabelStyle);

            Rect selectorRect = new Rect(margin + 80f, y + 4f,
                fieldWidth - 88f, UITheme.RowHeight - 8f);

            List<ConditionComparison> validComparisons = GetValidComparisons(variable.Type);

            string[] comparisonNames = new string[validComparisons.Count];
            int selectedIndex = 0;
            for (int i = 0; i < validComparisons.Count; i++)
            {
                comparisonNames[i] = FormatComparison(validComparisons[i]);
                if (validComparisons[i] == condition.Comparison)
                    selectedIndex = i;
            }

            int newIndex = EditorGUI.Popup(selectorRect, selectedIndex, comparisonNames);
            if (newIndex >= 0 && newIndex < validComparisons.Count)
            {
                var newComparison = validComparisons[newIndex];
                if (newComparison != condition.Comparison)
                {
                    condition.Comparison = newComparison;
                    Changed?.Invoke();
                }
            }

            y += UITheme.RowHeight;
        }

        private void DrawValueField(ref float y, float margin, float fieldWidth,
            TransitionCondition condition, BlackboardVariable variable)
        {
            Rect labelRect = new Rect(margin, y, 76f, UITheme.RowHeight);
            GUI.Label(labelRect, "Value", UITheme.LabelStyle);

            Rect fieldRect = new Rect(margin + 80f, y + 4f,
                fieldWidth - 88f, UITheme.RowHeight - 8f);

            string currentValue = condition.CompareValue ?? "";

            switch (variable.Type)
            {
                case BlackboardVariableType.Bool:
                {
                    bool boolVal = currentValue.ToLowerInvariant() == "true";
                    bool newVal = EditorGUI.Toggle(fieldRect, boolVal);
                    string newStr = newVal ? "true" : "false";
                    if (newStr != currentValue)
                    {
                        condition.CompareValue = newStr;
                        Changed?.Invoke();
                    }
                    break;
                }
                case BlackboardVariableType.Int:
                {
                    int.TryParse(currentValue, out int intVal);
                    int newInt = EditorGUI.IntField(fieldRect, intVal);
                    string newStr = newInt.ToString();
                    if (newStr != currentValue)
                    {
                        condition.CompareValue = newStr;
                        Changed?.Invoke();
                    }
                    break;
                }
                case BlackboardVariableType.Float:
                {
                    float.TryParse(currentValue,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out float floatVal);
                    float newFloat = EditorGUI.FloatField(fieldRect, floatVal);
                    string newStr = newFloat.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                    if (newStr != currentValue)
                    {
                        condition.CompareValue = newStr;
                        Changed?.Invoke();
                    }
                    break;
                }
                default:
                {
                    EditorGUI.DrawRect(fieldRect, UITheme.FieldBg);
                    string newValue = GUI.TextField(fieldRect, currentValue, UITheme.RowFieldStyle);
                    if (newValue != currentValue)
                    {
                        condition.CompareValue = newValue;
                        Changed?.Invoke();
                    }
                    break;
                }
            }

            y += UITheme.RowHeight;
        }

        private static BlackboardVariable FindVariable(string name, List<BlackboardVariable> variables)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == name)
                    return variables[i];
            }
            return null;
        }

        private static List<ConditionComparison> GetValidComparisons(BlackboardVariableType type)
        {
            switch (type)
            {
                case BlackboardVariableType.Bool:
                case BlackboardVariableType.Int:
                case BlackboardVariableType.Float:
                    return new List<ConditionComparison>
                    {
                        ConditionComparison.EqualTo, ConditionComparison.NotEqualTo,
                        ConditionComparison.GreaterThan, ConditionComparison.LessThan,
                        ConditionComparison.GreaterOrEqual, ConditionComparison.LessOrEqual
                    };
                case BlackboardVariableType.String:
                case BlackboardVariableType.Vector2:
                case BlackboardVariableType.Vector3:
                    return new List<ConditionComparison>
                    {
                        ConditionComparison.EqualTo, ConditionComparison.NotEqualTo
                    };
                default:
                    return new List<ConditionComparison> { ConditionComparison.EqualTo };
            }
        }

        private static ConditionComparison GetDefaultComparison(BlackboardVariableType type)
        {
            switch (type)
            {
                case BlackboardVariableType.Bool:
                    return ConditionComparison.EqualTo;
                case BlackboardVariableType.Int:
                case BlackboardVariableType.Float:
                    return ConditionComparison.GreaterThan;
                default:
                    return ConditionComparison.EqualTo;
            }
        }

        private static string FormatComparison(ConditionComparison comparison)
        {
            switch (comparison)
            {
                case ConditionComparison.GreaterThan: return "greater than";
                case ConditionComparison.LessThan: return "less than";
                case ConditionComparison.EqualTo: return "equal to";
                case ConditionComparison.NotEqualTo: return "not equal to";
                case ConditionComparison.GreaterOrEqual: return "greater or equal";
                case ConditionComparison.LessOrEqual: return "less or equal";
                default: return comparison.ToString();
            }
        }

        private void RebuildExpandedAfterDeletion(int deletedIndex)
        {
            var newExpanded = new HashSet<int>();
            foreach (int id in _expandedConditions)
            {
                if (id < deletedIndex)
                    newExpanded.Add(id);
                else if (id > deletedIndex)
                    newExpanded.Add(id - 1);
            }
            _expandedConditions.Clear();
            foreach (int id in newExpanded)
                _expandedConditions.Add(id);
        }

        private static string GetConditionSummary(TransitionCondition condition)
        {
            string varName = string.IsNullOrEmpty(condition.BlackboardVariableName)
                ? "(no variable)" : condition.BlackboardVariableName;
            string comp = FormatComparison(condition.Comparison);

            string value = condition.CompareValue;

            if (!string.IsNullOrEmpty(value))
                return $" {varName} {comp} {value}";
            else
                return $" {varName} {comp}";
        }

        private static float GetExpandedHeight()
        {
            return UITheme.RowHeight * 4f + 4f;
        }

        private float ComputeTotalHeight(List<TransitionCondition> conditions)
        {
            float h = UITheme.RowHeight + 4f;

            if (conditions.Count == 0)
                h += UITheme.RowHeight;
            else
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    h += _expandedConditions.Contains(i) ? GetExpandedHeight() : UITheme.RowHeight;
                }
            }
            return h + 12f;
        }
    }
}
