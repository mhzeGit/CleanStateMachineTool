using UnityEditor;

namespace CleanStateMachine
{
    internal static class BehaviourEntryExtensions
    {
        public static MonoScript GetScript(this BehaviourEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.TypeName))
                return null;
            return ScriptReferenceUtility.FindScriptByTypeName(entry.TypeName);
        }

        public static void SetScript(this BehaviourEntry entry, MonoScript script)
        {
            entry.TypeName = script != null ? ScriptReferenceUtility.GetTypeName(script) : null;
        }
    }

    internal static class ConditionEntryExtensions
    {
        public static MonoScript GetScript(this ConditionEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.TypeName))
                return null;
            return ScriptReferenceUtility.FindScriptByTypeName(entry.TypeName);
        }

        public static void SetScript(this ConditionEntry entry, MonoScript script)
        {
            entry.TypeName = script != null ? ScriptReferenceUtility.GetTypeName(script) : null;
        }
    }
}
