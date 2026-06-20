using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private class TileInfo
    {
        public bool Revealed;
        public bool Flagged;
        public bool HasMine;
    }
    
    private int _cols;
    private int _rows;
    private TileInfo[] _data;
    private int _minesCount;

    private List<Vector2Int> _cachedAvailablePositions;
    
    public bool TryInit(int cols, int rows, int minesCount)
    {
        _cols = cols;
        _rows = rows;
        _minesCount = minesCount;

        if (!IsValid())
        {
            return false;
        }

        _data = new TileInfo[_cols * _rows];
        for (var i = 0; i < _data.Length; i++)
        {
            _data[i] = new TileInfo();
        }

        _cachedAvailablePositions = new(_cols * rows);
        for (var c = 0; c < _cols; ++c)
        for (var r = 0; r < _rows; ++r)
        {
            _cachedAvailablePositions.Add(new Vector2Int(c, r));
        }

        int minesProcessed = 0;
        while (minesProcessed != _minesCount)
        {
            var availablePosIndex = Random.Range(0, _cachedAvailablePositions.Count);
            var pos = _cachedAvailablePositions[availablePosIndex];
            _cachedAvailablePositions.RemoveAt(availablePosIndex);

            var posIndex = pos.y * _cols + pos.x;
            _data[posIndex].HasMine = true;

            minesProcessed++;
        }

        return true;
    }

    public int Cols => _cols;
    public int Rows => _rows;

    public bool IsInBounds(int col, int row)
    {
        return TryGetTileIndex(col, row, out _);
    }

    public bool IsMine(int col, int row)
    {
        return TryGetTileIndex(col, row, out var index) && _data[index].HasMine;
    }

    public bool IsRevealed(int col, int row)
    {
        return TryGetTileIndex(col, row, out var index) && _data[index].Revealed;
    }

    public bool IsFlagged(int col, int row)
    {
        return TryGetTileIndex(col, row, out var index) && _data[index].Flagged;
    }

    public bool TryToggleFlag(int col, int row)
    {
        if (!TryGetTileIndex(col, row, out var index) || _data[index].Revealed)
        {
            return false;
        }

        _data[index].Flagged = !_data[index].Flagged;
        return true;
    }

    public bool TryRevealCell(int col, int row)
    {
        if (!TryGetTileIndex(col, row, out var index) || _data[index].Revealed)
        {
            return false;
        }

        _data[index].Revealed = true;
        return true;
    }

    public int GetNeighborMineCount(int col, int row)
    {
        if (!TryGetTileIndex(col, row, out _))
        {
            return -1;
        }

        var count = 0;
        for (var dCol = -1; dCol <= 1; dCol++)
        {
            for (var dRow = -1; dRow <= 1; dRow++)
            {
                if (dCol == 0 && dRow == 0)
                {
                    continue;
                }

                if (IsMine(col + dCol, row + dRow))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public bool IsThereMineAtPos(int x, int y)
    {
        var posIndex = y * _cols + x;

        return _data[posIndex].HasMine;
    }

    public void ResetupTheMine(Vector2Int minePos)
    {
        // removing old mine
        var minePosIndex = minePos.y * _cols + minePos.x;
        _data[minePosIndex].HasMine = false;
        
        // setup new mine
        var mineIndex = Random.Range(0, _cachedAvailablePositions.Count);
        var newMinePos = _cachedAvailablePositions[mineIndex];
        var newMinePosIndex = newMinePos.y * _cols + newMinePos.x;
        _data[newMinePosIndex].HasMine = true;
        
        // adjusting available positions cache
        _cachedAvailablePositions.RemoveAt(newMinePosIndex);
        _cachedAvailablePositions.Add(minePos);
    }

    private bool TryGetTileIndex(int col, int row, out int index)
    {
        if (_data == null || col < 0 || col >= _cols || row < 0 || row >= _rows)
        {
            index = -1;
            return false;
        }

        index = row * _cols + col;
        return true;
    }

    private bool IsValid()
    {
        if (_cols == 0 || _rows == 0)
        {
            return false;
        }

        if (_cols + _rows == 1)
        {
            return false;
        }

        if (_minesCount >= _cols * _rows)
        {
            return false;
        }

        return true;
    }
}