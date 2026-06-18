using System;
using System.Collections.Generic;

namespace CleanStateMachine
{
    public class AddBlackboardEventCommand : IUndoableCommand
    {
        private readonly List<BlackboardEvent> _eventList;
        private readonly BlackboardEvent _event;

        public string Description => $"Add Event '{_event.Name}'";

        public AddBlackboardEventCommand(
            List<BlackboardEvent> eventList,
            BlackboardEvent blackboardEvent)
        {
            _eventList = eventList ?? throw new ArgumentNullException(nameof(eventList));
            _event = blackboardEvent ?? throw new ArgumentNullException(nameof(blackboardEvent));
        }

        public void Execute()
        {
            if (!_eventList.Contains(_event))
                _eventList.Add(_event);
        }

        public void Undo()
        {
            _eventList.Remove(_event);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
