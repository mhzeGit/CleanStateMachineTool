using System;
using System.Collections.Generic;

namespace CleanStateMachine
{
    public class AddBlackboardVariableCommand : IUndoableCommand
    {
        private readonly List<BlackboardVariable> _variableList;
        private readonly BlackboardVariable _variable;

        public string Description => $"Add Variable '{_variable.Name}'";

        public AddBlackboardVariableCommand(
            List<BlackboardVariable> variableList,
            BlackboardVariable variable)
        {
            _variableList = variableList ?? throw new ArgumentNullException(nameof(variableList));
            _variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }

        public void Execute()
        {
            if (!_variableList.Contains(_variable))
                _variableList.Add(_variable);
        }

        public void Undo()
        {
            _variableList.Remove(_variable);
        }

        public void Redo()
        {
            Execute();
        }
    }
}
