using System;
using System.Collections.Generic;

namespace CleanStateMachine
{
    public class DeleteBlackboardEventCommand : IUndoableCommand
    {
        private readonly List<BlackboardEvent> _eventList;
        private readonly BlackboardEvent _deletedEvent;
        private readonly int _deletedIndex;

        public string Description => "Delete Event";

        public DeleteBlackboardEventCommand(
            List<BlackboardEvent> eventList,
            int index)
        {
            _eventList = eventList ?? throw new ArgumentNullException(nameof(eventList));
            _deletedIndex = index;
            _deletedEvent = eventList[index].Clone();
        }

        public void Execute()
        {
            _eventList.RemoveAt(_deletedIndex);
        }

        public void Undo()
        {
            _eventList.Insert(_deletedIndex, _deletedEvent);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
