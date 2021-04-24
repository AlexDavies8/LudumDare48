using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameGrid _grid = null;
    [SerializeField] private int _stepDelay = 60;
    [SerializeField, Range(0f, 1f)] private float _dropDelayMultiplier = 0.2f;
    [SerializeField, Range(0f, 2f)] private float _gridStepDelayMultiplier = 0.5f;
    [SerializeField] private TileFactory _tileFactory = null;
    [SerializeField] private int _groundHeight = 4;
    [SerializeField] private GroundGenerator _groundGenerator = null;
    [SerializeField] private Sprite _tileExplodeSprite = null;
    [SerializeField] private int _explodeDelay = 30;
    [SerializeField] private Transform _backgroundContainer = null;
    [SerializeField] private Transform _gridContainer = null;
    [SerializeField] private int _scrollDelay = 20;
    [SerializeField] private ParticleFactory _particleFactory = null;
    [SerializeField] private CameraShaker _cameraShaker = null;
    [SerializeField] private int _energyBallSeekDelay = 30;
    [SerializeField] private Vector2 _energyBallOffset = new Vector2(0f, 3f);

    public int Depth { get; set; }

    Pair _currentPair;

    int _stepTimer = 0;

    bool _updatingGrid = false;

    int _pauseTimer = 0;
    int _scrollTimer = 0;

    int _seekCount = 0;
    GameObject _energyBall = null;

    int _multiplier = 0;
    int _actualStepDelay = 0;

    private void Awake()
    {
        NextPair();
        _grid.ExplodeChainsCallback = HandleExplodedChains;
        CreateGround(_groundHeight);
        _actualStepDelay = _stepDelay;
    }

    private void Update()
    {
        if (_pauseTimer <= 0 && _scrollTimer <= 0)
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
            {
                _stepTimer = 0;
                _actualStepDelay = (int)(_stepDelay * _dropDelayMultiplier);
            }
            if (Input.GetKeyUp(KeyCode.S))
                _actualStepDelay = _stepDelay;
        }
    }

    private void FixedUpdate()
    {
        if (_pauseTimer <= 0 && _scrollTimer <= 0)
        {
            _stepTimer--;
            if (_updatingGrid)
            {
                if (_stepTimer <= 0)
                {
                    _stepTimer = (int)(_stepDelay * _gridStepDelayMultiplier);

                    RaiseGround();
                    _grid.StepGrid();
                    if (_grid.IsGridStable())
                    {
                        bool chainsToExplode = _grid.ExplodeChains();
                        if (!chainsToExplode)
                        {
                            if (_seekCount > 0)
                            {
                                _pauseTimer = Mathf.Max(_pauseTimer, _energyBallSeekDelay);
                                _particleFactory.CreateExplosionParticle((Vector2)(_grid.GridSize) * 0.5f + _energyBallOffset);
                                FireEnergyBall();
                            }
                            else
                            {
                                _multiplier = 0;
                                _updatingGrid = false;
                            }
                        }
                    }
                }
            }
            else
            {
                if (_currentPair == null)
                    NextPair();
                if (_stepTimer <= 0)
                {
                    if (!_grid.StepPair(_currentPair))
                    {
                        _currentPair = null;
                        _updatingGrid = true;
                    }
                    else
                        _stepTimer = _actualStepDelay;
                }
            }
        }
        if (_scrollTimer > 0)
        {
            if (_gridContainer.position.y < 0)
            {
                _gridContainer.position += Vector3.up * Time.fixedDeltaTime / (_scrollDelay * Time.fixedDeltaTime);
                _backgroundContainer.position += Vector3.up * Time.fixedDeltaTime / (_scrollDelay * Time.fixedDeltaTime);
                _cameraShaker.AddTrauma(2f * Time.fixedDeltaTime);
            }
            else
            {
                _gridContainer.position = new Vector2(_gridContainer.position.x, 0);
            }
        }
        else
        {
            _gridContainer.position = new Vector2(_gridContainer.position.x, 0);
        }

        _scrollTimer--;
        _pauseTimer--;
    }

    private void NextPair()
    {
        Vector2Int pairStartPosition = new Vector2Int(_grid.GridSize.x / 2 - 1, _grid.GridSize.y);
        _currentPair = _tileFactory.CreatePair(pairStartPosition, (Tile.TileType)Random.Range(0, 4), (Tile.TileType)Random.Range(0, 4));
    }

    private void RaiseGround()
    {
        int groundLevel = _grid.GetLowestEmpty();
        if (groundLevel < _groundHeight)
        {
            _grid.RaiseGrid(_groundHeight - groundLevel);
            CreateGround(_groundHeight - groundLevel);
            Depth += _groundHeight - groundLevel;
            _gridContainer.position -= Vector3.up * (_groundHeight - groundLevel);
            _scrollTimer = _scrollDelay * (_groundHeight - groundLevel);
        }
    }

    private void CreateGround(int level)
    {
        for (int y = 0; y < level; y++)
        {
            for (int x = 0; x < _grid.GridSize.x; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                _grid.SetTile(_groundGenerator.GenerateTile(_tileFactory, gridPos, Depth));
            }
        }
    }

    private void HandleExplodedChains(List<List<Vector2Int>> chains)
    {
        if (chains.Count == 0) return;

        foreach (List<Vector2Int> chain in chains)
        {
            if (chain.Count < 4) continue;
            _cameraShaker.AddTrauma(1f - 8f/Mathf.Pow(2, chain.Count + _multiplier));
            _multiplier++;
            _pauseTimer = Mathf.Max(_pauseTimer, _explodeDelay);
            if (chain.Count >= 5)
            {
                _pauseTimer = Mathf.Max(_pauseTimer, _explodeDelay + _energyBallSeekDelay);
                _seekCount += chain.Count - 4;
                if (_energyBall == null)
                    _energyBall = _particleFactory.CreateEnergyBallParticle(_energyBallOffset);
                Vector2 centre = Vector2.zero;
                foreach (Vector2Int pos in chain)
                {
                    centre += ((Vector2)pos) / chain.Count;
                }
                ParticlePath seekParticle = _particleFactory.CreateSeekParticle(centre, (Vector2)(_grid.GridSize) / 2f + _energyBallOffset, _energyBallSeekDelay * Time.fixedDeltaTime);
                seekParticle.transform.localScale = Vector3.one * Mathf.Sqrt(chain.Count - 4);
                seekParticle.OnPathCompleted = () =>
                {
                    if (_energyBall != null)
                        _energyBall.transform.localScale = Vector3.one * Mathf.Pow(_seekCount, 1 / 3f);
                };
            }
            foreach (Vector2Int pos in chain)
            {
                ExplodeTile(pos);
            }
        }
    }

    Vector2Int[] ExplodeOffsets = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    private void ExplodeTile(Vector2Int pos)
    {
        _grid.Grid[pos.x, pos.y].GetComponent<SpriteRenderer>().sprite = _tileExplodeSprite;
        _grid.DestroyTileAt(pos, Time.fixedDeltaTime * _explodeDelay);

        foreach (Vector2Int offset in ExplodeOffsets)
        {
            Vector2Int checkPos = pos + offset;
            if (_grid.InsideGrid(checkPos) && _grid.Grid[checkPos.x, checkPos.y] != null && _grid.Grid[checkPos.x, checkPos.y].Type == Tile.TileType.Ground)
            {
                ExplodeGround(checkPos);
            }
        }
    }

    private void FireEnergyBall()
    {
        _multiplier--;
        _cameraShaker.AddTrauma(0.8f);
        List<Vector2Int> destroyedGround = new List<Vector2Int>();
        int i = _seekCount;
        if (_multiplier <= 0)
        {
            _seekCount = 0;
            Destroy(_energyBall);
        }
        for (int y = _grid.GridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < _grid.GridSize.x; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!destroyedGround.Contains(pos) && _grid.Grid[x, y] != null && _grid.Grid[x, y].Type == Tile.TileType.Ground)
                {
                    ParticlePath seekParticle = _particleFactory.CreateSeekParticle((Vector2)(_grid.GridSize) * 0.5f + _energyBallOffset, pos, Time.fixedDeltaTime * _energyBallSeekDelay);
                    seekParticle.OnPathCompleted += () => ExplodeGround(pos);
                    destroyedGround.Add(pos);
                    i--;
                }
                if (i <= 0) return;
            }
        }
    }

    private void ExplodeGround(Vector2Int pos)
    {
        _cameraShaker.AddTrauma(0.1f);
        _grid.DestroyTileAt(pos, Time.fixedDeltaTime * _explodeDelay);
        _particleFactory.CreateExplosionParticle(pos);
    }
}
