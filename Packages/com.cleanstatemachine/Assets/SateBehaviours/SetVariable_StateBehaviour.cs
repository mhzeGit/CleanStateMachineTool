using UnityEngine;
using CleanStateMachine;

public class SetVariable_StateBehaviour : StateBehaviour
{
    public string variableName;
    public BlackboardVariableType variableType = BlackboardVariableType.Bool;
    public BlackboardVariableReference value = new BlackboardVariableReference
    {
        ValueType = BlackboardVariableType.Bool,
        DefaultValue = "True"
    };

    public override void OnStateEnter(StateMachineComponent stateMachine)
    {
        switch (variableType)
        {
            case BlackboardVariableType.Bool:
                stateMachine.SetBoolParameter(variableName, value.GetBoolValue(stateMachine));
                break;
            case BlackboardVariableType.Int:
                stateMachine.SetIntParameter(variableName, value.GetIntValue(stateMachine));
                break;
            case BlackboardVariableType.Float:
                stateMachine.SetFloatParameter(variableName, value.GetFloatValue(stateMachine));
                break;
            case BlackboardVariableType.String:
                stateMachine.SetStringParameter(variableName, value.GetStringValue(stateMachine));
                break;
            case BlackboardVariableType.Vector2:
                stateMachine.SetVector2Parameter(variableName, value.GetVector2Value(stateMachine));
                break;
            case BlackboardVariableType.Vector3:
                stateMachine.SetVector3Parameter(variableName, value.GetVector3Value(stateMachine));
                break;
        }
    }
}
