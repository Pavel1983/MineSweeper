using System.Collections.Generic;
using UnityEngine;

public class Board
{
    private int _cols;
    private int _rows;
    private bool[] _data;
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

        _data = new bool [_cols * _rows];
        
        // Generate 
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
            _data[posIndex] = true;

            minesProcessed++;
        }

        return true;
    }

    public bool IsThereMineAtPos(int x, int y)
    {
        var posIndex = y * _cols + x;

        return _data[posIndex];
    }

    public void ResetupTheMine(Vector2Int minePos)
    {
        // removing old mine
        var minePosIndex = minePos.y * _cols + minePos.x;
        _data[minePosIndex] = false;
        
        // setup new mine
        var mineIndex = Random.Range(0, _cachedAvailablePositions.Count);
        var newMinePos = _cachedAvailablePositions[mineIndex];
        var newMinePosIndex = newMinePos.y * _cols + newMinePos.x;
        _data[newMinePosIndex] = true;
        
        // adjusting available positions cache
        _cachedAvailablePositions.RemoveAt(newMinePosIndex);
        _cachedAvailablePositions.Add(minePos);
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