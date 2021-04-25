using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transitioner : MonoBehaviour
{
    [SerializeField] private float _transitionTime = 1f;
    [SerializeField] private Material _material = null;

    private void Awake()
    {
        StartCoroutine(TransitionFromCoroutine());
    }

    public void TransitionTo(string sceneName)
    {
        StartCoroutine(TransitionToCoroutine(sceneName));
    }

    IEnumerator TransitionToCoroutine(string sceneName)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / _transitionTime)
        {
            _material.SetFloat("_ClipThreshold", 1 - t);
            yield return null;
        }
        _material.SetFloat("_ClipThreshold", 0);

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator TransitionFromCoroutine()
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / _transitionTime)
        {
            _material.SetFloat("_ClipThreshold", t);
            yield return null;
        }
        _material.SetFloat("_ClipThreshold", 1);
    }
}
