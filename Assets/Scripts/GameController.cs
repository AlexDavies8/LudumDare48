using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameGrid _grid = null;
    [SerializeField] private int _stepDelay = 60;
    [SerializeField, Range(0f, 2f)] private float _gridStepDelayMultiplier = 0.5f;
    [SerializeField] private TileFactory _tileFactory = null;
    [SerializeField] private int _groundHeight = 4;
    [SerializeField] private GroundGenerator _groundGenerator = null;
    [SerializeField] private Sprite _tileExplodeSprite = null;
    [SerializeField] private int _explodeDelay = 30;
    [SerializeField] private Transform _backgroundContainer = null;
    [SerializeField] private Transform _gridContainer = null;
    [SerializeField] private int _scrollDelay = 20;
    
    public int Depth { get; set; }

    Pair _currentPair;

    int _stepTimer = 0;

    bool _updatingGrid = false;

    int _pauseTimer = 0;
    bool _scrolling = false;

    private void Awake()
    {
        NextPair();
        _grid.ExplodeChainsCallback = HandleExplodedChains;
    }

    private void Update()
    {
        if (_pauseTimer <= 0)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                _grid.RotatePairACW(_currentPair);
            if (Input.GetKeyDown(KeyCode.E))
                _grid.RotatePairCW(_currentPair);
            if (Input.GetKeyDown(KeyCode.A))
                _grid.MovePair(_currentPair, Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.D))
                _grid.MovePair(_currentPair, Vector2Int.right);
            if (Input.GetKeyDown(KeyCode.S))
                _grid.MovePair(_currentPair, Vector2Int.down);
        }
    }

    private void FixedUpdate()
    {
        if (_pauseTimer <= 0)
        {
            _stepTimer--;
            if (_updatingGrid)
            {
                if (_stepTimer <= 0)
                {
                    _stepTimer = (int)(_stepDelay * _gridStepDelayMultiplier);

                    _grid.StepGrid();
                    int groundLevel = _grid.GetHeighest(Tile.TileType.Ground) + 1;
                    if (groundLevel < _groundHeight)
                    {
                        _grid.RaiseGrid(_groundHeight - groundLevel);
                        for (int y = 0; y < _groundHeight - groundLevel; y++)
                        {
                            for (int x = 0; x < _grid.GridSize.x; x++)
                            {
                                Vector2Int gridPos = new Vector2Int(x, y);
                                _grid.SetTile(_groundGenerator.GenerateTile(_tileFactory, gridPos, Depth));
                            }
                        }
                        Depth += _groundHeight - groundLevel;
                        _gridContainer.position -= Vector3.up * (_groundHeight - groundLevel);
                        _pauseTimer = _scrollDelay * (_groundHeight - groundLevel);
                        _scrolling = true;
                    }
                    if (_grid.IsGridStable())
                        _updatingGrid = false;
                }
            }
            else
            {
                if (_stepTimer <= 0)
                {
                    if (!_grid.StepPair(_currentPair))
                    {
                        NextPair();
                        _updatingGrid = true;
                    }
                    else
                        _stepTimer = _stepDelay;
                }
            }
        }
        if (_scrolling)
        {
            if (_gridContainer.position.y < 0)
            {
                _gridContainer.position += Vector3.up * Time.fixedDeltaTime / (_scrollDelay * Time.fixedDeltaTime);
                _backgroundContainer.position += Vector3.up * Time.fixedDeltaTime / (_scrollDelay * Time.fixedDeltaTime);
            }    
            else
            {
                _scrolling = false;
                _gridContainer.position = new Vector2(_gridContainer.position.x, 0);
            }
        }

        _pauseTimer--;
    }

    private void NextPair()
    {
        Vector2Int pairStartPosition = new Vector2Int(_grid.GridSize.x / 2, _grid.GridSize.y);
        _currentPair = _tileFactory.CreatePair(pairStartPosition, (Tile.TileType)Random.Range(0, 4), (Tile.TileType)Random.Range(0, 4));
    }

    private void HandleExplodedChains(List<List<Vector2Int>> chains)
    {
        if (chains.Count == 0) return;

        foreach (List<Vector2Int> chain in chains)
        {
            if (chain.Count < 4) continue;
            _pauseTimer = _explodeDelay;
            foreach (Vector2Int pos in chain)
            {
                _grid.Grid[pos.x, pos.y].GetComponent<SpriteRenderer>().sprite = _tileExplodeSprite;
                _grid.DestroyTileAt(pos, Time.fixedDeltaTime * _explodeDelay);
            }
        }
    }
}
