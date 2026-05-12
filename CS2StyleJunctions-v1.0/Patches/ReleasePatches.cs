using HarmonyLib;

namespace CS2StyleJunctions.Patches
{
    // Fixes the "phantom corner on deleted junction" bug.
    //
    // The pattern: highway with a T-junction (3 segments, our patch polished
    // it). User deletes the side road. The highway's node now has only 2
    // segments — it's no longer a real junction. But our cache still holds
    // the polished radius for the old 3-segment configuration, so the next
    // render keeps pulling the corner back, creating the visible blue/teal
    // stub artifact.
    //
    // Fix: when a segment is released, evict cache entries for both of its
    // endpoint nodes, forcing the topology-changed neighbors to re-derive.

    [HarmonyPatch(typeof(NetManager), nameof(NetManager.ReleaseNode))]
    public static class NetManager_ReleaseNode_Patch
    {
        public static void Postfix(ushort node)
        {
            if (node == 0) return;
            NodeRegistry.OnNodeReleased(node);
        }
    }

    // Use a Prefix here so we can read m_startNode and m_endNode from the
    // segment BEFORE CS1 zeroes its data. A Postfix would see cleared
    // fields and miss the topology-aware eviction.
    [HarmonyPatch(typeof(NetManager), nameof(NetManager.ReleaseSegment))]
    public static class NetManager_ReleaseSegment_Patch
    {
        public static void Prefix(ushort segment, bool keepNodes)
        {
            if (segment == 0) return;
            NodeRegistry.OnSegmentReleased(segment);
        }
    }
}
