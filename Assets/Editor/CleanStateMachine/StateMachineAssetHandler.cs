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
            var controller = EditorUtility.InstanceIDToObject(instanceID) as StateMachineController;
            if (controller != null)
            {
                CleanStateMachineWindow.OpenWithController(controller);
                return true;
            }
            return false;
        }
    }
}
