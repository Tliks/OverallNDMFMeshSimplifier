using UnityEngine;
using nadena.dev.ndmf;
using com.aoyon.OverallNDMFMeshSimplifier;

[assembly: ExportsPlugin(typeof(PluginDefinition))]

namespace com.aoyon.OverallNDMFMeshSimplifier
{
    public class PluginDefinition : Plugin<PluginDefinition>
    {
        public override string QualifiedName => "com.aoyon.overall-ndmf-mesh-simplifier";

        public override string DisplayName => "OverallNDMFMeshSimplifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Optimizing)
            .AfterPlugin("net.rs64.tex-trans-tool")
            .BeforePlugin("com.anatawa12.avatar-optimizer")
            .Run("OverallNDMFMeshSimplifier", ctx => 
            {
                foreach (var component in ctx.AvatarRootObject.GetComponentsInChildren<OverallNdmfMeshSimplifier>(true))
                {
                    if (component.gameObject.activeInHierarchy)
                    {
                        foreach (var target in component.Targets)
                        {
                            if (target.IsValid() && target.Enabled())
                            {
                                var modifiedMesh = target.Process();
                                Utils.AssignMesh(target.Renderer, modifiedMesh);
                            }
                        }
                    }
                    Object.DestroyImmediate(component, true);
                }
            }).PreviewingWith(new OverallNDMFMeshSimplifierPreview());
        }
    }
}