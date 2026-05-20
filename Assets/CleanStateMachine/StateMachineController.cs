using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CleanStateMachine
{
    [CreateAssetMenu(menuName = "Clean State Machine/Controller", fileName = "NewStateMachineController")]
    public class StateMachineController : ScriptableObject
    {
        [SerializeField] private SerializableData _data = new SerializableData();

        public SerializableData Data
        {
            get => _data;
            set
            {
                _data = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }
    }
}
