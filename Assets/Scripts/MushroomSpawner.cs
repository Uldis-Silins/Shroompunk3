using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Mapbox.LocationModule;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

public class MushroomSpawner : MonoBehaviour
{
    #region Internal classes

    [Serializable]
    public class Cell
    {
        private float m_humidity;

        public float Humidity
        {
            get => m_humidity;
            set => m_humidity = Mathf.Clamp(value, 0.0f, 100.0f);
        }

        private float m_temperature;

        public float Temperature
        {
            get => m_temperature;
            set => m_temperature = Mathf.Clamp(value, 0.0f, 100.0f);
        }

        private float m_shade;

        public float Shade
        {
            get => m_shade;
            set => m_shade = Mathf.Clamp(value, 0.0f, 100.0f);
        }

        public Vector3 WorldPosition { get; private set; }

        public Cell(Vector3 worldPosition, float humidity, float temperature, float shade)
        {
            WorldPosition = worldPosition;
            Humidity = humidity;
            Temperature = temperature;
            Shade = shade;
        }
    }

    [Serializable]
    public class SpawnPosition
    {
        public float spawnChance;
        public MushroomData mushroomData;
        public Mushroom spawnedObject;
        public Vector3 spawnPosition;
        
        public bool IsSpawned => spawnedObject != null;
    }

    #endregion // Internal classes

    [SerializeField] private LocationPolygon m_locationData;
    [SerializeField] private LocationProviderFactory m_locationProvider;
    
    [SerializeField] private VegetationSpawner m_vegetationSpawner;
    
    [SerializeField] private MushroomData[] m_mushroomData;
    
    [SerializeField] private AnimationCurve m_minSpawnChanceCurve;
    [SerializeField] private AnimationCurve m_maxSpawnChanceCurve;

    public Texture2D humidityTexture;
    public Texture2D temperatureTexture;
    public Texture2D shadeTexture;
    public Material areaMaterial;

    private Texture2D m_currentTexture;

    private Cell[,] m_grid;
    private MeshRenderer m_gridMesh;
    private ConvexHull2D m_hull;
    
    private List<SpawnPosition> m_spawnPoints;
    private float m_maxSpawnChance = 100f;
    private float m_minSpawnChance = 80f;

    private float m_spawnTick;
    private float m_spawnTickRate = 2f;
    
    private List<Mushroom> m_spawnedMushrooms = new List<Mushroom>();

    public bool IsAreaInitialized => m_grid is { Length: > 0 };
    public bool SpawningEnabled { get; set; }

    private void Awake()
    {
        m_spawnPoints = new List<SpawnPosition>();
    }

    private IEnumerator Start()
    {
        while (m_locationProvider.DefaultLocationProvider == null && !m_locationProvider.IsLocationProviderReady)
            yield return null;
    }

    private void OnEnable()
    {
        m_vegetationSpawner.onSpawned += OnVegetationSpawned;
    }

    private void OnDisable()
    {
        m_vegetationSpawner.onSpawned -= OnVegetationSpawned;
    }

    // private void OnDrawGizmos()
    // {
    //     Color prevColor = Gizmos.color;
    //     Gizmos.color = Color.red;
    //     if (m_spawnPoints != null && m_spawnPoints.Count > 0)
    //     {
    //         foreach (Vector3 spawnPoint in m_spawnPoints)
    //         {
    //             Gizmos.DrawSphere(spawnPoint, 0.1f);
    //         }
    //     }
    //     Gizmos.color = prevColor;
    // }


    private void Update()
    {
        if(!SpawningEnabled) return;
        
#if UNITY_EDITOR
        if (m_grid != null && Input.GetKeyDown(KeyCode.Space))
        {
            if (m_currentTexture == humidityTexture) m_currentTexture = temperatureTexture;
            else if (m_currentTexture == temperatureTexture) m_currentTexture = shadeTexture;
            else m_currentTexture = humidityTexture;

            m_gridMesh.material.SetTexture("_BaseMap", m_currentTexture);
        }
#endif  // UNITY_EDITOR

        if (m_spawnTick <= 0f)
        {
            foreach (SpawnPosition spawnPoint in m_spawnPoints)
            {
                spawnPoint.spawnChance++;

                if (!spawnPoint.IsSpawned && spawnPoint.spawnChance >= 100f)
                {
                    var instance = Instantiate(spawnPoint.mushroomData.CutTopPrefab,
                        spawnPoint.spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    Mushroom shroom = instance.AddComponent<Mushroom>();
                    shroom.gameObject.AddComponent<ARAnchor>();
                    spawnPoint.spawnedObject = shroom;
                    shroom.Initialize(spawnPoint.mushroomData);
                    m_spawnedMushrooms.Add(shroom);
                }
            }
            
            m_spawnTick = m_spawnTickRate;
        }
        
        m_spawnTick -= Time.deltaTime;
    }

    // Returns: grid positions of the supplied location area
    [Obsolete("Not yet implemented")]
    private Cell[,] InitializeArea(LocationPolygon location)
    {
        if (!location.IsValid) throw new Exception("Invalid location data");

        List<Vector3> verts =
            new List<Vector3>(location.GetWorldVertices(m_locationProvider.DefaultLocationProvider.CurrentLocation));

        GameObject gridMesh = new GameObject("GridMesh");
        MeshFilter meshFilter = gridMesh.AddComponent<MeshFilter>();
        m_gridMesh = gridMesh.AddComponent<MeshRenderer>();

        IOrderedEnumerable<Vector3> vertsSorted = verts.OrderBy(v => v.x).ThenBy(v => v.z);

        Vector3 pivotOffset = vertsSorted.Last();
        pivotOffset.y = 0;
        Vector3[] meshVertices = vertsSorted.ToArray();

        for (int i = 0; i < meshVertices.Length; i++)
        {
            meshVertices[i] -= pivotOffset;
        }

        Mesh mesh = new Mesh { name = "GridMesh" };
        meshFilter.mesh = mesh;
        mesh.vertices = meshVertices;
        mesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
        mesh.normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        mesh.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
        m_gridMesh.material = areaMaterial;

        m_gridMesh.transform.position += pivotOffset;

        Bounds bounds = new Bounds(verts[0], Vector3.zero);
        verts.ForEach(v => bounds.Encapsulate(v));
        m_hull = ConvexHull2D.Build(verts);

        Vector3 minPos = bounds.min;
        float distance = location.TileSize;
        int xPositionCount = (int)(bounds.size.x / distance);
        int yPositionCount = (int)(bounds.size.z / distance);
        Cell[,] grid = new Cell[xPositionCount, yPositionCount];

        for (int x = 0; x < xPositionCount; x++)
        {
            for (int y = 0; y < yPositionCount; y++)
            {
                Vector3 position = minPos + new Vector3(x * distance, 0, y * distance);

                if (m_hull.IsPointInsideHull(position))
                {
                    grid[x, y] = new Cell(position, Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f),
                        Random.Range(0.0f, 100.0f));
                }
            }
        }
        
        int texWidth = (int)Vector3.Distance(meshVertices[1], meshVertices[2]);
        int texHeight = (int)Vector3.Distance(meshVertices[0], meshVertices[1]);

        humidityTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false)
        {
            name = "HumidityTexture",
            filterMode = FilterMode.Point
        };
        temperatureTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false)
        {
            name = "TemperatureTexture",
            filterMode = FilterMode.Point
        };
        shadeTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false)
        {
            name = "ShadeTexture",
            filterMode = FilterMode.Point
        };
        
        for (int x = 0; x < xPositionCount; x++)
        {
            for (int y = 0; y < yPositionCount; y++)
            {
                if (grid[x, y] != null)
                {
                    Vector2 uv = GetPointInMesh(m_gridMesh, mesh, grid[x, y].WorldPosition);
                    int texX = Mathf.FloorToInt(uv.x * texWidth);
                    int texY = Mathf.FloorToInt(uv.y * texHeight);
                    humidityTexture.SetPixel(texX, texY, new Color(grid[x,y].Humidity / 100f, 0f, 0f));
                    temperatureTexture.SetPixel(texX, texY, new Color(0f, grid[x, y].Temperature / 100f, 0f));
                    shadeTexture.SetPixel(texX, texY, new Color(0f, 0f, grid[x, y].Humidity / 100f));
                }
            }
        }
        
        humidityTexture.Apply();
        temperatureTexture.Apply();
        shadeTexture.Apply();
        
        m_currentTexture = humidityTexture;
        m_gridMesh.material.SetTexture("_BaseMap", m_currentTexture);

        return grid;
    }

    [Obsolete("Not yet implemented")]
    private bool SamplePosition(Cell[,] grid, ConvexHull2D hull, Vector3 position, out Cell cell, float cellSize = 0.5f)
    {
        cell = null;
        float closestDistance = float.MaxValue;

        if (!hull.IsPointInsideHull(position)) return false;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y] != null)
                {
                    float dist = Vector3.Distance(grid[x, y].WorldPosition, position);

                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        cell = grid[x, y];
                    }
                }
            }
        }

        return true;
    }

    // Returns: uv location in mesh
    [Obsolete("Not yet implemented")]
    private Vector2 GetPointInMesh(MeshRenderer meshRenderer, Mesh mesh, Vector3 position)
    {
        Vector3 localPos = meshRenderer.transform.InverseTransformPoint(position);

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] tris = mesh.triangles;
        
        float closestDist = float.MaxValue;
        int triangleIndex = -1;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 a = vertices[tris[i]];
            Vector3 b = vertices[tris[i + 1]];
            Vector3 c = vertices[tris[i + 2]];

            Vector3 closest = GetPointInTriangle(localPos, a, b, c);

            float dist = (localPos - closest).sqrMagnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                triangleIndex = i / 3;
            }
        }
        
        int tri = triangleIndex * 3;

        Vector3 v0 = vertices[tris[tri]];
        Vector3 v1 = vertices[tris[tri + 1]];
        Vector3 v2 = vertices[tris[tri + 2]];

        Vector3 coord = GetPointInTriangle(localPos, v0, v1, v2);

        return uvs[tris[tri]] * coord.x + uvs[tris[tri + 1]] * coord.y + uvs[tris[tri + 2]] * coord.z;
    }

    [Obsolete("Not yet implemented")]
    private Vector3 GetPointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = p - a;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;

        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        return new Vector3(u, v, w);
    }

    private void OnVegetationSpawned(Vector3 position)
    {
        var positions = GetSpawnMushroomPositions(position);
        List<SpawnPosition> spawns = new List<SpawnPosition>();

        foreach (var pos in positions)
        {
            MushroomData data = m_mushroomData[Random.Range(0, m_mushroomData.Length)];
            float chance = Mathf.Clamp(Random.Range(m_minSpawnChance, m_maxSpawnChance), 1f, 100f);
            //if (m_minSpawnChance > 1f) m_minSpawnChance = Mathf.Clamp(m_minSpawnChance - 3f, 1f, 100f);
            //if (m_maxSpawnChance > 1f) m_maxSpawnChance = Mathf.Clamp(m_maxSpawnChance - 1f, 1f, 100f);
            SpawnPosition spawn = new SpawnPosition() { mushroomData = data, spawnPosition = pos, spawnChance = chance };
            spawns.Add(spawn);
            Debug.Log($"Added spawn: {spawn}");
        }
        
        m_spawnPoints.AddRange(spawns);
    }

    public void DespawnMushrooms()
    {
        for (int i = m_spawnedMushrooms.Count; --i >= 0;)
        {
            if (m_spawnedMushrooms[i] != null)
            {
                Destroy(m_spawnedMushrooms[i].gameObject);
            }
        }
        
        m_spawnedMushrooms.Clear();
    }

    public void SetSpawnChance(float time)
    {
        m_minSpawnChance = m_minSpawnChanceCurve.Evaluate(time) * 100f;
        m_maxSpawnChance = m_maxSpawnChanceCurve.Evaluate(time) * 100f;
    }
    
    public IEnumerable<Vector3> GetSpawnMushroomPositions(Vector3 position)
    {
        List<Vector3> positions = new List<Vector3>();
        int count = Random.Range(1, 6);

        for (int i = 0; i <= count; i++)
        {
            float angle = i * (360f / count);
            const float maxSpawnDist = 0.5f;
            Vector3 offset = new Vector3(Random.insideUnitCircle.x * 0.25f, 0f, Random.insideUnitCircle.y * 0.25f);
            positions.Add(position + Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * maxSpawnDist) + offset);
        }
        
        return positions;
    }

    public void ResetSpawn()
    {
        m_spawnTick = 0f;
        m_spawnPoints.Clear();
    }

    public void PickupMushroom(MushroomData data)
    {
        for (int i = m_spawnPoints.Count - 1; --i >= 0;)
        {
            if (m_spawnPoints[i].mushroomData == data)
            {
                m_spawnPoints.Remove(m_spawnPoints[i]);
            }
        }
    }
}