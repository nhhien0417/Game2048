using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using TMPro;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public class TileCapture
    {
        public TileState State;
        public Vector2Int Coordinates;
        public string Characters;

        public TileCapture(string character, Vector2Int coordinates, TileState state)
        {
            State = state;
            Coordinates = coordinates;
            Characters = character;
        }
    }

    public TileState State { get; private set; }
    public TileCell Cell { get; private set; }

    public string Characters { get; private set; }
    public bool Locked { get; set; }

    public IObjectPool<Tile> _managePool;
    public IObjectPool<Tile> ObjectPool { set => _managePool = value; }

    private Image _background;
    private TextMeshProUGUI _numberText;

    private void Awake()
    {
        _background = GetComponent<Image>();
        _numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state, string characters)
    {
        State = state;
        Characters = characters;

        _background.color = state.BackgroundColor;
        _numberText.color = state.TextColor;
        _numberText.text = characters.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (Cell != null)
        {
            Cell.Tile = null;
        }

        Cell = cell;
        Cell.Tile = this;

        transform.position = cell.transform.position;
    }

    public void MoveTo(TileCell cell)
    {
        if (Cell != null)
        {
            Cell.Tile = null;
        }

        Cell = cell;
        Cell.Tile = this;

        transform.DOMove(cell.transform.position, 0.18f);
    }

    public void Merge(TileCell cell)
    {
        if (Cell != null)
        {
            Cell.Tile = null;
        }

        Cell = null;

        cell.Tile.Locked = true;

        transform.DOMove(cell.transform.position, 0.18f).OnComplete(() => _managePool.Release(this));
    }
}
