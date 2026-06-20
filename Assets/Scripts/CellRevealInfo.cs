public readonly struct CellRevealInfo
{
    public int Col { get; }
    public int Row { get; }
    public int NeighborMines { get; }
    public bool IsMine { get; }

    public CellRevealInfo(int col, int row, int neighborMines, bool isMine = false)
    {
        Col = col;
        Row = row;
        NeighborMines = neighborMines;
        IsMine = isMine;
    }
}