using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Meshia.MeshSimplification;

namespace com.aoyon.OverallNDMFMeshSimplifier
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Overall NDMF Mesh Simplifier/Overall NDMF Mesh Simplifier")]
    public class OverallNdmfMeshSimplifier : MonoBehaviour
#if CONTAINS_VRCHAT_BASE
    , VRC.SDKBase.IEditorOnly
#endif
    {
        public bool IsAutoAdjust = false;
        public int TargetTriangleCount = 70000;
        public List<OverallNDMFMeshSimplifierTarget> Targets = new();
    }
    
    [Serializable]
    public struct OverallNDMFMeshSimplifierTarget : IEquatable<OverallNDMFMeshSimplifierTarget>
    {
        public Renderer Renderer;
        public OverallNdmfMeshSimplifierTargetState State;

        // triangles.lengh / 3
        public int AbsoulteTriangleCount;
        public int TotalTriangleCount;

        public bool Fixed; // AutoAdjustの対象から除外

        public MeshSimplifierOptions Options;
        
        public static bool TryGet(Renderer renderer, out OverallNDMFMeshSimplifierTarget target)
        {
            target = default;

            if (renderer == null) return false;
            if (renderer is not SkinnedMeshRenderer and not MeshRenderer) return false;
            var mesh = Utils.GetMesh(renderer);
            if (mesh == null) return false;
            if (mesh.triangles.Length < 0) return false;

            target = new();
            target.Renderer = renderer;
            target.State = OverallNdmfMeshSimplifierTargetState.Enabled;
            target.AbsoulteTriangleCount = mesh.triangles.Length / 3;
            target.TotalTriangleCount = mesh.triangles.Length / 3;
            target.Fixed = false;
            target.Options = MeshSimplifierOptions.Default;
            return true;
        }

        public bool IsValid()
        {
            if (Renderer == null) return false;
            if (Renderer is not SkinnedMeshRenderer and not MeshRenderer) return false;
            var mesh = Utils.GetMesh(Renderer);
            if (mesh == null) return false;
            if (mesh.triangles.Length < 0) return false;

            return true;
        }

        public bool Enabled() => State == OverallNdmfMeshSimplifierTargetState.Enabled;
    
        public Mesh Process()
        {
            var mesh = Utils.GetMesh(Renderer);

            if (AbsoulteTriangleCount >= mesh.triangles.Length / 3) return UnityEngine.Object.Instantiate(mesh);

            var simplifiedMesh = new Mesh();
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteVertexCount,
                Value = AbsoulteTriangleCount
            };
            MeshSimplifier.Simplify(mesh, target, Options, simplifiedMesh);
            return simplifiedMesh;
        }

        public async Task<Mesh> ProcessAsync(CancellationToken cancellationToken = default)
        {
            var mesh = Utils.GetMesh(Renderer);

            if (AbsoulteTriangleCount >= mesh.triangles.Length / 3) return UnityEngine.Object.Instantiate(mesh);

            var simplifiedMesh = new Mesh();
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteVertexCount,
                Value = AbsoulteTriangleCount
            };
            await MeshSimplifier.SimplifyAsync(mesh, target, Options, simplifiedMesh, cancellationToken);
            return simplifiedMesh;
        }

        public bool Equals(OverallNDMFMeshSimplifierTarget other)
        {
            return State == other.State &&
                   Renderer.Equals(other.Renderer) &&
                   Fixed == other.Fixed &&
                   AbsoulteTriangleCount == other.AbsoulteTriangleCount &&
                   TotalTriangleCount == other.TotalTriangleCount &&
                   Options.Equals(other.Options);

        }

        public override bool Equals(object obj)
        {
            return obj is OverallNDMFMeshSimplifierTarget other && Equals(other);
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