using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Convex hull in XZ axis.
/// </summary>
public class ConvexHull2D
{
    /// <summary>
    /// Set of hull points. Call <see cref="Build"/> before accessing to build and initialize the hull.
    /// </summary>
    public List<Vector3> Points = new List<Vector3>();

    /// <summary>
    /// Build a convex hull from given set of points.
    /// </summary>
    /// <param name="points">Positions in XZ axis space, Y axis is assumed to be on same plane.</param>
    /// <returns>Convex hull of points in XZ axis, access them through <see cref="Points"/> property.</returns>
    public static ConvexHull2D Build(List<Vector3> points)
    {
        ConvexHull2D hull = new ConvexHull2D();
        hull.Points = hull.ComputeHull(points).ToList();
        return hull;
    }
    
    /// <summary>
    /// Compute convex hull using Graham scan
    /// </summary>
    /// <param name="points">2D in XZ axis, Y is always 0</param>
    /// <returns>Collection of convex hull points.</returns>
    private IEnumerable<Vector3> ComputeHull(List<Vector3> points)
    {
        if (points.Count <= 3) return new List<Vector3>(points);
        
        Vector3 pivot = points.OrderBy(p => p.y).ThenBy(p => p.x).First();
        
        List<Vector3> sorted = points
            .Where(p => p != pivot)
            .OrderBy(p => Mathf.Atan2(p.z - pivot.z, p.x - pivot.x))
            .ThenBy(p => Vector3.Distance(pivot, p))
            .ToList();

        Stack<Vector3> hull = new Stack<Vector3>();

        hull.Push(pivot);
        hull.Push(sorted[0]);
        hull.Push(sorted[1]);

        for (int i = 2; i < sorted.Count; i++)
        {
            Vector3 top = hull.Pop();

            while (hull.Count > 0 && GetDirection(hull.Peek(), top, sorted[i]) <= 0)
            {
                top = hull.Pop();
            }

            hull.Push(top);
            hull.Push(sorted[i]);
        }

        return hull.Reverse().ToList();
    }
    
    /// <summary>
    /// Is the <paramref name="point"/> inside this hull?
    /// </summary>
    /// <param name="point">Position in XZ axis, Y is assumed to be the same as the hull plane y.</param>
    /// <returns></returns>
    public bool IsPointInsideHull(Vector3 point)
    {
        int count = Points.Count;

        for (int i = 0; i < count; i++)
        {
            Vector3 a = Points[i];
            Vector3 b = Points[(i + 1) % count];

            float direction = GetDirection(a, b, point);

            if (direction < 0) return false;
        }

        return true;
    }
    
    public float DistanceToHull(Vector3 point)
    {
        Vector2 p = new Vector2(point.x, point.z);
        
        if (IsPointInsideHull(p)) return 0f;

        float minDistSq = float.PositiveInfinity;

        for (int i = 0; i < Points.Count; i++)
        {
            Vector2 closest = ClosestPointOnSegment(p, Points[i], Points[(i + 1) % Points.Count]);

            float distSq = (p - closest).sqrMagnitude;

            if (distSq < minDistSq) minDistSq = distSq;
        }

        return Mathf.Sqrt(minDistSq);
    }

    
    /// <summary>
    /// Returns the winding order of the supplied points inside the hull.
    /// </summary>
    /// <returns>1 if the point is inside the hull wind direction.</returns>
    private float GetDirection(Vector3 a, Vector3 b, Vector3 c)
    {
        return (b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x);
    }

    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;

        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return a + ab * t;
    }
}