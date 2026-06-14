using UnityEngine;
using CleanStateMachine;

public class DestroyGameObject_StateBehaviour : StateBehaviour
{
    public override string DisplayName => "Destroy GameObject";

    public BlackboardVariableSelector target = new BlackboardVariableSelector
    {
        ValueType = BlackboardVariableType.GameObject,
    };

    public override void OnStateEnter(StateMachineComponent stateMachine)
    {
        if (string.IsNullOrEmpty(target.VariableName))
            return;

        GameObject go = stateMachine.GetGameObjectParameter(target.VariableName);
        if (go != null)
            Object.Destroy(go);
    }
}
