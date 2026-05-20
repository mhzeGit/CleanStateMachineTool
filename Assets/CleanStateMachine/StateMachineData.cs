using System.Collections.Generic;
using UnityEngine;

namespace CleanStateMachine
{
    [System.Serializable]
    public class StateData
    {
        public string Name = "New State";
        public Vector2 Position;
        public Vector2 Size = new Vector2(160f, 40f);
        public bool IsEntry;
    }

    [System.Serializable]
    public class ConnectionData
    {
        public int FromIndex;
        public int ToIndex;
    }

    [System.Serializable]
    public class GroupData
    {
        public string Label = "Group";
        public List<int> MemberIndices = new List<int>();
    }

    [System.Serializable]
    public class SerializableData
    {
        public List<StateData> States = new List<StateData>();
        public List<ConnectionData> Connections = new List<ConnectionData>();
        public List<GroupData> Groups = new List<GroupData>();
        public List<BlackboardVariable> BlackboardVariables = new List<BlackboardVariable>();
        public Vector2 PanOffset;
        public float Zoom = 1f;
        public bool ShowBlackboard = true;
        public float BlackboardWidth = 220f;
        public float DetailsWidth = 220f;
    }
}
