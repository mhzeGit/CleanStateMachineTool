using System.Collections.Generic;
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

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (_data != null)
            {
                bool needsRebuild = false;
                for (int i = 0; i < _data.States.Count; i++)
                {
                    var sd = _data.States[i];
                    for (int j = 0; j < sd.Behaviours.Count; j++)
                    {
                        var be = sd.Behaviours[j];
                        if (be.Instance != null && be.Instance is StateBehaviour)
                            continue;
                        if (!string.IsNullOrEmpty(be.TypeName))
                        {
                            needsRebuild = true;
                            break;
                        }
                    }
                    if (needsRebuild) break;
                }
                if (!needsRebuild)
                {
                    for (int i = 0; i < _data.Connections.Count; i++)
                    {
                        var cd = _data.Connections[i];
                        if (cd.Conditions == null) continue;
                        for (int j = 0; j < cd.Conditions.Count; j++)
                        {
                            var ce = cd.Conditions[j];
                            if (ce.Instance != null && ce.Instance is ConditionScript)
                                continue;
                            if (!string.IsNullOrEmpty(ce.TypeName))
                            {
                                needsRebuild = true;
                                break;
                            }
                        }
                        if (needsRebuild) break;
                    }
                }
                if (needsRebuild)
                    RebuildBehaviourInstances(addSubAssets: false);
            }
#endif
        }

        public SerializableData Data
        {
            get => _data;
            set
            {
                _data = value;
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                    EnsureSubAssets();
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public void Save()
        {
#if UNITY_EDITOR
            EnsureSubAssets();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#endif
        }

        public void RebuildBehaviourInstances(bool addSubAssets = true)
        {
#if UNITY_EDITOR
            if (_data == null) return;

            for (int i = 0; i < _data.States.Count; i++)
            {
                var sd = _data.States[i];
                for (int j = 0; j < sd.Behaviours.Count; j++)
                {
                    var be = sd.Behaviours[j];
                    if (!string.IsNullOrEmpty(be.TypeName) && be.Instance == null)
                    {
                        var type = System.Type.GetType(be.TypeName);
                        if (type == null)
                        {
                            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(be.TypeName);
                                if (type != null) break;
                            }
                        }
                        if (type != null && type.IsSubclassOf(typeof(StateBehaviour)))
                        {
                            be.Instance = (StateBehaviour)ScriptableObject.CreateInstance(type);
                            be.Instance.name = $"{sd.Name}_Behaviour_{j}";
                            be.Instance.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }
                }
            }

            for (int i = 0; i < _data.Connections.Count; i++)
            {
                var cd = _data.Connections[i];
                if (cd.Conditions == null) return;
                for (int j = 0; j < cd.Conditions.Count; j++)
                {
                    var ce = cd.Conditions[j];
                    if (!string.IsNullOrEmpty(ce.TypeName) && ce.Instance == null)
                    {
                        var type = System.Type.GetType(ce.TypeName);
                        if (type == null)
                        {
                            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                            {
                                type = asm.GetType(ce.TypeName);
                                if (type != null) break;
                            }
                        }
                        if (type != null && type.IsSubclassOf(typeof(ConditionScript)))
                        {
                            ce.Instance = (ConditionScript)ScriptableObject.CreateInstance(type);
                            ce.Instance.name = $"Condition_{cd.FromIndex}_{cd.ToIndex}_{j}";
                            ce.Instance.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }
                }
            }

            if (addSubAssets)
            {
                EnsureSubAssets();
                EditorUtility.SetDirty(this);
            }
#endif
        }

#if UNITY_EDITOR
        public void EnsureSubAssets()
        {
            var path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path)) return;

            var referenced = new HashSet<Object>();
            CollectReferencedSubAssets(_data, referenced);

            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            for (int i = subAssets.Length - 1; i >= 0; i--)
            {
                var sub = subAssets[i];
                if (sub != null && !referenced.Contains(sub) &&
                    (sub is StateBehaviour || sub is ConditionScript))
                {
                    Object.DestroyImmediate(sub, true);
                }
            }
        }

        private void CollectReferencedSubAssets(SerializableData data, HashSet<Object> referenced)
        {
            if (data == null) return;
            for (int i = 0; i < data.States.Count; i++)
            {
                for (int j = 0; j < data.States[i].Behaviours.Count; j++)
                {
                    var inst = data.States[i].Behaviours[j].Instance;
                    if (inst != null)
                    {
                        referenced.Add(inst);
                        if (!AssetDatabase.Contains(inst))
                        {
                            inst.hideFlags = HideFlags.HideInHierarchy;
                            AssetDatabase.AddObjectToAsset(inst, this);
                        }
                    }
                }
            }

            for (int i = 0; i < data.Connections.Count; i++)
            {
                var cd = data.Connections[i];
                if (cd.Conditions == null) continue;
                for (int j = 0; j < cd.Conditions.Count; j++)
                {
                    var inst = cd.Conditions[j].Instance;
                    if (inst != null)
                    {
                        referenced.Add(inst);
                        if (!AssetDatabase.Contains(inst))
                        {
                            inst.hideFlags = HideFlags.HideInHierarchy;
                            AssetDatabase.AddObjectToAsset(inst, this);
                        }
                    }
                }
            }
        }
#endif
    }
}
