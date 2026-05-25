namespace CleanStateMachine
{
    [System.Serializable]
    public class BlackboardVariableSelector
    {
        public string VariableName;
        public BlackboardVariableType ValueType = BlackboardVariableType.Bool;
    }
}
