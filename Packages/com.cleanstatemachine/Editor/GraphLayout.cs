using System;
using System.Collections.Generic;
using UnityEngine;

namespace CleanStateMachine
{
    internal static class GraphLayout
    {
        private const float HorizontalSpacing = 220f;
        private const float VerticalSpacing = 100f;

        public static Dictionary<StateView, Vector2> ComputeLayout(
            List<StateView> allStates,
            List<ConnectionView> allConnections,
            SelectionController selection)
        {
            var selected = new List<StateView>();
            for (int i = 0; i < selection.Count; i++)
            {
                if (selection.Selected[i] is StateView sv)
                    selected.Add(sv);
            }

            if (selected.Count == 0)
                return null;

            var selectedSet = new HashSet<StateView>(selected);

            var outgoing = new Dictionary<StateView, List<StateView>>();
            var incoming = new Dictionary<StateView, List<StateView>>();
            foreach (var s in selected)
            {
                outgoing[s] = new List<StateView>();
                incoming[s] = new List<StateView>();
            }

            foreach (var conn in allConnections)
            {
                if (selectedSet.Contains(conn.From) && selectedSet.Contains(conn.To))
                {
                    outgoing[conn.From].Add(conn.To);
                    incoming[conn.To].Add(conn.From);
                }
            }

            var layerIndex = new Dictionary<StateView, int>();
            var visited = new HashSet<StateView>();

            var queue = new Queue<StateView>();
            bool hasEntry = false;
            foreach (var s in selected)
            {
                if (s.IsEntry)
                {
                    queue.Enqueue(s);
                    layerIndex[s] = 0;
                    hasEntry = true;
                    break;
                }
            }

            if (!hasEntry)
            {
                foreach (var s in selected)
                {
                    if (incoming[s].Count == 0)
                    {
                        queue.Enqueue(s);
                        layerIndex[s] = 0;
                    }
                }
            }

            if (queue.Count == 0 && selected.Count > 0)
            {
                queue.Enqueue(selected[0]);
                layerIndex[selected[0]] = 0;
            }

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();
                if (!visited.Add(curr))
                    continue;

                int nextLayer = layerIndex[curr] + 1;
                foreach (var next in outgoing[curr])
                {
                    if (!layerIndex.ContainsKey(next) || layerIndex[next] < nextLayer)
                        layerIndex[next] = nextLayer;
                    if (!visited.Contains(next))
                        queue.Enqueue(next);
                }
            }

            foreach (var s in selected)
            {
                if (!visited.Contains(s))
                {
                    int fallback = 0;
                    while (true)
                    {
                        bool conflict = false;
                        foreach (var kvp in layerIndex)
                        {
                            if (kvp.Value == fallback)
                            {
                                conflict = true;
                                break;
                            }
                        }
                        if (!conflict) break;
                        fallback++;
                    }
                    layerIndex[s] = fallback;
                    visited.Add(s);
                }
            }

            var layerMap = new Dictionary<int, List<StateView>>();
            int maxLayer = 0;
            foreach (var kvp in layerIndex)
            {
                int li = kvp.Value;
                if (li > maxLayer) maxLayer = li;
                if (!layerMap.TryGetValue(li, out var list))
                {
                    list = new List<StateView>();
                    layerMap[li] = list;
                }
                list.Add(kvp.Key);
            }

            var result = new Dictionary<StateView, Vector2>();

            for (int layer = 0; layer <= maxLayer; layer++)
            {
                if (!layerMap.TryGetValue(layer, out var layerStates))
                    continue;

                layerStates.Sort((a, b) =>
                {
                    if (a.IsEntry != b.IsEntry) return a.IsEntry ? -1 : 1;
                    return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                });

                float layerWidth = (layerStates.Count - 1) * HorizontalSpacing;
                float startX = -layerWidth * 0.5f;

                for (int i = 0; i < layerStates.Count; i++)
                {
                    float x = startX + i * HorizontalSpacing;
                    float y = layer * VerticalSpacing;
                    result[layerStates[i]] = new Vector2(x, y);
                }
            }

            return result;
        }
    }
}
