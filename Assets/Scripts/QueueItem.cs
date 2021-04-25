using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueItem : MonoBehaviour
{
    [SerializeField] private int _index = 0;

    [SerializeField] private SpriteRenderer _pivot = null;
    [SerializeField] private SpriteRenderer _outer = null;

    [SerializeField] private GameController _gameController = null;

    [SerializeField] private Sprite[] _sprites = new Sprite[4];

    private void Awake()
    {
        _gameController.OnNextPair += (queue) =>
        {
            var types = queue[_index];
            _pivot.sprite = _sprites[(int)types.Item1];
            _outer.sprite = _sprites[(int)types.Item2];
        };
    }
}
