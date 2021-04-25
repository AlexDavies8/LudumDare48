using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int _gridSize;
    //[SerializeField] private Transform _tileContainer = null;
    public Tile[,] Grid { get; private set; }
    public Vector2Int GridSize { get => _gridSize; set => _gridSize = value; }
    public Action<List<List<Vector2Int>>> ExplodeChainsCallback { get; set; }

    private void Awake()
    {
        Grid = new Tile[GridSize.x, GridSize.y];
    }

    public void StepGrid()
    {
        for (int y = 1; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                if (Grid[x, y] != null && Grid[x, y - 1] == null)
                {
                    Grid[x, y - 1] = Grid[x, y];
                    Grid[x, y] = null;
                    Grid[x, y - 1].Move(Vector2Int.down);
                }
            }
        }
    }

    public bool StepPair(Pair pair)
    {
        if (pair.Pivot.GridPosition.y - 1 < 0 || pair.Outer.GridPosition.y - 1 < 0)
        {
            DecomposePair(pair);
            return false;
        }
        if (Grid[pair.Pivot.GridPosition.x, pair.Pivot.GridPosition.y - 1] == null && Grid[pair.Outer.GridPosition.x, pair.Outer.GridPosition.y - 1] == null)
        {
            pair.Pivot.Move(Vector2Int.down);
            pair.Outer.Move(Vector2Int.down);
            return true;
        }
        DecomposePair(pair);
        return false;
    }

    public void DestroyTileAt(Vector2Int pos, float timeDelay = 0f)
    {
        Tile tile = Grid[pos.x, pos.y];
        Grid[pos.x, pos.y] = null;
        Destroy(tile.gameObject, timeDelay);
    }

    public bool IsGridStable()
    {
        for (int y = 1; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                if (Grid[x, y] != null && Grid[x, y - 1] == null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool ExplodeChains()
    {
        List<Vector2Int> visited = new List<Vector2Int>();
        List<List<Vector2Int>> chains = new List<List<Vector2Int>>();
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                if (Grid[x, y] == null || Grid[x, y] == null || visited.Contains(new Vector2Int(x, y)) || (int)Grid[x, y].Type >= (int)Tile.TileType.Ground) continue;

                List<Vector2Int> chain = new List<Vector2Int>();
                RecurseChain(new Vector2Int(x, y), visited, chain);
                chains.Add(chain);
            }
        }

        ExplodeChainsCallback?.Invoke(chains);

        foreach (List<Vector2Int> chain in chains)
            if (chain.Count >= 4) return true;
        return false;
    }

    void RecurseChain(Vector2Int position, List<Vector2Int> visited, List<Vector2Int> chain)
    {
        visited.Add(position);
        chain.Add(position);
        foreach (Vector2Int offset in RotationOffsets)
        {
            Vector2Int checkPos = position + offset;
            if (!InsideGrid(checkPos) || Grid[checkPos.x, checkPos.y] == null || Grid[checkPos.x, Mathf.Max(checkPos.y - 1, 0)] == null)
                continue;
            if (Grid[checkPos.x, checkPos.y].Type == Grid[position.x, position.y].Type && !visited.Contains(checkPos))
            {
                RecurseChain(checkPos, visited, chain);
            }
        }
    }

    public int GetHeighest()
    {
        for (int y = GridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                if (Grid[x, y] != null)
                    return y;
            }
        }
        return -1;
    }
    public int GetHeighest(Tile.TileType type)
    {
        for (int y = GridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                if (Grid[x, y] != null && Grid[x, y].Type == type)
                    return y;
            }
        }
        return -1;
    }
    public int GetLowestEmpty()
    {
        for (int y = 0; y <= GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                if (Grid[x, y] == null)
                    return y;
            }
        }
        return -1;
    }

    public void RaiseGrid(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            for (int y = GridSize.y - 2; y >= 0; y--)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    if (Grid[x, y] != null)
                    {
                        Grid[x, y + 1] = Grid[x, y];
                        Grid[x, y] = null;
                        Grid[x, y + 1].Move(Vector2Int.up);
                    }
                }
            }
        }
    }

    public void SetTile(Tile tile)
    {
        Grid[tile.GridPosition.x, tile.GridPosition.y] = tile;
    }

    public void MovePair(Pair pair, Vector2Int offset)
    {
        if (pair == null) return;

        Vector2Int pivotOffset = pair.Pivot.GridPosition + offset;
        Vector2Int outerOffset = pair.Outer.GridPosition + offset;

        if (!InsideGrid(pivotOffset) || !InsideGrid(outerOffset))
            return;

        if (Grid[pivotOffset.x, pivotOffset.y] == null && Grid[outerOffset.x, outerOffset.y] == null)
        {
            pair.Outer.Move(offset);
            pair.Pivot.Move(offset);
        }
    }

    Vector2Int[] RotationOffsets = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};
    public void RotatePairCW(Pair pair)
    {
        if (pair == null) return;
        for (int i = 1; i < 4; i++)
        {
            int newRotation = (pair.Rotation + i) % 4;
            Vector2Int newOffset = RotationOffsets[newRotation];
            Vector2Int newOuterPos = pair.Pivot.GridPosition + newOffset;
            Vector2Int newPivotPos = pair.Pivot.GridPosition - newOffset;
            if (InsideGrid(newOuterPos) && Grid[newOuterPos.x, newOuterPos.y] == null)
            {
                pair.Outer.MoveTo(newOuterPos);
                pair.Rotation = newRotation;
                return;
            }
            else if (InsideGrid(newPivotPos) && Grid[newPivotPos.x, newPivotPos.y] == null)
            {
                pair.Outer.MoveTo(pair.Pivot.GridPosition);
                pair.Pivot.MoveTo(newPivotPos);
                pair.Rotation = newRotation;
                return;
            }
        }
    }
    public void RotatePairACW(Pair pair)
    {
        if (pair == null) return;
        for (int i = 1; i < 4; i++)
        {
            int newRotation = (pair.Rotation + 4 - i) % 4;
            Vector2Int newOffset = RotationOffsets[newRotation];
            Vector2Int newOuterPos = pair.Pivot.GridPosition + newOffset;
            Vector2Int newPivotPos = pair.Pivot.GridPosition - newOffset;
            if (InsideGrid(newOuterPos) && Grid[newOuterPos.x, newOuterPos.y] == null)
            {
                pair.Outer.MoveTo(newOuterPos);
                pair.Rotation = newRotation;
                return;
            }
            else if (InsideGrid(newPivotPos) && Grid[newPivotPos.x, newPivotPos.y] == null)
            {
                pair.Outer.MoveTo(pair.Pivot.GridPosition);
                pair.Pivot.MoveTo(newPivotPos);
                pair.Rotation = newRotation;
                return;
            }
        }
    }
    public void DecomposePair(Pair pair)
    {
        if (InsideGrid(pair.Pivot.GridPosition))
            Grid[pair.Pivot.GridPosition.x, pair.Pivot.GridPosition.y] = pair.Pivot;
        if (InsideGrid(pair.Outer.GridPosition))
            Grid[pair.Outer.GridPosition.x, pair.Outer.GridPosition.y] = pair.Outer;
    }

    public bool InsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GridSize.x && pos.y >= 0 && pos.y < GridSize.y;
    }
}
