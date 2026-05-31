using System;
using UnityEngine;
using UnityEngine.Events;

namespace CleanStateMachine
{
    [Serializable]
    public class BlackboardEvent
    {
        public string Name = "New Event";
        public UnityEvent unityEvent = new UnityEvent();

        public BlackboardEvent Clone()
        {
            return new BlackboardEvent
            {
                Name = Name,
                unityEvent = unityEvent
            };
        }
    }
}
