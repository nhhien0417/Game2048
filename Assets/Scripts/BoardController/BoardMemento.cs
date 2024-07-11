using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardMemento : Memento<Tile.TileCapture>
{
    public int Score;
    public int Moves;

    private static List<Tile.TileCapture> TileCaptures(List<Tile> tiles)
    {
        List<Tile.TileCapture> captures = new List<Tile.TileCapture>();
        foreach (var tile in tiles)
        {
            captures.Add(new Tile.TileCapture(tile.Characters, tile.Cell.Coordinates, tile.State));
        }
        return captures;
    }

    public BoardMemento(List<Tile> tiles, int score, int moves) : base(TileCaptures(tiles))
    {
        Score = score;
        Moves = moves;
    }
}

public class BoardCaretaker
{
    public Stack<BoardMemento> _mementos = new();

    public void SaveMemento(BoardMemento memento)
    {
        _mementos.Push(memento);
    }

    public BoardMemento RestoreMemento()
    {
        if (_mementos.Count == 0 || GameManager.Instance.RemainingUndos == 0)
            return null;

        return _mementos.Pop();
    }
}