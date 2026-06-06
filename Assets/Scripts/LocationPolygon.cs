using System;
using System.Collections.Generic;
using Mapbox.BaseModule.Data.Vector2d;
using Mapbox.BaseModule.Utilities;
using Mapbox.LocationModule;
using UnityEngine;

/// <summary>
/// TODO: Check out polylabel for center point inside geo location tile
/// https://github.com/mapbox/polylabel/blob/master/include/mapbox/polylabel.hpp
/// </summary>

[CreateAssetMenu(fileName = "LocationPolygon", menuName = "Data/Location Polygon")]
public class LocationPolygon : ScriptableObject
{
    [field: SerializeField] public List<LatitudeLongitude> Vertices { get; private set; }
    [field: SerializeField] public float TileSize { get; private set; } = 0.5f;
    
    /// <summary>
    /// Polygon should have at least 4 locations to build the grid.
    /// </summary>
    public bool IsValid => Vertices.Count > 3;

    public IEnumerable<Vector3> GetWorldVertices(Location playerLocation)
    {
        List<Vector3> vertices = new List<Vector3>();
        
        Vector3 playerOffset =
            Conversions.LatitudeLongitudeToWorldPosition(playerLocation.LatitudeLongitude, new Vector2d(), 1f);
        
        foreach (var vertex in Vertices)
        {
            vertices.Add(Conversions.LatitudeLongitudeToWorldPosition(vertex, new Vector2d(playerOffset.x, playerOffset.z), 1f));
        }
        
        return vertices;
    }
}