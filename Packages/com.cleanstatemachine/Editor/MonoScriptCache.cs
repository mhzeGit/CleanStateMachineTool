using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CleanStateMachine
{
    internal static class MonoScriptCache
    {
        private static Dictionary<Type, List<MonoScript>> _cache;
        private static bool _dirty = true;

        internal static List<MonoScript> GetScriptsByBaseType<T>() where T : class
        {
            Type baseType = typeof(T);
            if (_cache == null)
                _cache = new Dictionary<Type, List<MonoScript>>();

            if (_dirty || !_cache.TryGetValue(baseType, out var scripts))
            {
                scripts = BuildCache(baseType);
                _cache[baseType] = scripts;
            }
            return scripts;
        }

        private static List<MonoScript> BuildCache(Type baseType)
        {
            var results = new List<MonoScript>();
            var guids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null)
                {
                    var type = script.GetClass();
                    if (type != null && type.IsSubclassOf(baseType))
                        results.Add(script);
                }
            }
            results.Sort((a, b) => a.name.CompareTo(b.name));
            return results;
        }

        internal static void InvalidateCache()
        {
            _dirty = true;
        }

        internal class Postprocessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (var path in importedAssets)
                {
                    if (path.EndsWith(".cs"))
                    {
                        MonoScriptCache.InvalidateCache();
                        return;
                    }
                }
                foreach (var path in deletedAssets)
                {
                    if (path.EndsWith(".cs"))
                    {
                        MonoScriptCache.InvalidateCache();
                        return;
                    }
                }
            }
        }
    }
}
