using ColossalFramework;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace CS2StyleJunctions.Patches
{
    [HarmonyPatch(typeof(NetSegment), nameof(NetSegment.CalculateCorner))]
    [HarmonyPatch(new[]
    {
        typeof(ushort), typeof(bool), typeof(bool), typeof(bool),
        typeof(Vector3), typeof(Vector3), typeof(bool)
    },
    new[]
    {
        ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
        ArgumentType.Out, ArgumentType.Out, ArgumentType.Out,
    })]
    public static class NetSegment_CalculateCorner_Patch
    {
        private const float VanillaBaseline = 4f;
        private const float MaxExtraDistance = 30f;
        private const float MaxLengthFraction = 0.4f;
        private const float SafetyMargin = 4f;

        private static readonly HashSet<ushort> _loggedNodes = new HashSet<ushort>();

        public static void Postfix(
            ushort segmentID,
            bool heightOffset,
            bool start,
            bool leftSide,
            ref Vector3 cornerPos,
            ref Vector3 cornerDirection,
            ref bool smooth)
        {
            NetManager netManager = Singleton<NetManager>.instance;
            ref NetSegment segment = ref netManager.m_segments.m_buffer[segmentID];

            ushort nodeId = start ? segment.m_startNode : segment.m_endNode;
            if (nodeId == 0) return;

            // Phase 4 build-time-decision change:
            // First check the bake cache. If we already chose a radius for
            // this (node, segment) pair on a previous frame, use that — we
            // do NOT re-derive from current slider values. This is what
            // makes slider changes only affect future-built junctions.
            float radius;
            if (!NodeRegistry.TryGetBakedRadius(nodeId, segmentID, out radius))
            {
                // First sight — derive from current settings and cache.
                radius = JunctionAnalyzer.ComputeRadius(nodeId, segmentID);
                if (radius < 0f)
                {
                    // Don't cache "skip this" decisions; they're cheap to
                    // re-derive and may legitimately change between frames
                    // (e.g. a new segment getting connected promotes a bend
                    // into a junction).
                    return;
                }
                NodeRegistry.BakeRadius(nodeId, segmentID, radius);
            }
            // From here on `radius` is the frozen decision.

            float extraDistance = radius - VanillaBaseline;
            if (extraDistance <= 0f) return;
            if (extraDistance > MaxExtraDistance) extraDistance = MaxExtraDistance;

            ushort startNodeId = segment.m_startNode;
            ushort endNodeId = segment.m_endNode;
            if (startNodeId == 0 || endNodeId == 0) return;

            Vector3 startPos = netManager.m_nodes.m_buffer[startNodeId].m_position;
            Vector3 endPos = netManager.m_nodes.m_buffer[endNodeId].m_position;
            float chordLength = Vector3.Distance(startPos, endPos);

            // For the other-end coordination, also use the bake cache so it's
            // consistent. If the other end isn't baked yet, we derive — that
            // case is rare and self-corrects on the next frame.
            ushort otherNodeId = start ? endNodeId : startNodeId;
            float otherRadius;
            if (!NodeRegistry.TryGetBakedRadius(otherNodeId, segmentID, out otherRadius))
            {
                otherRadius = JunctionAnalyzer.ComputeRadius(otherNodeId, segmentID);
            }
            float otherExtra = 0f;
            if (otherRadius > 0f)
            {
                otherExtra = otherRadius - VanillaBaseline;
                if (otherExtra < 0f) otherExtra = 0f;
                if (otherExtra > MaxExtraDistance) otherExtra = MaxExtraDistance;
            }

            float lengthClamp = chordLength * MaxLengthFraction;
            float availableRoom = chordLength - otherExtra - SafetyMargin;
            if (availableRoom < lengthClamp) lengthClamp = availableRoom;

            if (extraDistance > lengthClamp) extraDistance = lengthClamp;
            if (extraDistance < 0.5f) return;

            cornerPos += cornerDirection * extraDistance;

            if (!_loggedNodes.Contains(nodeId))
            {
                _loggedNodes.Add(nodeId);
                Debug.Log($"[CS2SJ] Node {nodeId} seg {segmentID}: baked {extraDistance:F1}m " +
                          $"(target {radius}m, chord {chordLength:F1}m, other end {otherExtra:F1}m)");
            }
        }
    }
}
