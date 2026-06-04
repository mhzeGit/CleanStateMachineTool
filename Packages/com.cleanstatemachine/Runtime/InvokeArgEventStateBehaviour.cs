using UnityEngine;

namespace CleanStateMachine
{
    public class InvokeArgEventStateBehaviour : StateBehaviour
    {
        public override string DisplayName => "Invoke ArgEvent";

        public enum InvokeTiming
        {
            OnEnter,
            OnUpdate,
            OnExit
        }

        public InvokeTiming invokeOn = InvokeTiming.OnEnter;
        public BlackboardEventSelector targetEvent = new BlackboardEventSelector();
        public BlackboardVariableSelector parameterVariable = new BlackboardVariableSelector();

        public override void OnStateEnter(StateMachineComponent stateMachine)
        {
            if (invokeOn == InvokeTiming.OnEnter)
                Invoke(stateMachine);
        }

        public override void OnStateUpdate(StateMachineComponent stateMachine)
        {
            if (invokeOn == InvokeTiming.OnUpdate)
                Invoke(stateMachine);
        }

        public override void OnStateExit(StateMachineComponent stateMachine)
        {
            if (invokeOn == InvokeTiming.OnExit)
                Invoke(stateMachine);
        }

        private void Invoke(StateMachineComponent stateMachine)
        {
            if (string.IsNullOrEmpty(targetEvent.EventName))
                return;

            if (!string.IsNullOrEmpty(parameterVariable.VariableName))
            {
                object paramValue = ResolveParameter(stateMachine);
                stateMachine.InvokeArgEvent(targetEvent.EventName, paramValue);
            }
            else
            {
                stateMachine.InvokeEvent(targetEvent.EventName);
            }
        }

        private object ResolveParameter(StateMachineComponent stateMachine)
        {
            var variables = stateMachine.RuntimeVariables;
            if (variables == null) return null;

            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].Name == parameterVariable.VariableName)
                    return variables[i].GetValueAsObject();
            }
            return null;
        }
    }
}
