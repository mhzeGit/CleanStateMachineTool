using System.Collections.Generic;
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
        public List<ArgEventParameterAssignment> argParameters = new List<ArgEventParameterAssignment>();

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

        public void Invoke(StateMachineComponent stateMachine)
        {
            if (string.IsNullOrEmpty(targetEvent.EventName))
                return;

            var runtimeEvents = stateMachine.RuntimeEvents;
            BlackboardEvent targetEventDef = null;
            for (int i = 0; i < runtimeEvents.Count; i++)
            {
                if (runtimeEvents[i].Name == targetEvent.EventName)
                {
                    targetEventDef = runtimeEvents[i];
                    break;
                }
            }

            if (targetEventDef == null)
            {
                stateMachine.InvokeEvent(targetEvent.EventName);
                return;
            }

            var argDefs = targetEventDef.Arguments;
            if (argDefs == null || argDefs.Count == 0)
            {
                stateMachine.InvokeEvent(targetEvent.EventName);
                return;
            }

            object[] paramValues = new object[argDefs.Count];
            for (int i = 0; i < argDefs.Count; i++)
                paramValues[i] = ResolveParameterValue(stateMachine, argDefs[i].Name, argDefs[i].Type);

            stateMachine.InvokeArgEvent(targetEvent.EventName, paramValues);
        }

        private object ResolveParameterValue(StateMachineComponent stateMachine, string argName, BlackboardVariableType argType)
        {
            for (int i = 0; i < argParameters.Count; i++)
            {
                if (argParameters[i].ArgumentName == argName)
                {
                    var assignment = argParameters[i];
                    if (assignment.Value.UseBlackboard && !string.IsNullOrEmpty(assignment.Value.BlackboardVariableName))
                    {
                        var variables = stateMachine.RuntimeVariables;
                        if (variables != null)
                        {
                            for (int j = 0; j < variables.Count; j++)
                            {
                                if (variables[j].Name == assignment.Value.BlackboardVariableName)
                                {
                                    return argType switch
                                    {
                                        BlackboardVariableType.Bool => variables[j].BoolValue,
                                        BlackboardVariableType.Trigger => variables[j].BoolValue,
                                        BlackboardVariableType.Int => variables[j].IntValue,
                                        BlackboardVariableType.Float => variables[j].FloatValue,
                                        BlackboardVariableType.String => variables[j].StringValue,
                                        _ => variables[j].StringValue
                                    };
                                }
                            }
                        }
                        return null;
                    }

                    return argType switch
                    {
                        BlackboardVariableType.Bool => assignment.Value.GetBoolValue(null),
                        BlackboardVariableType.Trigger => assignment.Value.GetBoolValue(null),
                        BlackboardVariableType.Int => assignment.Value.GetIntValue(null),
                        BlackboardVariableType.Float => assignment.Value.GetFloatValue(null),
                        BlackboardVariableType.String => assignment.Value.DefaultValue,
                        _ => assignment.Value.DefaultValue
                    };
                }
            }
            return null;
        }
    }
}
