using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;

public class VegetationSpawner : MonoBehaviour
{
    public Action<Vector3> onSpawned;   // T1: Spawn location
    
    [SerializeField] private ARPlaneManager m_planeManager;
    [SerializeField] private ARCameraManager m_cameraManager;
    [SerializeField] private GameObject m_vegetationPrefab;
    [SerializeField] private Camera m_arCamera;

    //public RawImage camImage; // Use for debugging green color detection
    
    private List<ARAnchor> m_spawnedVegetation;
    private MeshPointSampler m_pointSampler = new MeshPointSampler();
    
    private Texture2D m_texture;
    private bool m_texReceived;
    
    List<ARPlane> m_planeList = new List<ARPlane>();
    
    public bool SpawningEnabled { get; set; }

    private void OnEnable()
    {
        m_planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
        
        m_cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable()
    {
        m_planeManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        
        m_cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> planes)
    {
        foreach (var plane in planes.added)
        {
            if (!m_planeList.Contains(plane))
            {
                m_planeList.Add(plane);
                plane.boundaryChanged += OnGroundPlaneChanged;
                var meshFilter = plane.gameObject.GetComponent<MeshFilter>();

                if (meshFilter != null)
                {
                    SpawnVegetation(meshFilter);
                }
            }
        }

        foreach (var plane in planes.updated)
        {
            if (!m_planeList.Contains(plane))
            {
                m_planeList.Add(plane);
                plane.boundaryChanged += OnGroundPlaneChanged;
            }
        }

        foreach (var plane in planes.removed)
        {
            if (m_planeList.Contains(plane.Value))
            {
                plane.Value.boundaryChanged -= OnGroundPlaneChanged;
                m_planeList.Remove(plane.Value);
            }
        }
    }

    private void OnGroundPlaneChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        var meshFilter = args.plane.GetComponent<MeshFilter>();

        SpawnVegetation(meshFilter);
    }

    private void SpawnVegetation(MeshFilter meshFilter)
    {
        if(!SpawningEnabled) return;

        List<Vector3> positions = m_pointSampler.Generate(meshFilter);
        if(positions == null) return;

        if (m_spawnedVegetation == null)
        {
            m_spawnedVegetation = new List<ARAnchor>();
        }

        foreach (var pos in m_spawnedVegetation)
        {
            positions.RemoveAll(x => Vector3.Distance(pos.transform.position, x) < 1f);
        }

        foreach (var pos in positions)
        {
            bool canSpawn = true;

            var pixels = m_texture.GetPixels32();
            var changed = new Color32[pixels.Length];
            Array.Copy(pixels, changed, changed.Length);

            //Vector2 screenPos = m_arCamera.WorldToScreenPoint(pos);

            // if (screenPos.x >= 0 && screenPos.x < m_texture.width && screenPos.y >= 0 && screenPos.y < m_texture.height)
            // {
            //     int texX = Mathf.RoundToInt(screenPos.x / Screen.width * m_texture.width);
            //     int texY = Mathf.RoundToInt(screenPos.y / Screen.height * m_texture.height);
            //     int index = texY * m_texture.height + texX;
            //     
            //     if (index > 0 && index < pixels.Length)
            //     {
            //         Color32 p = pixels[index];
            //         bool isGreen = p.g > p.r && p.g > p.b;
            //
            //         if (isGreen)
            //         {
            //             canSpawn = true;
            //         }
            //     }
            // }

            if (canSpawn)
            {
                m_spawnedVegetation.Add(
                    Instantiate(m_vegetationPrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f))
                        .AddComponent<ARAnchor>());

                onSpawned?.Invoke(pos);
            }
        }
    }

    public void DespawnVegetation()
    {
        for (int i = m_spawnedVegetation.Count; --i >= 0;)
        {
            Destroy(m_spawnedVegetation[i].gameObject);
        }
        
        m_spawnedVegetation.Clear();
    }

    private unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (m_cameraManager.TryAcquireLatestCpuImage(out var image))
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions =  new Vector2Int(image.width / 2, image.height / 2),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorX
            };
            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
            image.Dispose();

            if (m_texture == null)
            {
                m_texture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y,
                    TextureFormat.RGBA32, false);
            }

            m_texture.LoadRawTextureData(buffer);

            m_texture.Apply();
            buffer.Dispose();
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            m_texture = RotateTexture90(m_texture);
            #endif

            //camImage.texture = m_texture;
        }
    }

    [Obsolete("Use for debugging green color detection")]
    private IEnumerator ChangeColor()
    {
        yield return null;
        
        var pixels = m_texture.GetPixels32();
        var changed = new Color32[pixels.Length];
        Array.Copy(pixels, changed, changed.Length);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            bool isGreen = p.g > p.r && p.g > p.b;
            
            if (isGreen)
            {
                changed[i] = new Color32(255, 0, 255, 255);
            }
        }

        m_texture.SetPixels32(changed);
        m_texture.Apply();
    }
    
    public static Texture2D RotateTexture90(Texture2D source)
    {
        int width = source.width;
        int height = source.height;

        Texture2D result = new Texture2D(height, width);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result.SetPixel(y, width - x - 1, source.GetPixel(x, y));
            }
        }

        result.Apply();
        return result;
    }
}