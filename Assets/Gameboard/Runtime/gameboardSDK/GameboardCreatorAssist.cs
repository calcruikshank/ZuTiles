using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public static class GameboardCreatorAssist
    {
        public static Mesh BuildMeshFromTrackedObject(TrackedBoardObject boardObject, Transform spaceTransform, Mesh inMesh)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for(int i = 0; i < boardObject.contourWorldVectors3D.Length; i++)
            {
                Vector3 localPoint = spaceTransform.InverseTransformPoint(boardObject.contourWorldVectors3D[i]);

                vertices.Add(localPoint);
                vertices.Add(new Vector3(localPoint.x, localPoint.y + 5f, localPoint.z));
            }

            for(int i = 0; i < vertices.Count - 3; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

                triangles.Add(i + 2);
                triangles.Add(i + 3);
                triangles.Add(i + 1);
            }

            // Connect the end to the start
            int vertCount = vertices.Count - 1;
            triangles.Add(vertCount - 1);
            triangles.Add(vertCount);
            triangles.Add(0);

            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(vertCount);


            // Build the mesh
            inMesh.vertices = vertices.ToArray();
            inMesh.triangles = triangles.ToArray();

            inMesh.RecalculateNormals();

            return inMesh;
        }
    }
}