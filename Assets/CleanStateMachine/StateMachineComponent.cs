using UnityEngine;

namespace CleanStateMachine
{
    public class StateMachineComponent : MonoBehaviour
    {
        [SerializeField] private StateMachineController _controller;

        public StateMachineController Controller
        {
            get => _controller;
            set => _controller = value;
        }

        public SerializableData Data => _controller != null ? _controller.Data : null;
    }
}
