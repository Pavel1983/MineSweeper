using UnityEngine;

public readonly struct GridLayout
{
    public int Cols { get; }
    public int Rows { get; }
    public float CellSize { get; }
    public float Pitch { get; }
    public Vector3 Origin { get; }

    public GridLayout(int cols, int rows, float cellSize, float pitch, Vector3 origin)
    {
        Cols = cols;
        Rows = rows;
        CellSize = cellSize;
        Pitch = pitch;
        Origin = origin;
    }
}

public static class BoardLayout
{
    public static bool TryCalculate(int cols, int rows, Bounds bounds, float spacing, out GridLayout layout)
    {
        var cellSizeX = (bounds.size.x - (cols - 1) * spacing) / cols;
        var cellSizeY = (bounds.size.y - (rows - 1) * spacing) / rows;
        var cellSize = Mathf.Min(cellSizeX, cellSizeY);

        if (cellSize <= 0f)
        {
            Debug.LogError("Tile spacing is too large for the board viewport.");
            layout = default;
            return false;
        }

        var gridWidth = cols * cellSize + (cols - 1) * spacing;
        var gridHeight = rows * cellSize + (rows - 1) * spacing;
        var origin = bounds.center - new Vector3(gridWidth * 0.5f, gridHeight * 0.5f, 0f);
        layout = new GridLayout(cols, rows, cellSize, cellSize + spacing, origin);
        return true;
    }

    public static Vector3 GetCellCenter(in GridLayout layout, int col, int row)
    {
        return layout.Origin + new Vector3(
            col * layout.Pitch + layout.CellSize * 0.5f,
            row * layout.Pitch + layout.CellSize * 0.5f,
            0f
        );
    }

    public static int GetTileIndex(int cols, int col, int row)
    {
        return row * cols + col;
    }
}
