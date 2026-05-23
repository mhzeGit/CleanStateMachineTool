using UnityEngine;
using CleanStateMachine;

public class TriggerEnter_SetBool : StateMachineAction
{
    public override BlackboardVariableType RequiredVariableType => BlackboardVariableType.Bool;

    [SerializeField] private bool _value = true;

    private void OnTriggerEnter(Collider other)
    {
        SetBlackboardValue(_value);
    }
}
