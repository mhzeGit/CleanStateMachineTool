using UnityEngine;
using CleanStateMachine;

public class SetVariable_StateBehaviour : StateBehaviour
{
    public BlackboardVariableReference target = new BlackboardVariableReference
    {
        UseBlackboard = true,
        ValueType = BlackboardVariableType.Bool,
    };
    public BlackboardVariableReference value = new BlackboardVariableReference
    {
        ValueType = BlackboardVariableType.Bool,
        DefaultValue = "True"
    };

    public override void OnStateEnter(StateMachineComponent stateMachine)
    {
        if (string.IsNullOrEmpty(target.BlackboardVariableName))
            return;

        switch (target.ValueType)
        {
            case BlackboardVariableType.Bool:
                stateMachine.SetBoolParameter(target.BlackboardVariableName, value.GetBoolValue(stateMachine));
                break;
            case BlackboardVariableType.Int:
                stateMachine.SetIntParameter(target.BlackboardVariableName, value.GetIntValue(stateMachine));
                break;
            case BlackboardVariableType.Float:
                stateMachine.SetFloatParameter(target.BlackboardVariableName, value.GetFloatValue(stateMachine));
                break;
            case BlackboardVariableType.String:
                stateMachine.SetStringParameter(target.BlackboardVariableName, value.GetStringValue(stateMachine));
                break;
            case BlackboardVariableType.Vector2:
                stateMachine.SetVector2Parameter(target.BlackboardVariableName, value.GetVector2Value(stateMachine));
                break;
            case BlackboardVariableType.Vector3:
                stateMachine.SetVector3Parameter(target.BlackboardVariableName, value.GetVector3Value(stateMachine));
                break;
        }
    }
}
