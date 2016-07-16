using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class OITHelper
{
    public static Mesh fsqMesh // Fullscreen quad mesh
    {
        get
        {
            if (_fsqMesh == null)
            {
                _fsqMesh = new Mesh();
                _fsqMesh.SetVertices(new List<Vector3>(new Vector3[]
                {
                        new Vector3(-1.0f, -1.0f, 0.0f),
                        new Vector3(-1.0f, 1.0f, 0.0f),
                        new Vector3(1.0f, -1.0f, 0.0f),

                        new Vector3(1.0f, -1.0f, 0.0f),
                        new Vector3(-1.0f, 1.0f, 0.0f),
                        new Vector3(1.0f, 1.0f, 0.0f),

                }));
                _fsqMesh.SetIndices(new int[] { 0, 1, 2, 3, 4, 5 }, MeshTopology.Triangles, 0);
                _fsqMesh.SetUVs(0, new List<Vector2>(new Vector2[]
                {
                        new Vector2 (0, 0),
                        new Vector2 (0, 1),
                        new Vector2 (1, 0),

                        new Vector2 (1, 0),
                        new Vector2 (0, 1),
                        new Vector2 (1, 1)
                }));
                _fsqMesh.SetNormals(new List<Vector3>(new Vector3[]
                {
                        Camera.main.ViewportPointToRay(new Vector2(0, 0)).direction.normalized,
                        Camera.main.ViewportPointToRay(new Vector2(0, 1)).direction.normalized,
                        Camera.main.ViewportPointToRay(new Vector2(1, 0)).direction.normalized,

                        Camera.main.ViewportPointToRay(new Vector2(1, 0)).direction.normalized,
                        Camera.main.ViewportPointToRay(new Vector2(0, 1)).direction.normalized,
                        Camera.main.ViewportPointToRay(new Vector2(1, 1)).direction.normalized,
                }));
            }
            return _fsqMesh;
        }
    }
    private static Mesh _fsqMesh;
}
