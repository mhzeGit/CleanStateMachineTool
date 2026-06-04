using System;
using ArgEvent;
using UnityEngine;
using UnityEngine.Events;

namespace CleanStateMachine
{
    public enum BlackboardEventType
    {
        UnityEvent,
        ArgEvent
    }

    [Serializable]
    public class BlackboardEvent
    {
        public string Name = "New Event";
        public BlackboardEventType EventType = BlackboardEventType.UnityEvent;
        public UnityEvent unityEvent = new UnityEvent();
        public ArgEventBinding argEvent = new ArgEventBinding();

        public BlackboardEvent Clone()
        {
            return new BlackboardEvent
            {
                Name = Name,
                EventType = EventType,
                unityEvent = unityEvent,
                argEvent = argEvent
            };
        }
    }
}
