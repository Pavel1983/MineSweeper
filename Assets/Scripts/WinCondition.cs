public class WinCondition
{
    public bool IsMet(Board board)
    {
        if (board == null || board.Cols == 0 || board.Rows == 0)
        {
            return false;
        }

        for (var row = 0; row < board.Rows; row++)
        {
            for (var col = 0; col < board.Cols; col++)
            {
                if (board.IsFlagged(col, row) && !board.IsMine(col, row))
                {
                    return false;
                }

                if (!board.IsMine(col, row) && !board.IsRevealed(col, row))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
