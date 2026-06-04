using System;
using System.Collections.Generic;
using ArgEvent;
using UnityEngine;

namespace CleanStateMachine
{
    [Serializable]
    public class BlackboardEvent
    {
        public string Name = "New Event";
        public ArgEventBinding argEvent = new ArgEventBinding();
        public List<BlackboardEventArg> Arguments = new List<BlackboardEventArg>();

        public BlackboardEvent Clone()
        {
            var clone = new BlackboardEvent
            {
                Name = Name,
                argEvent = argEvent
            };
            clone.Arguments.Clear();
            for (int i = 0; i < Arguments.Count; i++)
                clone.Arguments.Add(Arguments[i].Clone());
            return clone;
        }
    }
}
