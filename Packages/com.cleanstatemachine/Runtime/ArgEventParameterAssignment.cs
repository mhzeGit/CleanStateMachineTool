using System;

namespace CleanStateMachine
{
    [Serializable]
    public class ArgEventParameterAssignment
    {
        public string ArgumentName = "";
        public BlackboardVariableReference Value = new BlackboardVariableReference();
    }
}
