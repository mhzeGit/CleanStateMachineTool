using UnityEngine;

namespace CleanStateMachine
{
    public class InvokeUnityEventStateBehaviour : StateBehaviour
    {
        public override string DisplayName => "Invoke Blackboard Event";

        public enum InvokeTiming
        {
            OnEnter,
            OnUpdate,
            OnExit
        }

        public InvokeTiming invokeOn = InvokeTiming.OnEnter;
        public BlackboardEventSelector targetEvent = new BlackboardEventSelector();

        public override void OnStateEnter(StateMachineComponent stateMachine)
        {
            if (invokeOn == InvokeTiming.OnEnter && !string.IsNullOrEmpty(targetEvent.EventName))
                stateMachine.InvokeEvent(targetEvent.EventName);
        }

        public override void OnStateUpdate(StateMachineComponent stateMachine)
        {
            if (invokeOn == InvokeTiming.OnUpdate && !string.IsNullOrEmpty(targetEvent.EventName))
                stateMachine.InvokeEvent(targetEvent.EventName);
        }

        public override void OnStateExit(StateMachineComponent stateMachine)
        {
            if (invokeOn == InvokeTiming.OnExit && !string.IsNullOrEmpty(targetEvent.EventName))
                stateMachine.InvokeEvent(targetEvent.EventName);
        }
    }
}
