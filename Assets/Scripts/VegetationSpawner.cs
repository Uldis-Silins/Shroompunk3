using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Random = UnityEngine.Random;

public class VegetationSpawner : MonoBehaviour
{
    public Action<Vector3> onSpawned;   // T1: Spawn location
    
    [SerializeField] private ARPlaneManager m_planeManager;
    
    [SerializeField] private GameObject m_vegetationPrefab;

    private ARPlane m_groundPlane;
    
    private List<ARAnchor> m_spawnedVegetation;
    MeshPointSampler m_pointSampler = new MeshPointSampler();
    
    public bool SpawningEnabled { get; set; }

    private void OnEnable()
    {
        m_planeManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        m_planeManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnDestroy()
    {
        if (m_groundPlane != null)
        {
            m_groundPlane.boundaryChanged -= OnGroundPlaneChanged;
            m_groundPlane = null;
        }
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARPlane> planes)
    {
        foreach (var plane in planes.added)
        {
            //throw new NotImplementedException();
        }

        foreach (var plane in planes.updated)
        {
            if (m_groundPlane == null && plane.size is { x: > 2f, y: > 2f })
            {
                m_groundPlane = plane;
                m_groundPlane.boundaryChanged += OnGroundPlaneChanged;
            }
        }

        foreach (var plane in planes.removed)
        {
            if (m_groundPlane != null && plane.Value == m_groundPlane)
            {
                m_groundPlane.boundaryChanged -= OnGroundPlaneChanged;
                m_groundPlane = null;
            }
        }
    }

    private void OnGroundPlaneChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        if (args.plane != m_groundPlane) return;
        if(!SpawningEnabled) return;

        List<Vector3> positions = m_pointSampler.Generate(args.plane.GetComponent<MeshFilter>());

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
            m_spawnedVegetation.Add(
                Instantiate(m_vegetationPrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f))
                    .AddComponent<ARAnchor>());
            
            onSpawned?.Invoke(pos);
        }
    }
}