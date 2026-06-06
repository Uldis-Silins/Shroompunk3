using System;
using PrimeTween;
using UnityEngine;
using Random = UnityEngine.Random;

public class Vegetation : MonoBehaviour
{
    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        Tween.Scale(transform, Vector3.zero, Vector3.one * Random.Range(1.5f, 2.25f), startDelay: Random.Range(0.1f, 0.5f), duration: Random.Range(0.25f, 0.4f), ease: Ease.InOutBounce);
    }
}