using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    [SerializeField] private Transform _target = null;
    [SerializeField] private float _recoveryRate = 1f;
    [SerializeField] private float _maximumMagnitude = 1f;
    [SerializeField] private float _frequency = 10f;

    public float Trauma { get; private set; }

    public void SetTrauma(float amount)
    {
        if (amount > Trauma)
            Trauma = Mathf.Min(amount, 1f);
    }

    public void AddTrauma(float amount)
    {
        Trauma = Trauma + Mathf.Min((1 - Trauma * Trauma) * amount, 1f);
    }

    private void Update()
    {
        float xShake = (Mathf.PerlinNoise(Time.time * _frequency, Mathf.PI) * 2 - 1) * Trauma * Trauma * _maximumMagnitude;
        float yShake = (Mathf.PerlinNoise(Mathf.PI, Time.time * _frequency) * 2 - 1) * Trauma * Trauma * _maximumMagnitude;
        _target.transform.localPosition = new Vector2(xShake, yShake);

        Trauma = Mathf.Clamp01(Trauma - _recoveryRate * Time.deltaTime);
    }
}
