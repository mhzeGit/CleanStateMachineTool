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
            var controller = InstanceIDToObject(instanceID) as StateMachineController;
            if (controller != null)
            {
                CleanStateMachineWindow.OpenWithController(controller);
                return true;
            }
            return false;
        }

        private static Object InstanceIDToObject(int instanceID)
        {
#pragma warning disable 0618, 0619
            return EditorUtility.InstanceIDToObject(instanceID);
#pragma warning restore 0618, 0619
        }
    }
}
