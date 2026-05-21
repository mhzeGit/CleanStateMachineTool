using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public static class ScriptReferenceUtility
    {
        public static string GetTypeName(MonoScript script)
        {
            if (script == null) return null;
            var type = script.GetClass();
            return type != null ? type.FullName : null;
        }

        public static MonoScript FindScriptByTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            for (int i = 0; i < scripts.Length; i++)
            {
                var type = scripts[i].GetClass();
                if (type != null && type.FullName == typeName)
                    return scripts[i];
            }
            return null;
        }
    }
}
