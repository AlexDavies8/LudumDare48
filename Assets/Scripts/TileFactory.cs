using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFactory : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab = null;
    [SerializeField] private Sprite[] _tileSprites = null;
    [SerializeField] private Transform _tileContainer = null;

    public Tile CreateTile(Vector2Int gridPosition, Tile.TileType type)
    {
        GameObject tileGO = Instantiate(_tilePrefab, _tileContainer);
        Tile tile = tileGO.GetComponent<Tile>();
        tile.Type = type;
        tile.MoveTo(gridPosition);
        if ((int)type < _tileSprites.Length)
            tileGO.GetComponent<SpriteRenderer>().sprite = _tileSprites[(int)type];
        return tile;
    }

    public Tile CreateTile(Vector2Int gridPosition, Tile.TileType type, GameObject prefabOverride)
    {
        GameObject tileGO = Instantiate(prefabOverride, _tileContainer);
        Tile tile = tileGO.GetComponent<Tile>();
        tile.Type = type;
        tile.MoveTo(gridPosition);
        if ((int)type < _tileSprites.Length)
            tileGO.GetComponent<SpriteRenderer>().sprite = _tileSprites[(int)type];
        return tile;
    }

    public Pair CreatePair(Vector2Int gridPosition, Tile.TileType pivotType, Tile.TileType outerType)
    {
        Pair pair = new Pair();
        pair.Pivot = CreateTile(gridPosition, pivotType);
        pair.Outer = CreateTile(gridPosition + Vector2Int.right, outerType);
        pair.Rotation = 1;

        return pair;
    }
}
