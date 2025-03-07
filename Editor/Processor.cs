using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Meshia.MeshSimplification;

namespace com.aoyon.OverallNDMFMeshSimplifiers
{
    // Meshia.MeshSimplificationをラップ
    internal static class Processor
    {
        private static Mesh Simplify(Mesh mesh, MeshSimplificationTarget target, MeshSimplifierOptions options)
        {
            var simplifiedMesh = new Mesh();
            MeshSimplifier.Simplify(mesh, target, options, simplifiedMesh);
            return simplifiedMesh;
        }

        private static async Task<Mesh> SimplifyAsync(Mesh mesh, MeshSimplificationTarget target, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var simplifiedMesh = new Mesh();
            await MeshSimplifier.SimplifyAsync(mesh, target, options, simplifiedMesh, cancellationToken);
            return simplifiedMesh;
        }

        public static Mesh SimplifyRelativeVertexCount(Mesh mesh, float relativeValue, MeshSimplifierOptions options)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.RelativeVertexCount,
                Value = relativeValue
            };
            return Simplify(mesh, target, options);
        }

        public static Task<Mesh> SimplifyRelativeVertexCountAsync(Mesh mesh, float relativeValue, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.RelativeVertexCount,
                Value = relativeValue
            };
            return SimplifyAsync(mesh, target, options, cancellationToken);
        }

        public static Mesh SimplifyAbsoluteVertexCount(Mesh mesh, int value, MeshSimplifierOptions options)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteVertexCount,
                Value = value
            };
            return Simplify(mesh, target, options);
        }

        public static Task<Mesh> SimplifyAbsoluteVertexCountAsync(Mesh mesh, int value, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteVertexCount,
                Value = value
            };
            return SimplifyAsync(mesh, target, options, cancellationToken);
        }

        public static Mesh SimplifyScaledTotalError(Mesh mesh, float value, MeshSimplifierOptions options)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.ScaledTotalError,
                Value = value
            };
            return Simplify(mesh, target, options);
        }

        public static Task<Mesh> SimplifyScaledTotalErrorAsync(Mesh mesh, float value, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.ScaledTotalError,
                Value = value
            };
            return SimplifyAsync(mesh, target, options, cancellationToken);
        }

        public static Mesh SimplifyAbsoluteTotalError(Mesh mesh, float value, MeshSimplifierOptions options)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteTotalError,
                Value = value
            };
            return Simplify(mesh, target, options);
        }

        public static Task<Mesh> SimplifyAbsoluteTotalErrorAsync(Mesh mesh, float value, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteTotalError,
                Value = value
            };
            return SimplifyAsync(mesh, target, options, cancellationToken);
        }

        public static Mesh SimplifyRelativeTriangleCount(Mesh mesh, float relativeValue, MeshSimplifierOptions options)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.RelativeTriangleCount,
                Value = relativeValue
            };
            return Simplify(mesh, target, options);
        }

        public static Task<Mesh> SimplifyRelativeTriangleCountAsync(Mesh mesh, float relativeValue, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.RelativeTriangleCount,
                Value = relativeValue
            };
            return SimplifyAsync(mesh, target, options, cancellationToken);
        }

        public static Mesh SimplifyAbsoluteTriangleCount(Mesh mesh, int polyCount, MeshSimplifierOptions options)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteTriangleCount,
                Value = polyCount
            };
            return Simplify(mesh, target, options);
        }

        public static Task<Mesh> SimplifyAbsoluteTriangleCountAsync(Mesh mesh, int polyCount, MeshSimplifierOptions options, CancellationToken cancellationToken = default)
        {
            var target = new MeshSimplificationTarget()
            {
                Kind = MeshSimplificationTargetKind.AbsoluteTriangleCount,
                Value = polyCount
            };
            return SimplifyAsync(mesh, target, options, cancellationToken);
        }
    }
}