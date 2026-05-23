using UnityEngine;
using CleanStateMachine;

public class DebugLog_StateBehaviour : StateBehaviour
{
    public CleanStateMachine.BlackboardVariableReference message = new CleanStateMachine.BlackboardVariableReference
    {
        ValueType = CleanStateMachine.BlackboardVariableType.String,
        DefaultValue = "Hello World"
    };

    public override void OnStateEnter(StateMachineComponent stateMachine)
    {
        Debug.Log(message.GetStringValue(stateMachine));
        base.OnStateEnter(stateMachine);
    }

    public override void OnStateUpdate(StateMachineComponent stateMachine)
    {
        base.OnStateUpdate(stateMachine);
    }

    public override void OnStateExit(StateMachineComponent stateMachine)
    {
        base.OnStateExit(stateMachine);
    }
}
