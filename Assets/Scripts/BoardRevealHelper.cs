using System.Collections.Generic;

public class BoardRevealHelper
{
    private readonly Board _board;
    private readonly Queue<(int Col, int Row)> _pendingCells = new();
    private readonly List<CellRevealInfo> _openedCells = new();

    public BoardRevealHelper(Board board)
    {
        _board = board;
    }

    public RevealResult Reveal(int col, int row)
    {
        if (!_board.IsInBounds(col, row) || _board.IsRevealed(col, row))
        {
            return RevealResult.Failed;
        }

        _openedCells.Clear();
        _pendingCells.Clear();

        if (_board.IsMine(col, row))
        {
            RevealMine(col, row);
            return new RevealResult(true, true, _openedCells);
        }

        _pendingCells.Enqueue((col, row));
        FloodReveal();

        return new RevealResult(true, false, _openedCells);
    }

    private void RevealMine(int col, int row)
    {
        _board.TryRevealCell(col, row);
        _openedCells.Add(new CellRevealInfo(col, row, _board.GetNeighborMineCount(col, row), isMine: true));
    }

    // BFS
    private void FloodReveal()
    {
        while (_pendingCells.Count > 0)
        {
            var (col, row) = _pendingCells.Dequeue();

            if (_board.IsRevealed(col, row) || _board.IsMine(col, row))
            {
                continue;
            }

            var neighborMines = _board.GetNeighborMineCount(col, row);
            if (!_board.TryRevealCell(col, row))
            {
                continue;
            }

            _openedCells.Add(new CellRevealInfo(col, row, neighborMines));

            if (neighborMines != 0)
            {
                continue;
            }

            EnqueueNeighbors(col, row);
        }
    }

    private void EnqueueNeighbors(int col, int row)
    {
        for (var dCol = -1; dCol <= 1; dCol++)
        {
            for (var dRow = -1; dRow <= 1; dRow++)
            {
                if (dCol == 0 && dRow == 0)
                {
                    continue;
                }

                var neighborCol = col + dCol;
                var neighborRow = row + dRow;

                if (!_board.IsInBounds(neighborCol, neighborRow)
                    || _board.IsRevealed(neighborCol, neighborRow)
                    || _board.IsMine(neighborCol, neighborRow))
                {
                    continue;
                }

                _pendingCells.Enqueue((neighborCol, neighborRow));
            }
        }
    }
}
