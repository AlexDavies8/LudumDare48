using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitSpriteToScreen : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera = null;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;

    private void Update()
    {
        _spriteRenderer.size = new Vector2(_mainCamera.orthographicSize * 2 * _mainCamera.aspect * 2, _mainCamera.orthographicSize * 2 * 2);   
    }
}
