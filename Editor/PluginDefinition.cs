using UnityEngine;
using nadena.dev.ndmf;
using com.aoyon.OverallNDMFMeshSimplifiers;

[assembly: ExportsPlugin(typeof(PluginDefinition))]

namespace com.aoyon.OverallNDMFMeshSimplifiers
{
    public class PluginDefinition : Plugin<PluginDefinition>
    {
        public override string QualifiedName => "com.aoyon.overall-ndmf-mesh-simplifier";

        public override string DisplayName => "OverallNDMFMeshSimplifier";

        protected override void Configure()
        {
            var sequence =
                InPhase(BuildPhase.Transforming);

            sequence
            .Run("OverallNDMFMeshSimplifier", ctx => 
            {
                foreach (var component in ctx.AvatarRootObject.GetComponentsInChildren<OverallNdmfMeshSimplifier>(true))
                {
                    foreach (var target in component.Targets)
                    {
                        if (target.IsValid())
                        {
                            var mesh = Utils.GetMesh(target.Renderer);
                            if (mesh == null) continue;
                            var modifiedMesh = Processor.SimplifyAbsoluteTriangleCount(mesh, target.AbsoulteTriangleCount, target.Options);
                            Utils.AssignMesh(target.Renderer, modifiedMesh);
                        }
                    }
                    Object.DestroyImmediate(component, true);
                }
            }).PreviewingWith(OverallNDMFMeshSimplifierPreview.Instance);
        }
    }
}