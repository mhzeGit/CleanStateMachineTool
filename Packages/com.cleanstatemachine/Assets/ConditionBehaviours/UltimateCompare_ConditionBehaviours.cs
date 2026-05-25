using UnityEngine;
using CleanStateMachine;

public class UltimateCompare_ConditionBehaviours : ConditionScript
{
    public enum CompareType
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }

    public BlackboardVariableType variableType = BlackboardVariableType.Float;

    public BlackboardVariableReference input1 = new BlackboardVariableReference
    {
        ValueType = BlackboardVariableType.Float,
        DefaultValue = "0"
    };

    public BlackboardVariableReference input2 = new BlackboardVariableReference
    {
        ValueType = BlackboardVariableType.Float,
        DefaultValue = "0"
    };

    public CompareType comparison = CompareType.Equal;
    public bool ignoreCase = true;

    public override string DisplayName => "Compare Variable";

    private void OnValidate()
    {
        input1.ValueType = variableType;
        input2.ValueType = variableType;
    }

    public override bool ShouldShowProperty(string propertyName)
    {
        if (propertyName == "ignoreCase")
            return variableType == BlackboardVariableType.String;
        return true;
    }

    public override bool Evaluate(StateMachineComponent stateMachine)
    {
        return variableType switch
        {
            BlackboardVariableType.Bool => CompareBool(stateMachine),
            BlackboardVariableType.Int => CompareInt(stateMachine),
            BlackboardVariableType.Float => CompareFloat(stateMachine),
            BlackboardVariableType.String => CompareString(stateMachine),
            BlackboardVariableType.Vector2 => CompareVector2(stateMachine),
            BlackboardVariableType.Vector3 => CompareVector3(stateMachine),
            _ => false
        };
    }

    private bool CompareBool(StateMachineComponent sm)
    {
        bool a = input1.GetBoolValue(sm);
        bool b = input2.GetBoolValue(sm);
        return comparison switch
        {
            CompareType.Equal => a == b,
            CompareType.NotEqual => a != b,
            _ => false
        };
    }

    private bool CompareInt(StateMachineComponent sm)
    {
        int a = input1.GetIntValue(sm);
        int b = input2.GetIntValue(sm);
        return comparison switch
        {
            CompareType.Equal => a == b,
            CompareType.NotEqual => a != b,
            CompareType.GreaterThan => a > b,
            CompareType.GreaterThanOrEqual => a >= b,
            CompareType.LessThan => a < b,
            CompareType.LessThanOrEqual => a <= b,
            _ => false
        };
    }

    private bool CompareFloat(StateMachineComponent sm)
    {
        float a = input1.GetFloatValue(sm);
        float b = input2.GetFloatValue(sm);
        return comparison switch
        {
            CompareType.Equal => Mathf.Approximately(a, b),
            CompareType.NotEqual => !Mathf.Approximately(a, b),
            CompareType.GreaterThan => a > b,
            CompareType.GreaterThanOrEqual => a >= b || Mathf.Approximately(a, b),
            CompareType.LessThan => a < b,
            CompareType.LessThanOrEqual => a <= b || Mathf.Approximately(a, b),
            _ => false
        };
    }

    private bool CompareString(StateMachineComponent sm)
    {
        string a = input1.GetStringValue(sm);
        string b = input2.GetStringValue(sm);
        System.StringComparison c = ignoreCase
            ? System.StringComparison.OrdinalIgnoreCase
            : System.StringComparison.Ordinal;
        return comparison switch
        {
            CompareType.Equal => string.Equals(a, b, c),
            CompareType.NotEqual => !string.Equals(a, b, c),
            _ => false
        };
    }

    private bool CompareVector2(StateMachineComponent sm)
    {
        Vector2 a = input1.GetVector2Value(sm);
        Vector2 b = input2.GetVector2Value(sm);
        return comparison switch
        {
            CompareType.Equal => a == b,
            CompareType.NotEqual => a != b,
            CompareType.GreaterThan => a.sqrMagnitude > b.sqrMagnitude,
            CompareType.GreaterThanOrEqual => a.sqrMagnitude >= b.sqrMagnitude,
            CompareType.LessThan => a.sqrMagnitude < b.sqrMagnitude,
            CompareType.LessThanOrEqual => a.sqrMagnitude <= b.sqrMagnitude,
            _ => false
        };
    }

    private bool CompareVector3(StateMachineComponent sm)
    {
        Vector3 a = input1.GetVector3Value(sm);
        Vector3 b = input2.GetVector3Value(sm);
        return comparison switch
        {
            CompareType.Equal => a == b,
            CompareType.NotEqual => a != b,
            CompareType.GreaterThan => a.sqrMagnitude > b.sqrMagnitude,
            CompareType.GreaterThanOrEqual => a.sqrMagnitude >= b.sqrMagnitude,
            CompareType.LessThan => a.sqrMagnitude < b.sqrMagnitude,
            CompareType.LessThanOrEqual => a.sqrMagnitude <= b.sqrMagnitude,
            _ => false
        };
    }
}
