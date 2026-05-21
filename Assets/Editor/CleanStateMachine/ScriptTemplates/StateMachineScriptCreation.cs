using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    public static class StateMachineScriptCreation
    {
        private const string StateBehaviourTemplate = "Assets/Editor/CleanStateMachine/ScriptTemplates/StateBehaviourTemplate.txt";
        private const string ConditionScriptTemplate = "Assets/Editor/CleanStateMachine/ScriptTemplates/ConditionScriptTemplate.txt";

        [MenuItem("Assets/Create/Clean State Machine/State Behaviour", false, 80)]
        private static void CreateStateBehaviour()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(StateBehaviourTemplate, "NewStateBehaviour.cs");
        }

        [MenuItem("Assets/Create/Clean State Machine/Condition Script", false, 81)]
        private static void CreateConditionScript()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(ConditionScriptTemplate, "NewConditionScript.cs");
        }
    }
}
