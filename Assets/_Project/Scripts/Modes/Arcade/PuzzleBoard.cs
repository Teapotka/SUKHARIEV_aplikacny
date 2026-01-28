using UnityEngine;

public class PuzzleBoard : MonoBehaviour
{
    public PuzzleTile tilePrefab;

    public int col = 6;
    public int row = 4;
    public float tileSpacing = 1.05f;
    public Transform anchor;
    public float tileScale = 1.35f;
    public float gap = 0f;

    private PuzzleTile[,] grid;
    private Vector2Int emptyCell;

    void Start()
    {
        grid = new PuzzleTile[col, row];
        emptyCell = new Vector2Int(col - 1, row - 1);

        var r = tilePrefab.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float tileWorldWidth = r.bounds.size.x * tileScale;
            tileSpacing = tileWorldWidth + gap;
        }

        Generate();
    }

    void Generate()
    {
        if (anchor == null) anchor = transform;

        for (int y = 0; y < row; y++)
            for (int x = 0; x < col; x++)
            {
                if (x == emptyCell.x && y == emptyCell.y) continue;

                var tile = Instantiate(tilePrefab, anchor);
                tile.transform.localScale *= tileScale;
                tile.Init(this, x, y, col, row);

                tile.transform.localPosition = GridToLocal(x, y);
                grid[x, y] = tile;
            }
    }

    private Vector3 GridToLocal(int x, int y)
    {
        float ox = (col - 1) / 2f;
        float oy = (row - 1) / 2f;

        return new Vector3(
            (x - ox) * tileSpacing,
            (oy - y) * tileSpacing,
            0f
        );
    }

    public bool CanMove(Vector2Int tilePos)
    {
        return Vector2Int.Distance(tilePos, emptyCell) == 1;
    }

    public void Move(PuzzleTile tile)
    {
        if (!CanMove(tile.GridPos)) return;

        Vector2Int oldPos = tile.GridPos;

        grid[emptyCell.x, emptyCell.y] = tile;
        grid[oldPos.x, oldPos.y] = null;

        tile.SetGridPos(emptyCell.x, emptyCell.y);
        tile.transform.localPosition = GridToLocal(emptyCell.x, emptyCell.y);

        emptyCell = oldPos;
    }
}
