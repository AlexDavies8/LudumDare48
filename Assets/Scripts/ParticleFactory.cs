using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFactory : MonoBehaviour
{
    [SerializeField] private Transform _particleContainer = null;

    [SerializeField] private GameObject _seekPrefab = null;
    [SerializeField] private GameObject _explosionPrefab = null;
    [SerializeField] private GameObject _energyBallPrefab = null;

    public ParticlePath CreateSeekParticle(Vector2 position, Vector2 target, float time)
    {
        GameObject seekGO = Instantiate(_seekPrefab, _particleContainer);
        ParticlePath seek = seekGO.GetComponent<ParticlePath>();
        seek.transform.position = position;
        seek.SetPath(target, time);
        return seek;
    }

    public GameObject CreateExplosionParticle(Vector2 position)
    {
        GameObject explosionGO = Instantiate(_explosionPrefab, _particleContainer);
        explosionGO.transform.localPosition = position;
        return explosionGO;
    }

    public GameObject CreateEnergyBallParticle(Vector2 position)
    {
        GameObject energyBallGO = Instantiate(_energyBallPrefab, _particleContainer);
        energyBallGO.transform.position = position;
        energyBallGO.transform.localScale = Vector3.zero;
        return energyBallGO;
    }
}
