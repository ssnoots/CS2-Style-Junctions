using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace CS2StyleJunctions
{
    public static class NodeRegistry
    {
        private static readonly HashSet<ushort> _preExistingNodes = new HashSet<ushort>();
        private static readonly Dictionary<long, float> _bakedRadii = new Dictionary<long, float>();

        public static bool ApplicationEnabled { get; set; } = false;
        public static bool AffectExistingNodes { get; set; } = false;

        public static int BakedDecisionCount => _bakedRadii.Count;

        public static void SnapshotExistingNodes()
        {
            _preExistingNodes.Clear();
            _bakedRadii.Clear();

            NetManager netManager = Singleton<NetManager>.instance;
            if (netManager == null) return;

            int count = 0;
            uint bufferLength = (uint)netManager.m_nodes.m_buffer.Length;
            for (uint i = 1; i < bufferLength; i++)
            {
                ref NetNode node = ref netManager.m_nodes.m_buffer[i];
                if ((node.m_flags & NetNode.Flags.Created) != NetNode.Flags.None)
                {
                    _preExistingNodes.Add((ushort)i);
                    count++;
                }
            }

            Debug.Log($"[CS2SJ] Snapshotted {count} pre-existing nodes; new nodes built " +
                      $"after this point will be eligible for auto-polish.");
        }

        public static void Clear()
        {
            _preExistingNodes.Clear();
            _bakedRadii.Clear();
        }

        public static bool IsPreExisting(ushort nodeId)
        {
            return _preExistingNodes.Contains(nodeId);
        }

        public static bool ShouldProcess(ushort nodeId)
        {
            if (!ApplicationEnabled) return false;
            if (!AffectExistingNodes && IsPreExisting(nodeId)) return false;
            return true;
        }

        private static long MakeKey(ushort nodeId, ushort segmentId)
        {
            return ((long)nodeId << 16) | (long)segmentId;
        }

        public static bool TryGetBakedRadius(ushort nodeId, ushort segmentId, out float radius)
        {
            return _bakedRadii.TryGetValue(MakeKey(nodeId, segmentId), out radius);
        }

        public static void BakeRadius(ushort nodeId, ushort segmentId, float radius)
        {
            _bakedRadii[MakeKey(nodeId, segmentId)] = radius;
        }

        public static int ClearBakedRadii()
        {
            int n = _bakedRadii.Count;
            _bakedRadii.Clear();
            return n;
        }

        // Evict every cache entry that references nodeId.
        public static int EvictByNode(ushort nodeId)
        {
            int evicted = 0;
            var keysToRemove = new List<long>();
            foreach (var kvp in _bakedRadii)
            {
                ushort cachedNodeId = (ushort)(kvp.Key >> 16);
                if (cachedNodeId == nodeId)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (long key in keysToRemove)
            {
                _bakedRadii.Remove(key);
                evicted++;
            }
            return evicted;
        }

        // Evict every cache entry that references segmentId.
        public static int EvictBySegment(ushort segmentId)
        {
            int evicted = 0;
            var keysToRemove = new List<long>();
            foreach (var kvp in _bakedRadii)
            {
                ushort cachedSegmentId = (ushort)(kvp.Key & 0xFFFF);
                if (cachedSegmentId == segmentId)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (long key in keysToRemove)
            {
                _bakedRadii.Remove(key);
                evicted++;
            }
            return evicted;
        }

        // Called when a node is deleted. Removes from snapshot (so the ID
        // could be reused as a "new" node), and evicts all cache entries
        // for the node itself.
        public static void OnNodeReleased(ushort nodeId)
        {
            _preExistingNodes.Remove(nodeId);
            EvictByNode(nodeId);
        }

        // Called when a segment is deleted. This is the more important hook
        // because deleting a side road from a T-junction leaves the junction
        // node with the wrong classification (3 segments -> 2 segments, no
        // longer a junction). The cached corner radius for that node and the
        // surviving segments is now wrong.
        //
        // The fix: when a segment is released, walk both of its endpoints
        // and evict cache entries for EVERY segment still connected to
        // those endpoint nodes. That forces a clean re-derive on the next
        // render of any segment whose topology was affected.
        public static void OnSegmentReleased(ushort segmentId)
        {
            // First evict for the dying segment itself.
            int evicted = EvictBySegment(segmentId);

            // Then walk both endpoint nodes (read while the segment is still
            // valid — CS1's Postfix runs before the segment data is zeroed).
            NetManager netManager = Singleton<NetManager>.instance;
            if (netManager == null) return;

            ref NetSegment segment = ref netManager.m_segments.m_buffer[segmentId];
            ushort startNode = segment.m_startNode;
            ushort endNode = segment.m_endNode;

            // For each endpoint node, evict cache entries for it. The next
            // render of any of its remaining segments will re-derive cleanly
            // against the new topology.
            if (startNode != 0)
                evicted += EvictByNode(startNode);
            if (endNode != 0 && endNode != startNode)
                evicted += EvictByNode(endNode);

            if (evicted > 0)
            {
                Debug.Log($"[CS2SJ] Segment {segmentId} released; evicted {evicted} cache entries " +
                          $"(neighbor nodes {startNode}, {endNode}).");
            }
        }
    }
}
