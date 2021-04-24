using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; set; }
    public TileType Type { get; set; }

    public void Move(Vector2Int offset)
    {
        GridPosition += offset;
        transform.localPosition = (Vector2)GridPosition;
    }

    public void MoveTo(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        transform.localPosition = (Vector2)GridPosition;
    }

    public enum TileType
    {
        Red,
        Blue,
        Orange,
        Green,
        Ground
    }
}
