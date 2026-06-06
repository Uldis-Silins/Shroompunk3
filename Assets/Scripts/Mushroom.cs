using System;
using UnityEngine;
using PrimeTween;
using Random = UnityEngine.Random;

public class Mushroom : MonoBehaviour
{
    [SerializeField] private float m_growRate = 0.5f;

    private float m_growTick;

    private readonly int m_maxGrowth = 100;
    
    public int Growth { get; private set; }
    public MushroomData Data { get; private set; }
    
    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public void Initialize(MushroomData data)
    {
        Tween.Scale(transform, Vector3.zero, Vector3.one, startDelay: Random.Range(0.1f, 0.5f), duration: Random.Range(0.25f, 0.4f), ease: Ease.InOutBounce);
        m_growTick = m_growRate;
        Data = data;
    }

    private void Update()
    {
        if (m_growTick <= 0f && Growth < m_maxGrowth)
        {
            m_growTick = m_growRate;
            Growth++;

            Tween.Scale(transform, transform.localScale, Vector3.one * Mathf.Lerp(1f, 3f, (float)Growth / m_maxGrowth), startDelay: Random.Range(0.1f, 0.5f), duration: Random.Range(0.25f, 0.4f), ease: Ease.OutBounce);
        }
        
        m_growTick -= Time.deltaTime;
    }
}