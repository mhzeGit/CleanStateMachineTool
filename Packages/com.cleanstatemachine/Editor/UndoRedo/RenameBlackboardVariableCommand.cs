using System;

namespace CleanStateMachine
{
    public class RenameBlackboardVariableCommand : IUndoableCommand
    {
        private readonly BlackboardVariable _variable;
        private readonly string _oldName;
        private readonly string _newName;

        public string Description => "Rename Variable";

        public RenameBlackboardVariableCommand(
            BlackboardVariable variable,
            string oldName,
            string newName)
        {
            _variable = variable ?? throw new ArgumentNullException(nameof(variable));
            _oldName = oldName;
            _newName = newName;
        }

        public void Execute()
        {
            _variable.Name = _newName;
        }

        public void Undo()
        {
            _variable.Name = _oldName;
        }

        public void Redo()
        {
            _variable.Name = _newName;
        }
    }
}
