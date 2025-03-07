using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace com.aoyon.OverallNDMFMeshSimplifier
{
    internal class OverallNDMFMeshSimplifierPreview : IRenderFilter
    {
        public static OverallNDMFMeshSimplifierPreview Instance => new();

        public static TogglablePreviewNode ToggleNode = TogglablePreviewNode.Create(
            () => "OverallNDMFMeshSimplifier",
            qualifiedName: "com.aoyon.overall-ndmf-mesh-simplifier-preview",
            true
        );
        
        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return ToggleNode;
        }

        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(ToggleNode.IsEnabled);
        }

        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var groups = new List<RenderGroup>();
            foreach (var component in context.GetComponentsByType<OverallNdmfMeshSimplifier>())
            {
                context.Observe(component, c => c.Targets.Count());
                for (int i = 0; i < component.Targets.Count(); i++)
                {
                    var index = i;
                    var enabled = context.Observe(component, c => c.Targets[index].IsValid() && c.Targets[index].Enabled());
                    if (!enabled) continue;

                    var renderer = component.Targets[i].Renderer;
                    groups.Add(RenderGroup.For(renderer).WithData<(OverallNdmfMeshSimplifier, int)>((component, i)));
                }
            }
            return groups.ToImmutableList();
        }

        public async Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            var data = group.GetData<(OverallNdmfMeshSimplifier, int)>();
            var component = data.Item1;
            var index = data.Item2;

            var proxy = proxyPairs.First().Item2;
            var mesh = Utils.GetMesh(proxy);
            if (mesh == null) return null;

            var target = context.Observe(component, c => c.Targets[index], (a, b) => a.Equals(b));
            
            CancellationTokenSource cts = new();
            context.InvokeOnInvalidate(cts, cts => cts.Cancel());
            var simplifiedMesh = await target.ProcessAsync(mesh);

            return new OverallNDMFMeshSimplifierPreviewNode(simplifiedMesh);
        }
    }

    internal class OverallNDMFMeshSimplifierPreviewNode : IRenderFilterNode
    {
        public RenderAspects WhatChanged => RenderAspects.Mesh;
        private Mesh _modifiedMesh; 

        public OverallNDMFMeshSimplifierPreviewNode(Mesh modifiedMesh)
        {
            _modifiedMesh = modifiedMesh;
        }
        
        public void OnFrame(Renderer original, Renderer proxy)
        {
            Utils.AssignMesh(proxy, _modifiedMesh);
        }
    }
}
