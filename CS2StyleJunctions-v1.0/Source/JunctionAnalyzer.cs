using ColossalFramework;
using UnityEngine;

namespace CS2StyleJunctions
{
    // The road's standalone classification, derived purely from its NetInfo
    // (lane count, AI type). Ramps aren't a class on this enum because
    // "ramp" is a property of the junction context, not the road itself.
    public enum RoadClass
    {
        Unknown,
        Small,
        Medium,
        Large,
        Highway
    }

    public static class JunctionAnalyzer
    {
        private const float RampAngleThresholdDegrees = 60f;
        private const int MinJunctionSegments = 3;

        // Classify a single road by its NetInfo, ignoring junction context.
        // Used by the UI to highlight which slider would apply if this road
        // were used in a typical (non-ramp) junction.
        public static RoadClass ClassifyRoad(NetInfo info)
        {
            if (info == null) return RoadClass.Unknown;

            bool isHighway = (info.m_netAI is RoadBaseAI roadAI && roadAI.m_highwayRules);
            if (isHighway) return RoadClass.Highway;

            int laneCount = info.m_lanes != null ? info.m_lanes.Length : 0;
            if (laneCount == 0) return RoadClass.Unknown;
            if (laneCount >= 6) return RoadClass.Large;
            if (laneCount >= 3) return RoadClass.Medium;
            return RoadClass.Small;
        }

        public static float ComputeRadius(ushort nodeId, ushort segmentId)
        {
            if (!NodeRegistry.ShouldProcess(nodeId)) return -1f;

            ref NetNode node = ref Singleton<NetManager>.instance.m_nodes.m_buffer[nodeId];

            if ((node.m_flags & NetNode.Flags.Created) == NetNode.Flags.None) return -1f;
            if ((node.m_flags & NetNode.Flags.Underground) != NetNode.Flags.None) return -1f;

            int segmentCount = 0;
            int maxLanes = 0;
            bool anyHighway = false;
            bool anyNonHighway = false;
            NetManager netManager = Singleton<NetManager>.instance;

            ushort[] segmentIds = new ushort[8];
            bool[] segmentIsHighway = new bool[8];

            for (int i = 0; i < 8; i++)
            {
                ushort sId = node.GetSegment(i);
                segmentIds[i] = sId;
                if (sId == 0) continue;
                segmentCount++;

                ref NetSegment segment = ref netManager.m_segments.m_buffer[sId];
                NetInfo info = segment.Info;
                if (info == null) continue;

                int laneCount = info.m_lanes != null ? info.m_lanes.Length : 0;
                if (laneCount > maxLanes) maxLanes = laneCount;

                bool isHighway = (info.m_netAI is RoadBaseAI roadAI && roadAI.m_highwayRules);
                segmentIsHighway[i] = isHighway;
                if (isHighway) anyHighway = true;
                else anyNonHighway = true;
            }

            if (segmentCount < MinJunctionSegments) return -1f;
            if (maxLanes == 0) return -1f;

            ref NetSegment thisSegment = ref netManager.m_segments.m_buffer[segmentId];
            NetInfo thisInfo = thisSegment.Info;
            bool thisIsHighway = (thisInfo != null && thisInfo.m_netAI is RoadBaseAI thisAI && thisAI.m_highwayRules);

            JunctionSettings s = JunctionSettings.Active;

            if (anyHighway && anyNonHighway)
            {
                if (HasRampAngle(nodeId, segmentIds, segmentIsHighway, segmentCount))
                {
                    return thisIsHighway ? s.RampHighwaySideRadius : s.RampSideRadius;
                }
            }

            if (anyHighway) return s.HighwayRadius;
            if (maxLanes >= 6) return s.LargeRoadRadius;
            if (maxLanes >= 3) return s.MediumRoadRadius;
            return s.SmallRoadRadius;
        }

        private static bool HasRampAngle(
            ushort nodeId,
            ushort[] segmentIds,
            bool[] segmentIsHighway,
            int segmentCount)
        {
            NetManager netManager = Singleton<NetManager>.instance;

            Vector3[] dirs = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                ushort sId = segmentIds[i];
                if (sId == 0)
                {
                    dirs[i] = Vector3.zero;
                    continue;
                }
                ref NetSegment seg = ref netManager.m_segments.m_buffer[sId];
                Vector3 d = (seg.m_startNode == nodeId) ? seg.m_startDirection : seg.m_endDirection;
                d.y = 0f;
                d.Normalize();
                dirs[i] = d;
            }

            for (int i = 0; i < 8; i++)
            {
                if (segmentIds[i] == 0) continue;
                for (int j = i + 1; j < 8; j++)
                {
                    if (segmentIds[j] == 0) continue;

                    bool mixed = segmentIsHighway[i] != segmentIsHighway[j];
                    if (!mixed) continue;

                    float dot = Vector3.Dot(dirs[i], dirs[j]);
                    dot = Mathf.Clamp(dot, -1f, 1f);
                    float angleDegrees = Mathf.Acos(dot) * Mathf.Rad2Deg;

                    float deviation = Mathf.Min(angleDegrees, 180f - angleDegrees);
                    if (deviation < RampAngleThresholdDegrees)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
