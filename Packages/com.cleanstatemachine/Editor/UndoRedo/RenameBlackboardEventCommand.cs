using System;

namespace CleanStateMachine
{
    public class RenameBlackboardEventCommand : IUndoableCommand
    {
        private readonly BlackboardEvent _event;
        private readonly string _oldName;
        private readonly string _newName;

        public string Description => "Rename Event";

        public RenameBlackboardEventCommand(
            BlackboardEvent blackboardEvent,
            string oldName,
            string newName)
        {
            _event = blackboardEvent ?? throw new ArgumentNullException(nameof(blackboardEvent));
            _oldName = oldName;
            _newName = newName;
        }

        public void Execute()
        {
            _event.Name = _newName;
        }

        public void Undo()
        {
            _event.Name = _oldName;
        }

        public void Redo()
        {
            _event.Name = _newName;
        }
    }
}
