using UnityEngine;

namespace com.aoyon.OverallNDMFMeshSimplifier
{
    public class Utils
    {
        public static Mesh GetMesh(Renderer renderer)
        {
            switch (renderer)
            {
                case MeshRenderer meshrenderer:
                    var meshfilter = meshrenderer.GetComponent<MeshFilter>();
                    return meshfilter?.sharedMesh;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    return skinnedMeshRenderer.sharedMesh;
                default:
                    return null;
            }
        } 
        
        public static void AssignMesh(Renderer renderer, Mesh mesh)
        {
            switch (renderer)
            {
                case MeshRenderer meshrenderer:
                    var meshfilter = meshrenderer.GetComponent<MeshFilter>();
                    if (meshfilter == null) return;
                    meshfilter.sharedMesh = mesh;
                    break;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.sharedMesh = mesh;
                    break;
            }
        }

        public static bool IsEditorOnlyInHierarchy(GameObject gameObject)
        {
            Transform current = gameObject.transform;
            while (current != null)
            {
                if (current.tag == "EditorOnly")
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }
    }
}