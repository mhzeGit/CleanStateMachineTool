using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CleanStateMachine
{
    public static class StateMachineAssetHandler
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
#if UNITY_6000_0_OR_NEWER
            var controller = EditorUtility.EntityIdToObject(EntityId.FromULong((ulong)instanceID)) as StateMachineController;
#else
            var controller = EditorUtility.InstanceIDToObject(instanceID) as StateMachineController;
#endif
            if (controller != null)
            {
                CleanStateMachineWindow.OpenWithController(controller);
                return true;
            }
            return false;
        }
    }
}
