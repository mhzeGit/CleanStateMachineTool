using System.Reflection;
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

        private static readonly MethodInfo _entityIdToObject =
            typeof(EditorUtility).GetMethod("EntityIdToObject", BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo _instanceIdToObject =
            typeof(EditorUtility).GetMethod("InstanceIDToObject", new[] { typeof(int) });

        private static Object InstanceIDToObject(int instanceID)
        {
            if (_entityIdToObject != null)
            {
                var entityIdType = _entityIdToObject.GetParameters()[0].ParameterType;
                var fromULong = entityIdType.GetMethod("FromULong", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ulong) }, null);
                if (fromULong != null)
                {
                    var entityId = fromULong.Invoke(null, new object[] { (ulong)instanceID });
                    return (Object)_entityIdToObject.Invoke(null, new object[] { entityId });
                }
            }
            if (_instanceIdToObject != null)
                return (Object)_instanceIdToObject.Invoke(null, new object[] { instanceID });
            return null;
        }
    }
}
