using System.Collections.Generic;

public struct RevealResult
{
    public bool Success { get; }
    public bool HitMine { get; }
    public IReadOnlyList<CellRevealInfo> OpenedCells { get; }

    public RevealResult(bool success, bool hitMine, IReadOnlyList<CellRevealInfo> openedCells)
    {
        Success = success;
        HitMine = hitMine;
        OpenedCells = openedCells;
    }

    public static RevealResult Failed { get; } = new(false, false, System.Array.Empty<CellRevealInfo>());
}