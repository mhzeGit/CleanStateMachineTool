using System;

namespace CleanStateMachine
{
    [Serializable]
    public class BlackboardEventArg
    {
        public string Name = "New Arg";
        public BlackboardVariableType Type = BlackboardVariableType.Float;

        public BlackboardEventArg Clone()
        {
            return new BlackboardEventArg
            {
                Name = Name,
                Type = Type
            };
        }
    }
}
