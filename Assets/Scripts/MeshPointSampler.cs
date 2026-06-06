using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class MeshPointSampler
{
    private readonly int m_seed = 12345;

    public List<Vector3> Generate(MeshFilter meshFilter, float minDistance = 1.5f,
        float maxDistance = 5.0f, int targetPointCount = 200, int maxAttempts = 1000)
    {
        Mesh mesh = meshFilter.sharedMesh;
        List<Vector3> points = new List<Vector3>();

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Precompute triangle areas
        float[] cumulativeAreas = new float[triangles.Length / 3];
        float totalArea = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = vertices[triangles[i]];
            Vector3 b = vertices[triangles[i + 1]];
            Vector3 c = vertices[triangles[i + 2]];

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;

            totalArea += area;
            cumulativeAreas[i / 3] = totalArea;
        }

        int attempts = 0;

        while (points.Count < targetPointCount && attempts < maxAttempts)
        {
            attempts++;
            
            float r = Random.value * totalArea;

            int triIndex = 0;

            for (int i = 0; i < cumulativeAreas.Length; i++)
            {
                if (r <= cumulativeAreas[i])
                {
                    triIndex = i * 3;
                    break;
                }
            }

            Vector3 v0 = vertices[triangles[triIndex]];
            Vector3 v1 = vertices[triangles[triIndex + 1]];
            Vector3 v2 = vertices[triangles[triIndex + 2]];
            
            Vector3 localPoint = RandomPointInTriangle(v0, v1, v2);

            Vector3 worldPoint = meshFilter.transform.TransformPoint(localPoint);

            bool isValid = true;
            bool hasNeighbor = false;

            foreach (Vector3 p in points)
            {
                float d = Vector3.Distance(worldPoint, p);

                if (d < minDistance)
                {
                    isValid = false;
                    break;
                }

                if (d <= maxDistance)
                {
                    hasNeighbor = true;
                }
            }

            // Optional connectivity rule
            if (points.Count > 0 && !hasNeighbor)
            {
                isValid = false;
            }

            if (isValid)
            {
                points.Add(worldPoint);
            }
        }

        return points;
    }

    private Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        
        return (1 - r1) * a + (r1 * (1 - r2)) * b + (r1 * r2) * c;
    }
}
