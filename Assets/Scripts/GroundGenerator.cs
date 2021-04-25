using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGenerator : MonoBehaviour
{
    [SerializeField] private GroundGroup[] groups = new GroundGroup[1];

    public Tile GenerateTile(TileFactory tileFactory, Vector2Int gridPosition, int depth)
    {
        int realDepth = depth - gridPosition.y;
        GroundGroup group = GetGroup(realDepth);
        Tile tile = tileFactory.CreateTile(gridPosition, Tile.TileType.Ground, group.prefab);
        if (group != null)
            tile.GetComponent<SpriteRenderer>().sprite = group.sprites[Random.Range(0, group.sprites.Length)];

        return tile;
    }

    GroundGroup GetGroup(int depth)
    {
        GroundGroup group = null;
        foreach (GroundGroup g in groups)
        {
            if (depth >= g.startDepth && (group == null || g.startDepth > group.startDepth))
                group = g;
        }
        return group;
    }

    [System.Serializable]
    public class GroundGroup
    {
        public GameObject prefab;
        public Sprite[] sprites;
        public int startDepth;
    }
}
