using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsExtensions
{
    /// <summary>
    /// Calculates the surface normal for CapsuleCast and SphereCast
    /// </summary>
    /// <param name="hit">original hit</param>
    /// <param name="dir">original direction of the raycast</param>
    /// <returns>The correct normal</returns>
    public static Vector3 GetFaceNormal(this RaycastHit hit, Vector3 dir)
    {
        if (hit.collider is MeshCollider)
        {
            var collider = hit.collider as MeshCollider;
            var mesh = collider.sharedMesh;
            var tris = mesh.triangles;
            var verts = mesh.vertices;

            var v0 = verts[tris[hit.triangleIndex * 3]];
            var v1 = verts[tris[hit.triangleIndex * 3 + 1]];
            var v2 = verts[tris[hit.triangleIndex * 3 + 2]];

            var n = Vector3.Cross(v1 - v0, v2 - v1).normalized;

            return hit.transform.TransformDirection(n);
        }
        else
        {
            hit.collider.Raycast(new Ray(hit.point - dir * 0.01f, dir), out RaycastHit result, 0.011f);
            return result.normal;
        }
    }

}
