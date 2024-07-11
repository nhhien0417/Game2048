using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using System;
using TMPro;

public class TileBoard : Singleton<TileBoard>
{
    public Swipe SwipeControls;
    public BoardCaretaker Caretaker = new BoardCaretaker();

    public Tile TilePrefab;
    public TileGrid _grid;
    public TileState[] TileStates;

    public int _highestIndex;
    public string _highestTile;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI HighscoreText;
    public TextMeshProUGUI MovesText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI UndoRemainingText;

    private List<Tile> _tiles;

    private bool _waiting;

    public Canvas Buttons;

    public void Initialize()
    {
        GameManager.Instance.IsLoaded = true;

        _grid = GetComponentInChildren<TileGrid>();
        _tiles = new List<Tile>(16);
    }

    private void Start()
    {
        Initialize();
        GameManager.Instance.NewGame();
    }

    private void Update()
    {
        if (GameManager.Instance.IsLoaded && GameManager.Instance._timeIsRunning && GameManager.Instance._timeRemaining >= 0)
        {
            GameManager.Instance._timeRemaining += Time.deltaTime;

            int minutes = Mathf.FloorToInt(GameManager.Instance._timeRemaining / 60);
            int seconds = Mathf.FloorToInt(GameManager.Instance._timeRemaining % 60);

            TimerText.text = $"{minutes:D2}:{seconds:D2}";
        }

        if (!_waiting)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || SwipeControls.SwipeUp)
            {
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || SwipeControls.SwipeDown)
            {
                MoveTiles(Vector2Int.down, 0, 1, _grid.Height - 2, -1);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || SwipeControls.SwipeLeft)
            {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || SwipeControls.SwipeRight)
            {
                MoveTiles(Vector2Int.right, _grid.Width - 2, -1, 0, 1);
            }
        }
    }

    public void ClearBoard()
    {
        foreach (var cell in _grid.Cells)
        {
            cell.Tile = null;
        }

        foreach (var tile in _tiles)
        {
            Destroy(tile.gameObject);
        }

        _tiles.Clear();
    }

    public void SpawnTiles()
    {
        var tile = GameManager.Instance._tilePool.Get();
        tile.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);

        if (SettingsManager.Instance.Theme == "Alphabet")
        {
            tile.SetState(TileStates[0], "A");
        }
        else
        {
            tile.SetState(TileStates[0], "2");
        }

        tile.Spawn(_grid.GetRandomEmptyCell());
        _tiles.Add(tile);
    }

    public void SpawnTilesUndo(BoardMemento memento)
    {
        foreach (var tile in memento.Items)
        {
            var UndoTile = GameManager.Instance._tilePool.Get();

            UndoTile.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
            UndoTile.SetState(tile.State, tile.Characters);
            UndoTile.Spawn(_grid.GetCell(tile.Coordinates));

            _tiles.Add(UndoTile);
        }
    }

    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        SaveToMemento();

        bool changed = false;

        for (var x = startX; x >= 0 && x < _grid.Width; x += incrementX)
        {
            for (var y = startY; y >= 0 && y < _grid.Height; y += incrementY)
            {
                var Cell = _grid.GetCell(x, y);

                if (Cell.Occupied)
                {
                    changed |= MoveTile(Cell.Tile, direction);
                }
            }
        }

        if (changed)
        {
            GameManager.Instance.SetMoves(false);
            StartCoroutine(WaitForChanges());
            AudioManager.Instance.SlideSFX();
        }
        else
        {
            Caretaker._mementos.Pop();
        }
    }

    private IEnumerator WaitForChanges()
    {
        _waiting = true;
        yield return new WaitForSeconds(0.05f);
        _waiting = false;

        foreach (var tile in _tiles)
        {
            tile.Locked = false;
        }

        if (_tiles.Count != _grid.Size)
        {
            SpawnTiles();
        }

        if (CheckForGameOver())
        {
            GameManager.Instance.GameOver();
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell NewCell = null;
        TileCell Adjacent = _grid.GetAdjacentCell(tile.Cell, direction);

        while (Adjacent != null)
        {
            if (Adjacent.Occupied)
            {
                if (CanMerge(tile, Adjacent.Tile))
                {
                    Merge(tile, Adjacent.Tile);

                    return true;
                }

                break;
            }

            NewCell = Adjacent;
            Adjacent = _grid.GetAdjacentCell(Adjacent, direction);
        }

        if (NewCell != null)
        {
            tile.MoveTo(NewCell);

            return true;
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.Characters == b.Characters && !b.Locked;
    }

    private void Merge(Tile a, Tile b)
    {
        _tiles.Remove(a);
        a.Merge(b.Cell);

        var index = Mathf.Clamp(IndexOf(b.State) + 1, 0, TileStates.Length - 1);
        string characters;

        if (SettingsManager.Instance.Theme == "Alphabet")
        {
            characters = ((char)(b.Characters[0] + 1)).ToString();
        }
        else
        {
            characters = (Convert.ToInt32(b.Characters) * 2).ToString();
        }

        if (index > _highestIndex)
        {
            _highestIndex = index;
            _highestTile = characters;
        }

        b.SetState(TileStates[index], characters);
        b.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.1f).
        OnComplete(() => b.transform.DOScale(Vector3.one, 0.1f));

        GameManager.Instance.IncreaseScore(StringToInt(characters));
        AudioManager.Instance.MergeSFX();
    }

    public int StringToInt(string str)
    {
        switch (str)
        {
            case "B":
            case "4":
                return 4;
            case "C":
            case "8":
                return 8;
            case "D":
            case "16":
                return 16;
            case "E":
            case "32":
                return 32;
            case "F":
            case "64":
                return 64;
            case "G":
            case "128":
                return 128;
            case "H":
            case "256":
                return 256;
            case "I":
            case "512":
                return 512;
            case "J":
            case "1024":
                return 1024;
            case "K":
            case "2048":
                return 2048;
            case "L":
            case "4096":
                return 4096;
            default:
                return 0;
        }
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < TileStates.Length; i++)
        {
            if (state == TileStates[i])
            {
                return i;
            }
        }

        return -1;
    }

    private bool CheckForGameOver()
    {
        if (_tiles.Count != _grid.Size)
        {
            return false;
        }

        foreach (var tile in _tiles)
        {
            var up = _grid.GetAdjacentCell(tile.Cell, Vector2Int.up);
            var down = _grid.GetAdjacentCell(tile.Cell, Vector2Int.down);
            var left = _grid.GetAdjacentCell(tile.Cell, Vector2Int.left);
            var right = _grid.GetAdjacentCell(tile.Cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.Tile))
            {
                return false;
            }

            if (down != null && CanMerge(tile, down.Tile))
            {
                return false;
            }

            if (left != null && CanMerge(tile, left.Tile))
            {
                return false;
            }

            if (right != null && CanMerge(tile, right.Tile))
            {
                return false;
            }
        }

        return true;
    }

    public void Undo()
    {
        this.enabled = false;
        var memento = Caretaker.RestoreMemento();

        if (memento != null)
        {
            RestoreFromMemento(memento);

            UndoRemainingText.text = (--GameManager.Instance.RemainingUndos).ToString();
            GameManager.Instance.SaveRemainingUndos();

            if (GameManager.Instance.RemainingUndos == 0)
            {
                UndoRemainingText.text = "+";
            }
        }

        this.enabled = true;
    }

    private void SaveToMemento()
    {
        var memento = new BoardMemento(_tiles, GameManager.Instance.Score, GameManager.Instance.Moves);
        Caretaker.SaveMemento(memento);
    }

    private void RestoreFromMemento(BoardMemento memento)
    {
        ClearBoard();
        SpawnTilesUndo(memento);

        GameManager.Instance.Score = memento.Score;
        GameManager.Instance.Moves = memento.Moves;
        GameManager.Instance.UndoUI();
    }

    public void NewGameButton()
    {
        GameManager.Instance.OnClickNewGame();
    }

    public void UndoButton()
    {
        GameManager.Instance.UndoMove();
    }

    public void SettingsButton()
    {
        GameManager.Instance.OpenSettings();
    }

    public void UndoRemainingButton()
    {
        GameManager.Instance.AddUndoRemaining();
    }
}
