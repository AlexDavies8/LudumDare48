using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private int _energyBallSeekDelayMin = 20;
    [SerializeField] private int _energyBallSeekDelayMax = 30;
    [SerializeField] private Vector2 _energyBallOffset = new Vector2(0f, 3f);
    [SerializeField] private SpriteRenderer _selector = null;
    [SerializeField] private Vector2 _selectorOffset = Vector2.zero;
    [SerializeField] private AudioPlayer _audioPlayer = null;
    [SerializeField] private AudioClip[] _explosionClips = new AudioClip[0];
    [SerializeField] private AudioClip[] _chainClips = new AudioClip[0];
    [SerializeField] private AudioClip _rumbleClip = null;
    [SerializeField] private AudioClip _seekClip = null;
    [SerializeField] private GameObject _depthGaugePrefab = null;
    [SerializeField] private int _depthGaugeSpacing = 5;
    [SerializeField] private Vector2 _gaugeOffset = Vector2.zero;
    [SerializeField] private GameObject _gameOverPanel = null;
    [SerializeField] private float _depthSpeedScalar = 1f;
    [SerializeField] private AudioSource _musicSource = null;

    public int Depth { get; set; }
    public Action<List<(Tile.TileType, Tile.TileType)>> OnNextPair;

    List<(int, Transform)> _depthGauges = new List<(int, Transform)>();
    int _lastDepthGauge = 0;

    Pair _currentPair;

    int _stepTimer = 0;

    bool _updatingGrid = false;

    int _pauseTimer = 0;
    int _scrollTimer = 0;

    int _seekCount = 0;
    GameObject _energyBall = null;

    List<(Tile.TileType, Tile.TileType)> _pairQueue = new List<(Tile.TileType, Tile.TileType)>();

    int _multiplier = 0;
    int _actualStepDelay = 0;
    bool _appliedMultiplier = false;

    private void Awake()
    {
        NextPair();
        _grid.ExplodeChainsCallback = HandleExplodedChains;
        CreateGround(_groundHeight);
        _actualStepDelay = _stepDelay;
        AddDepthGauge(0);
    }

    private int GetStepDelay()
    {
        return Mathf.CeilToInt((2 * _stepDelay * Mathf.Pow(0.5f, Mathf.Pow(Depth + 1, 1f/3f)) - _stepDelay) * _depthSpeedScalar + _stepDelay);
    }

    private void Update()
    {
        _actualStepDelay = GetStepDelay();
        _musicSource.pitch = 1f + Depth * 0.01f;
        if (_pauseTimer <= 0 && _scrollTimer <= 0)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Z))
                _grid.RotatePairACW(_currentPair);
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.X))
                _grid.RotatePairCW(_currentPair);
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                _grid.MovePair(_currentPair, Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                _grid.MovePair(_currentPair, Vector2Int.right);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                _stepTimer = 0;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                _actualStepDelay = (int)(GetStepDelay() * _dropDelayMultiplier);
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
                                _pauseTimer = Mathf.Max(_pauseTimer, _energyBallSeekDelayMax);
                                _particleFactory.CreateExplosionParticle((Vector2)(_grid.GridSize) * 0.5f + _energyBallOffset);
                                if (!_appliedMultiplier)
                                {
                                    _appliedMultiplier = true;
                                    _seekCount *= _multiplier;
                                }
                                FireEnergyBall();
                            }
                            else
                            {
                                if (_grid.GetHeighest() == _grid.GridSize.y - 1)
                                {
                                    _gameOverPanel.SetActive(true);
                                    this.enabled = false;
                                }

                                _multiplier = 0;
                                _appliedMultiplier = false;
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
            foreach (var gauge in _depthGauges)
            {
                gauge.Item2.localPosition = new Vector2(0, Depth - gauge.Item1 + _groundHeight) + _gaugeOffset;
            }

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

        _selector.enabled = _currentPair != null;
        if (_currentPair != null)
            _selector.transform.localPosition = _currentPair.Pivot.GridPosition + _selectorOffset;
    }

    private void NextPair()
    {
        while (_pairQueue.Count < 4)
            _pairQueue.Add(((Tile.TileType)UnityEngine.Random.Range(0, 4), (Tile.TileType)UnityEngine.Random.Range(0, 4)));
        var pairTypes = _pairQueue[0];
        _pairQueue.RemoveAt(0);
        Vector2Int pairStartPosition = new Vector2Int(_grid.GridSize.x / 2 - 1, _grid.GridSize.y);
        _currentPair = _tileFactory.CreatePair(pairStartPosition, pairTypes.Item1, pairTypes.Item2);
        OnNextPair?.Invoke(_pairQueue);
    }

    private void RaiseGround()
    {
        int groundLevel = _grid.GetLowestEmpty();
        if (groundLevel < _groundHeight)
        {
            _audioPlayer.Play(_rumbleClip);
            _grid.RaiseGrid(_groundHeight - groundLevel);
            CreateGround(_groundHeight - groundLevel);

            if (Depth + _groundHeight >= _lastDepthGauge + _depthGaugeSpacing)
            {
                AddDepthGauge(_lastDepthGauge + _depthGaugeSpacing);
            }

            Depth += _groundHeight - groundLevel;
            _gridContainer.position -= Vector3.up * (_groundHeight - groundLevel);
            _scrollTimer = _scrollDelay * (_groundHeight - groundLevel);
        }
    }

    void AddDepthGauge(int depth)
    {
        var gaugeTransform = Instantiate(_depthGaugePrefab).GetComponent<RectTransform>();
        gaugeTransform.SetParent(_gridContainer);
        gaugeTransform.localPosition = new Vector2(0, Depth - depth + _groundHeight) + _gaugeOffset;
        gaugeTransform.GetComponentInChildren<Text>().text = (depth).ToString();
        _depthGauges.Add((depth, gaugeTransform));
        _lastDepthGauge = depth;
    }

    private void CreateGround(int level)
    {
        for (int y = 0; y < level; y++)
        {
            for (int x = 0; x < _grid.GridSize.x; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                _grid.SetTile(_groundGenerator.GenerateTile(_tileFactory, gridPos, Depth + _groundHeight));
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
            _audioPlayer.Play(_chainClips[Mathf.Min(_chainClips.Length-1, _multiplier)]);
            _multiplier++;
            _pauseTimer = Mathf.Max(_pauseTimer, _explodeDelay);
            if (chain.Count >= 5)
            {
                _audioPlayer.Play(_seekClip);
                _pauseTimer = Mathf.Max(_pauseTimer, _explodeDelay + _energyBallSeekDelayMax);
                _seekCount += chain.Count - 4;
                if (_energyBall == null)
                    _energyBall = _particleFactory.CreateEnergyBallParticle(_energyBallOffset);
                Vector2 centre = Vector2.zero;
                foreach (Vector2Int pos in chain)
                {
                    centre += ((Vector2)pos) / chain.Count;
                }
                ParticlePath seekParticle = _particleFactory.CreateSeekParticle(centre, (Vector2)(_grid.GridSize) / 2f + _energyBallOffset, UnityEngine.Random.Range(_energyBallSeekDelayMin, _energyBallSeekDelayMax) * Time.fixedDeltaTime);
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
        _audioPlayer.PlayRandom(0.9f, _explosionClips);
        List<Vector2Int> destroyedGround = new List<Vector2Int>();
        int i = _seekCount;
        if (_multiplier <= 0)
        {
            _seekCount = 0;
            Destroy(_energyBall);
        }
        _audioPlayer.Play(_seekClip);
        for (int y = _grid.GridSize.y - 1; y >= 0; y--)
        {
            for (int x = 0; x < _grid.GridSize.x; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!destroyedGround.Contains(pos) && _grid.Grid[x, y] != null && _grid.Grid[x, y].Type == Tile.TileType.Ground)
                {
                    ParticlePath seekParticle = _particleFactory.CreateSeekParticle((Vector2)(_grid.GridSize) * 0.5f + _energyBallOffset, pos, Time.fixedDeltaTime * UnityEngine.Random.Range(_energyBallSeekDelayMin, _energyBallSeekDelayMax));
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
        _audioPlayer.PlayRandom(0.5f, _explosionClips);
        _cameraShaker.AddTrauma(0.1f);
        _grid.DestroyTileAt(pos, Time.fixedDeltaTime * _explodeDelay);
        _particleFactory.CreateExplosionParticle(pos);
    }
}
