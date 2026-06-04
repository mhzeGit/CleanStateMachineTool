using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CleanStateMachine
{
    public class StateMachineComponent : MonoBehaviour
    {
        [SerializeField] private StateMachineController _controller;

        private Dictionary<string, BlackboardVariable> _runtimeVariableLookup = new Dictionary<string, BlackboardVariable>();
        private List<BlackboardVariable> _runtimeVariableList = new List<BlackboardVariable>();
        private float _stateEnterTime;
        private bool _initialized = false;
        private readonly List<int> _activeStatePath = new List<int>();
        private List<TransitionRecord> _recentTransitions = new List<TransitionRecord>();
        private bool _isTransitioning;
        private bool _running;

        [SerializeField] private List<BlackboardEvent> _blackboardEvents = new List<BlackboardEvent>();

        private readonly Dictionary<StateData, List<StateBehaviour>> _behaviourInstances = new Dictionary<StateData, List<StateBehaviour>>();
        private readonly List<ConditionScript> _runtimeConditionInstances = new List<ConditionScript>();
        private readonly Dictionary<ConditionEntry, ConditionScript> _conditionCache = new Dictionary<ConditionEntry, ConditionScript>();
        private readonly List<int> _transitionOldPathBuffer = new List<int>();

        private const int MaxRecentTransitions = 100;

        public static event System.Action<StateMachineComponent> OnStateEnteredGlobal;

        public StateMachineController Controller
        {
            get => _controller;
            set => _controller = value;
        }

        public SerializableData Data => _controller != null ? _controller.Data : null;

        public string CurrentStateName
        {
            get
            {
                int idx = CurrentStateIndex;
                if (Data == null || idx < 0 || idx >= Data.States.Count)
                    return "None";
                return Data.States[idx].Name;
            }
        }

        public int CurrentStateIndex
        {
            get
            {
                if (_activeStatePath.Count == 0) return -1;
                return _activeStatePath[^1];
            }
        }

        public IReadOnlyList<int> CurrentStatePath => _activeStatePath;

        public float StateEnterTime => _stateEnterTime;
        public List<BlackboardVariable> RuntimeVariables => _runtimeVariableList;
        public List<BlackboardEvent> RuntimeEvents => _blackboardEvents;
        public List<TransitionRecord> RecentTransitions => _recentTransitions;

        public event Action<int, int> OnStateChanged;
        public event Action<string> OnStateEntered;
        public event Action<string> OnStateExited;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized || _controller == null) return;

            CopyVariablesFromController();
            SyncEventNamesFromController();
            PreCreateBehaviourInstances();
            PreCreateConditionInstances();
            BuildEntryPath();
            _initialized = true;

            if (_activeStatePath.Count > 0)
            {
                if (!GetEntryStateAutoRun())
                {
                    _running = false;
                    return;
                }

                _running = true;
                _stateEnterTime = Time.time;
                int leafIndex = CurrentStateIndex;
                string initialStateName = CurrentStateName;
                OnStateChanged?.Invoke(-1, leafIndex);
                OnStateEntered?.Invoke(initialStateName);
                EnterPathBehaviours();
                OnStateEnteredGlobal?.Invoke(this);
            }
        }

        private bool GetEntryStateAutoRun()
        {
            if (Data == null) return true;
            for (int i = 0; i < Data.States.Count; i++)
            {
                if (Data.States[i].IsEntry)
                    return Data.States[i].AutoRun;
            }
            return true;
        }

        private void CopyVariablesFromController()
        {
            _runtimeVariableLookup.Clear();
            _runtimeVariableList.Clear();
            if (Data != null)
            {
                for (int i = 0; i < Data.BlackboardVariables.Count; i++)
                {
                    var clone = Data.BlackboardVariables[i].Clone();
                    _runtimeVariableLookup[clone.Name] = clone;
                    _runtimeVariableList.Add(clone);
                }
            }
        }

        private void BuildEntryPath()
        {
            _activeStatePath.Clear();
            if (Data == null) return;

            for (int i = 0; i < Data.States.Count; i++)
            {
                if (Data.States[i].IsEntry)
                {
                    BuildFullPath(i);
                    return;
                }
            }
        }

        private void BuildFullPath(int targetIndex)
        {
            _activeStatePath.Clear();

            var chain = new List<int>();
            int current = targetIndex;
            chain.Add(current);

            while (true)
            {
                int parent = FindParentContaining(current);
                if (parent < 0) break;
                chain.Add(parent);
                current = parent;
            }

            for (int i = chain.Count - 1; i >= 0; i--)
                _activeStatePath.Add(chain[i]);

            ResolveSubEntriesDownward(targetIndex);
        }

        private int FindParentContaining(int childIndex)
        {
            if (Data == null) return -1;
            for (int i = 0; i < Data.States.Count; i++)
            {
                if (Data.States[i].IsSubStateMachine &&
                    Data.States[i].ChildIndices != null &&
                    Data.States[i].ChildIndices.Contains(childIndex))
                    return i;
            }
            return -1;
        }

        private void ResolveSubEntriesDownward(int stateIndex)
        {
            if (Data == null || stateIndex < 0 || stateIndex >= Data.States.Count) return;
            var state = Data.States[stateIndex];
            if (!state.IsSubStateMachine) return;

            int subEntry = FindSubEntry(state);
            if (subEntry >= 0)
            {
                _activeStatePath.Add(subEntry);
                ResolveSubEntriesDownward(subEntry);
            }
        }

        private int FindSubEntry(StateData parentState)
        {
            if (!parentState.IsSubStateMachine || parentState.ChildIndices == null || parentState.ChildIndices.Count == 0)
                return -1;

            for (int i = 0; i < parentState.ChildIndices.Count; i++)
            {
                int childIdx = parentState.ChildIndices[i];
                if (childIdx >= 0 && childIdx < Data.States.Count && Data.States[childIdx].IsSubEntry)
                    return childIdx;
            }

            return parentState.ChildIndices[0];
        }

        private void Start()
        {
            if (!_initialized) Initialize();
        }

        private void Update()
        {
            if (!_initialized || !_running || _activeStatePath.Count == 0) return;

            int leafIndex = CurrentStateIndex;
            var behaviours = GetBehaviours(leafIndex);
            if (behaviours != null)
            {
                for (int i = 0; i < behaviours.Count; i++)
                    behaviours[i]?.OnStateUpdate(this);
            }

            CheckTransitions();
        }

        private List<StateBehaviour> GetBehaviours(int stateIndex)
        {
            if (Data == null || stateIndex < 0 || stateIndex >= Data.States.Count)
                return null;

            var stateData = Data.States[stateIndex];
            return GetOrCreateBehaviours(stateData);
        }

        private List<StateBehaviour> GetOrCreateBehaviours(StateData state)
        {
            if (_behaviourInstances.TryGetValue(state, out var existing))
                return existing;

            if (state.Behaviours.Count == 0)
                return null;

            return null;
        }

        private void PreCreateBehaviourInstances()
        {
            if (Data == null) return;

            for (int s = 0; s < Data.States.Count; s++)
            {
                var state = Data.States[s];
                if (state.Behaviours == null || state.Behaviours.Count == 0)
                    continue;

                var instances = new List<StateBehaviour>();
                for (int i = 0; i < state.Behaviours.Count; i++)
                {
                    var be = state.Behaviours[i];
                    if (be.Instance != null)
                    {
                        instances.Add(be.Instance);
                        continue;
                    }

                    if (string.IsNullOrEmpty(be.TypeName))
                        continue;

                    var type = ResolveType(be.TypeName);
                    if (type == null || !type.IsSubclassOf(typeof(StateBehaviour)))
                        continue;

                    var instance = (StateBehaviour)ScriptableObject.CreateInstance(type);
                    instance.name = $"{state.Name}_Behaviour_{i}";
                    instance.hideFlags = HideFlags.HideAndDontSave;
                    instances.Add(instance);
                }

                if (instances.Count > 0)
                    _behaviourInstances[state] = instances;
            }
        }

        private void PreCreateConditionInstances()
        {
            if (Data == null) return;

            for (int c = 0; c < Data.Connections.Count; c++)
            {
                var conditions = Data.Connections[c].Conditions;
                if (conditions == null) continue;

                for (int i = 0; i < conditions.Count; i++)
                {
                    var entry = conditions[i];
                    if (entry.Instance != null)
                        continue;

                    if (_conditionCache.ContainsKey(entry))
                        continue;

                    if (string.IsNullOrEmpty(entry.TypeName))
                        continue;

                    var type = ResolveType(entry.TypeName);
                    if (type == null || !type.IsSubclassOf(typeof(ConditionScript)))
                        continue;

                    var instance = (ConditionScript)ScriptableObject.CreateInstance(type);
                    instance.name = $"{entry.TypeName}_Condition";
                    instance.hideFlags = HideFlags.HideAndDontSave;
                    _conditionCache[entry] = instance;
                    _runtimeConditionInstances.Add(instance);
                }
            }
        }

        private void CheckTransitions()
        {
            if (_isTransitioning) return;
            if (Data == null || _activeStatePath.Count == 0) return;

            int leafIndex = _activeStatePath[^1];

            // Check Any State transitions first (global transitions)
            for (int c = 0; c < Data.Connections.Count; c++)
            {
                var connection = Data.Connections[c];
                if (connection.FromIndex < 0 || connection.FromIndex >= Data.States.Count)
                    continue;
                if (!Data.States[connection.FromIndex].IsAnyState)
                    continue;

                // Skip transition to self
                if (connection.ToIndex == leafIndex)
                    continue;

                if (!CanTransition(connection) || !EvaluateConditions(connection))
                    continue;

                // Prevent transition to a direct child of the current state
                if (IsDirectChildOf(connection.ToIndex, leafIndex))
                    continue;

                _isTransitioning = true;
                try
                {
                    TransitionToState(c);
                }
                finally
                {
                    _isTransitioning = false;
                }
                return;
            }

            for (int depth = _activeStatePath.Count - 1; depth >= 0; depth--)
            {
                int fromIndex = _activeStatePath[depth];
                bool isLeaf = depth == _activeStatePath.Count - 1;

                for (int c = 0; c < Data.Connections.Count; c++)
                {
                    var connection = Data.Connections[c];
                    if (connection.FromIndex != fromIndex) continue;

                    if (!CanTransition(connection) || !EvaluateConditions(connection))
                        continue;

                    if (!isLeaf && IsDirectChildOf(connection.ToIndex, fromIndex))
                        continue;

                    _isTransitioning = true;
                    try
                    {
                        TransitionToState(c);
                    }
                    finally
                    {
                        _isTransitioning = false;
                    }
                    return;
                }
            }
        }

        private bool CanTransition(ConnectionData connection)
        {
            if (connection.MinStateTime <= 0f)
                return true;
            return Time.time - _stateEnterTime >= connection.MinStateTime;
        }

        private bool IsDirectChildOf(int childIndex, int parentIndex)
        {
            if (Data == null) return false;
            if (parentIndex < 0 || parentIndex >= Data.States.Count) return false;

            var parent = Data.States[parentIndex];
            if (!parent.IsSubStateMachine || parent.ChildIndices == null)
                return false;

            return parent.ChildIndices.Contains(childIndex);
        }

        private bool EvaluateConditions(ConnectionData connection)
        {
            if (connection.Conditions == null || connection.Conditions.Count == 0)
                return true;

            for (int i = 0; i < connection.Conditions.Count; i++)
            {
                var entry = connection.Conditions[i];

                ConditionScript condition = entry.Instance;
                if (condition == null)
                {
                    if (!_conditionCache.TryGetValue(entry, out condition))
                        return false;
                }

                if (!condition.Evaluate(this))
                    return false;
            }
            return true;
        }

        private void TransitionToState(int connectionIndex)
        {
            var connection = Data.Connections[connectionIndex];
            int toIndex = connection.ToIndex;

            if (toIndex < 0 || toIndex >= Data.States.Count) return;

            int fromLeaf = CurrentStateIndex;
            string previousLeafName = CurrentStateName;

            _transitionOldPathBuffer.Clear();
            _transitionOldPathBuffer.AddRange(_activeStatePath);

            BuildFullPath(toIndex);

            int commonDepth = 0;
            while (commonDepth < _transitionOldPathBuffer.Count &&
                   commonDepth < _activeStatePath.Count &&
                   _transitionOldPathBuffer[commonDepth] == _activeStatePath[commonDepth])
                commonDepth++;

            if (commonDepth == _transitionOldPathBuffer.Count && commonDepth == _activeStatePath.Count)
            {
                if (connection.FromIndex != connection.ToIndex)
                    return;
                commonDepth = _activeStatePath.Count - 1;
            }

            for (int i = _transitionOldPathBuffer.Count - 1; i >= commonDepth; i--)
            {
                int idx = _transitionOldPathBuffer[i];
                if (idx >= 0 && idx < Data.States.Count)
                {
                    var stateData = Data.States[idx];
                    var behaviours = GetOrCreateBehaviours(stateData);
                    if (behaviours != null)
                    {
                        for (int j = 0; j < behaviours.Count; j++)
                            behaviours[j]?.OnStateExit(this);
                    }
                }
            }

            OnStateExited?.Invoke(previousLeafName);

            _stateEnterTime = Time.time;

            int newLeaf = CurrentStateIndex;
            string newLeafName = CurrentStateName;
            _recentTransitions.Add(new TransitionRecord
            {
                FromIndex = fromLeaf,
                ToIndex = toIndex,
                ConnectionIndex = connectionIndex
            });
            TrimRecentTransitions();

            OnStateChanged?.Invoke(fromLeaf, newLeaf);
            OnStateEntered?.Invoke(newLeafName);

            for (int i = commonDepth; i < _activeStatePath.Count; i++)
            {
                int idx = _activeStatePath[i];
                if (idx >= 0 && idx < Data.States.Count)
                {
                    var stateData = Data.States[idx];
                    var behaviours = GetOrCreateBehaviours(stateData);
                    if (behaviours != null)
                    {
                        for (int j = 0; j < behaviours.Count; j++)
                            behaviours[j]?.OnStateEnter(this);
                    }
                    ExecuteExternalAction(stateData);
                }
            }
            OnStateEnteredGlobal?.Invoke(this);
        }

        private void EnterPathBehaviours()
        {
            for (int i = 0; i < _activeStatePath.Count; i++)
            {
                int idx = _activeStatePath[i];
                if (idx >= 0 && idx < Data.States.Count)
                {
                    var stateData = Data.States[idx];
                    var behaviours = GetOrCreateBehaviours(stateData);
                    if (behaviours != null)
                    {
                        for (int j = 0; j < behaviours.Count; j++)
                            behaviours[j]?.OnStateEnter(this);
                    }
                    ExecuteExternalAction(stateData);
                }
            }
        }

        private void ExitPathBehaviours()
        {
            for (int i = _activeStatePath.Count - 1; i >= 0; i--)
            {
                int idx = _activeStatePath[i];
                if (idx >= 0 && idx < Data.States.Count)
                {
                    var stateData = Data.States[idx];
                    var behaviours = GetOrCreateBehaviours(stateData);
                    if (behaviours != null)
                    {
                        for (int j = 0; j < behaviours.Count; j++)
                            behaviours[j]?.OnStateExit(this);
                    }
                }
            }
        }

        private static readonly Dictionary<string, Type> _resolvedTypeCache = new Dictionary<string, Type>();

        private static Type ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            if (_resolvedTypeCache.TryGetValue(typeName, out var cached))
                return cached;

            var type = Type.GetType(typeName);
            if (type != null)
            {
                _resolvedTypeCache[typeName] = type;
                return type;
            }

            if (typeName.IndexOf(',') >= 0)
            {
                _resolvedTypeCache[typeName] = null;
                return null;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    _resolvedTypeCache[typeName] = type;
                    return type;
                }
            }

            _resolvedTypeCache[typeName] = null;
            return null;
        }

        private void OnDestroy()
        {
            DestroyRuntimeInstances();
        }

        private void DestroyRuntimeInstances()
        {
            foreach (var list in _behaviourInstances.Values)
            {
                if (list == null) continue;
                for (int i = 0; i < list.Count; i++)
                {
                    DestroyRuntimeObject(list[i]);
                }
            }
            _behaviourInstances.Clear();

            foreach (var instance in _runtimeConditionInstances)
            {
                DestroyRuntimeObject(instance);
            }
            _runtimeConditionInstances.Clear();
            _conditionCache.Clear();
        }

        private static void DestroyRuntimeObject(UnityEngine.Object obj)
        {
            if (obj != null && IsRuntimeInstance(obj))
                Destroy(obj);
        }

        private static bool IsRuntimeInstance(UnityEngine.Object obj)
        {
            return (obj.hideFlags & HideFlags.DontSaveInEditor) != 0;
        }

        private bool TryGetVariable(string name, BlackboardVariableType expectedType, out BlackboardVariable variable)
        {
            return _runtimeVariableLookup.TryGetValue(name, out variable) && variable.Type == expectedType;
        }

        public void SyncEventNamesFromController()
        {
            if (Data?.BlackboardEvents == null) return;
            var newEvents = new List<BlackboardEvent>();
            foreach (var ce in Data.BlackboardEvents)
            {
                var existing = _blackboardEvents.Find(e => e.Name == ce.Name);
                if (existing != null)
                    newEvents.Add(existing);
                else
                    newEvents.Add(new BlackboardEvent
                    {
                        Name = ce.Name
                    });
            }
            _blackboardEvents = newEvents;
        }

        public void InvokeEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            for (int i = 0; i < _blackboardEvents.Count; i++)
            {
                if (_blackboardEvents[i].Name == eventName)
                {
                    _blackboardEvents[i].argEvent?.Invoke();
                    return;
                }
            }
        }

        public void InvokeArgEvent(string eventName, object parameter)
        {
            if (string.IsNullOrEmpty(eventName)) return;
            for (int i = 0; i < _blackboardEvents.Count; i++)
            {
                var be = _blackboardEvents[i];
                if (be.Name == eventName)
                {
                    if (be.argEvent == null) return;
                    var listeners = be.argEvent.Listeners;
                    int count = listeners.Count;
                    for (int j = 0; j < count; j++)
                        listeners[j].Invoke(parameter);
                    return;
                }
            }
        }

        public void SetBoolParameter(string name, bool value)
        {
            if (TryGetVariable(name, BlackboardVariableType.Bool, out var v))
                v.BoolValue = value;
        }

        public void SetIntParameter(string name, int value)
        {
            if (TryGetVariable(name, BlackboardVariableType.Int, out var v))
                v.IntValue = value;
        }

        public void SetFloatParameter(string name, float value)
        {
            if (TryGetVariable(name, BlackboardVariableType.Float, out var v))
                v.FloatValue = value;
        }

        public void SetStringParameter(string name, string value)
        {
            if (TryGetVariable(name, BlackboardVariableType.String, out var v))
                v.StringValue = value;
        }

        public void SetTriggerParameter(string name)
        {
            if (TryGetVariable(name, BlackboardVariableType.Trigger, out var v))
                v.BoolValue = true;
        }

        public bool GetTriggerParameter(string name)
        {
            if (TryGetVariable(name, BlackboardVariableType.Trigger, out var v))
            {
                bool value = v.BoolValue;
                if (value)
                    v.BoolValue = false;
                return value;
            }
            return false;
        }

        public bool GetBoolParameter(string name)
        {
            if (TryGetVariable(name, BlackboardVariableType.Bool, out var v))
                return v.BoolValue;
            return false;
        }

        public int GetIntParameter(string name)
        {
            if (TryGetVariable(name, BlackboardVariableType.Int, out var v))
                return v.IntValue;
            return 0;
        }

        public float GetFloatParameter(string name)
        {
            if (TryGetVariable(name, BlackboardVariableType.Float, out var v))
                return v.FloatValue;
            return 0f;
        }

        public string GetStringParameter(string name)
        {
            if (TryGetVariable(name, BlackboardVariableType.String, out var v))
                return v.StringValue;
            return "";
        }

        public void SetState(string stateName)
        {
            if (Data == null) return;
            for (int i = 0; i < Data.States.Count; i++)
            {
                if (Data.States[i].Name == stateName)
                {
                    TransitionToStateDirect(i);
                    return;
                }
            }
        }

        private void TransitionToStateDirect(int targetIndex)
        {
            if (_isTransitioning) return;
            if (targetIndex < 0 || targetIndex >= Data.States.Count) return;
            if (_activeStatePath.Count > 0 && _activeStatePath[^1] == targetIndex) return;

            _isTransitioning = true;
            try
            {
                int fromLeaf = CurrentStateIndex;
                string previousLeafName = CurrentStateName;
                _transitionOldPathBuffer.Clear();
                _transitionOldPathBuffer.AddRange(_activeStatePath);
                BuildFullPath(targetIndex);

                int commonDepth = 0;
                while (commonDepth < _transitionOldPathBuffer.Count &&
                       commonDepth < _activeStatePath.Count &&
                       _transitionOldPathBuffer[commonDepth] == _activeStatePath[commonDepth])
                    commonDepth++;

                for (int i = _transitionOldPathBuffer.Count - 1; i >= commonDepth; i--)
                {
                    int idx = _transitionOldPathBuffer[i];
                    if (idx >= 0 && idx < Data.States.Count)
                    {
                        var stateData = Data.States[idx];
                        var behaviours = GetOrCreateBehaviours(stateData);
                        if (behaviours != null)
                        {
                            for (int j = 0; j < behaviours.Count; j++)
                                behaviours[j]?.OnStateExit(this);
                        }
                    }
                }

                OnStateExited?.Invoke(previousLeafName);
                _stateEnterTime = Time.time;

                int newLeaf = CurrentStateIndex;
                OnStateChanged?.Invoke(fromLeaf, newLeaf);
                OnStateEntered?.Invoke(CurrentStateName);

                for (int i = commonDepth; i < _activeStatePath.Count; i++)
                {
                    int idx = _activeStatePath[i];
                    if (idx >= 0 && idx < Data.States.Count)
                    {
                        var stateData = Data.States[idx];
                        var behaviours = GetOrCreateBehaviours(stateData);
                        if (behaviours != null)
                        {
                            for (int j = 0; j < behaviours.Count; j++)
                                behaviours[j]?.OnStateEnter(this);
                        }
                    ExecuteExternalAction(stateData);
                }
            }
            OnStateEnteredGlobal?.Invoke(this);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private void ExecuteExternalAction(StateData state)
        {
            if (!state.IsExternalReference)
                return;

            if (state.ExternalStateMachine == null)
            {
                Debug.LogWarning($"[CleanStateMachine] State '{state.Name}' has an External Reference action but no target GameObject assigned.", this);
                return;
            }

            var externalSm = state.ExternalStateMachine.GetComponent<StateMachineComponent>();
            if (externalSm == null)
            {
                Debug.LogWarning($"[CleanStateMachine] State '{state.Name}' external reference target '{state.ExternalStateMachine.name}' has no StateMachineComponent.", this);
                return;
            }

            switch (state.ExternalAction)
            {
                case ExternalStateMachineAction.StartStateMachine:
                    externalSm.ResetStateMachine();
                    break;
                case ExternalStateMachineAction.SetStateByName:
                    if (string.IsNullOrEmpty(state.ExternalTargetStateName))
                    {
                        Debug.LogWarning($"[CleanStateMachine] State '{state.Name}' external action is 'Set State By Name' but no target state name is configured.", this);
                        return;
                    }
                    externalSm.SetState(state.ExternalTargetStateName);
                    break;
                case ExternalStateMachineAction.SetBlackboardParameter:
                    if (string.IsNullOrEmpty(state.ExternalBlackboardParmName))
                    {
                        Debug.LogWarning($"[CleanStateMachine] State '{state.Name}' external action is 'Set Blackboard Parameter' but no parameter name is configured.", this);
                        return;
                    }
                    SetExternalBlackboardParm(externalSm, state);
                    break;
            }
        }

        private static void SetExternalBlackboardParm(StateMachineComponent sm, StateData state)
        {
            switch (state.ExternalBlackboardParmType)
            {
                case BlackboardVariableType.Bool:
                    sm.SetBoolParameter(state.ExternalBlackboardParmName,
                        bool.TryParse(state.ExternalBlackboardParmValue, out var bv) && bv);
                    break;
                case BlackboardVariableType.Int:
                    sm.SetIntParameter(state.ExternalBlackboardParmName,
                        int.TryParse(state.ExternalBlackboardParmValue, out var iv) ? iv : 0);
                    break;
                case BlackboardVariableType.Float:
                    sm.SetFloatParameter(state.ExternalBlackboardParmName,
                        float.TryParse(state.ExternalBlackboardParmValue,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out var fv) ? fv : 0f);
                    break;
                case BlackboardVariableType.String:
                    sm.SetStringParameter(state.ExternalBlackboardParmName,
                        state.ExternalBlackboardParmValue);
                    break;
                case BlackboardVariableType.Trigger:
                    if (bool.TryParse(state.ExternalBlackboardParmValue, out var tr) && tr)
                        sm.SetTriggerParameter(state.ExternalBlackboardParmName);
                    break;
            }
        }

        public void StartRunning()
        {
            if (!_initialized)
                Initialize();

            if (_running || _activeStatePath.Count == 0) return;

            _running = true;
            _stateEnterTime = Time.time;
            int leafIndex = CurrentStateIndex;
            string initialStateName = CurrentStateName;
            OnStateChanged?.Invoke(-1, leafIndex);
            OnStateEntered?.Invoke(initialStateName);
            EnterPathBehaviours();
        }

        public void ResetStateMachine()
        {
            DestroyRuntimeInstances();
            _initialized = false;
            _running = false;
            _recentTransitions.Clear();
            Initialize();
        }

        private void TrimRecentTransitions()
        {
            if (_recentTransitions.Count > MaxRecentTransitions)
                _recentTransitions.RemoveRange(0, _recentTransitions.Count - MaxRecentTransitions);
        }

        [System.Serializable]
        public class TransitionRecord
        {
            public int FromIndex;
            public int ToIndex;
            public int ConnectionIndex = -1;
        }
    }
}
