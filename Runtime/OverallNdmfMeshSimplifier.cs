using System;
using System.Collections.Generic;
using UnityEngine;
using Meshia.MeshSimplification;

namespace com.aoyon.OverallNDMFMeshSimplifiers
{
    [DisallowMultipleComponent]
    [AddComponentMenu("OverallNDMFMeshSimplifier/OverallNDMFMeshSimplifier")]
    public class OverallNdmfMeshSimplifier : MonoBehaviour
#if CONTAINS_VRCHAT_BASE
    , VRC.SDKBase.IEditorOnly
#endif
    {
        public bool IsAutoAdjust = false;
        public int TargetTriangleCount = 70000;
        public List<OverallNdmfMeshSimplifierTarget> Targets = new();
    }

    [Serializable]
    public struct OverallNdmfMeshSimplifierTarget : IEquatable<OverallNdmfMeshSimplifierTarget>
    {
        public OverallNdmfMeshSimplifierTargetState State;
        public Renderer Renderer;

        // triangles.lengh / 3
        public int AbsoulteTriangleCount;
        public int TotalTriangleCount;

        public bool Fixed; // AutoAdjustの対象から除外

        public MeshSimplifierOptions Options;

        public OverallNdmfMeshSimplifierTarget(Renderer renderer, OverallNdmfMeshSimplifierTargetState state, int initialTriangleCount)
        {
            Renderer = renderer;
            State = state;
            Fixed = false;
            AbsoulteTriangleCount = initialTriangleCount;
            TotalTriangleCount = initialTriangleCount;
            Options = MeshSimplifierOptions.Default;
        }

        public bool IsValid() => Renderer != null && State == OverallNdmfMeshSimplifierTargetState.Enabled;

        public bool Equals(OverallNdmfMeshSimplifierTarget other)
        {
            return State == other.State && Renderer.Equals(other.Renderer) && Fixed == other.Fixed && AbsoulteTriangleCount == other.AbsoulteTriangleCount && TotalTriangleCount == other.TotalTriangleCount && Options.Equals(other.Options);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OverallNdmfMeshSimplifierTarget other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(State, Renderer, Fixed, AbsoulteTriangleCount, TotalTriangleCount, Options);
        }
    }

    public enum OverallNdmfMeshSimplifierTargetState
    {
        Enabled, // 簡略化の対象
        Disabled, // 簡略化せずそのままの値を総和に用いる
        EditorOnly // 総和を始め処理の対象に含めない
    }
}