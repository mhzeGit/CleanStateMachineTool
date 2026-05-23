using UnityEngine;

namespace CleanStateMachine
{
    public abstract class StateMachineAction : MonoBehaviour
    {
        [SerializeField] private StateMachineComponent _stateMachine;
        [SerializeField] private string _blackboardVariableName;
        [SerializeField] private BlackboardVariableType _blackboardVariableType;

        public StateMachineComponent StateMachine => _stateMachine;
        public string BlackboardVariableName => _blackboardVariableName;
        public virtual BlackboardVariableType RequiredVariableType => BlackboardVariableType.Bool;

        protected void SetBlackboardValue(bool value)
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                _stateMachine.SetBoolParameter(_blackboardVariableName, value);
        }

        protected void SetBlackboardValue(int value)
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                _stateMachine.SetIntParameter(_blackboardVariableName, value);
        }

        protected void SetBlackboardValue(float value)
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                _stateMachine.SetFloatParameter(_blackboardVariableName, value);
        }

        protected void SetBlackboardValue(string value)
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                _stateMachine.SetStringParameter(_blackboardVariableName, value);
        }

        protected void SetBlackboardValue(Vector2 value)
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                _stateMachine.SetVector2Parameter(_blackboardVariableName, value);
        }

        protected void SetBlackboardValue(Vector3 value)
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                _stateMachine.SetVector3Parameter(_blackboardVariableName, value);
        }

        protected bool GetBlackboardBool()
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                return _stateMachine.GetBoolParameter(_blackboardVariableName);
            return false;
        }

        protected int GetBlackboardInt()
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                return _stateMachine.GetIntParameter(_blackboardVariableName);
            return 0;
        }

        protected float GetBlackboardFloat()
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                return _stateMachine.GetFloatParameter(_blackboardVariableName);
            return 0f;
        }

        protected string GetBlackboardString()
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                return _stateMachine.GetStringParameter(_blackboardVariableName);
            return "";
        }

        protected Vector2 GetBlackboardVector2()
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                return _stateMachine.GetVector2Parameter(_blackboardVariableName);
            return Vector2.zero;
        }

        protected Vector3 GetBlackboardVector3()
        {
            if (_stateMachine != null && !string.IsNullOrEmpty(_blackboardVariableName))
                return _stateMachine.GetVector3Parameter(_blackboardVariableName);
            return Vector3.zero;
        }
    }
}
