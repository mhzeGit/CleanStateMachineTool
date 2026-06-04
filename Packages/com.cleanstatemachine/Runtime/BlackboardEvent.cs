using System;
using ArgEvent;
using UnityEngine;

namespace CleanStateMachine
{
    [Serializable]
    public class BlackboardEvent
    {
        public string Name = "New Event";
        public ArgEventBinding argEvent = new ArgEventBinding();

        public BlackboardEvent Clone()
        {
            return new BlackboardEvent
            {
                Name = Name,
                argEvent = argEvent
            };
        }
    }
}
