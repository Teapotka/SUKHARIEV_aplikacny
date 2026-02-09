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
    private PuzzleTile selectedTile;
    [SerializeField] private int shuffleMoves = 200;
    [SerializeField, Range(0f, 1f)]
    private float randomFlipChance = 0.5f;

    private Texture _frontTex;
    private Texture _backTex;
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
        Shuffle();
        RandomizeFlips();
    }

    public void SetTextures(Texture front, Texture back)
    {
        _frontTex = front;
        _backTex = back;
    }

    public void RebuildNewPuzzle()
    {
        ClearBoard();

        grid = new PuzzleTile[col, row];
        emptyCell = new Vector2Int(col - 1, row - 1);
        selectedTile = null;

        Generate();
        Shuffle();
        RandomizeFlips(); 
    }

    private void ClearBoard()
    {
        if (grid == null) return;

        for (int y = 0; y < row; y++)
            for (int x = 0; x < col; x++)
            {
                if (grid[x, y] != null)
                    Destroy(grid[x, y].gameObject);
            }
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
                tile.SetTexturesForMaterials(_frontTex, _backTex);
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



    // ---------- Selection ----------

    public void SelectTile(PuzzleTile tile)
    {
        Debug.Log("Selected: " + tile.name);
        if (selectedTile == tile) return;

        selectedTile?.Deselect();
        selectedTile = tile;
        selectedTile.Select();
    }


    public void MoveSelected(Vector2Int dir)
    {
        if (selectedTile == null) return;

        Vector2Int target = selectedTile.GridPos + dir;
        if (target != emptyCell) return;


        Vector2Int oldPos = selectedTile.GridPos;

        grid[emptyCell.x, emptyCell.y] = selectedTile;
        grid[oldPos.x, oldPos.y] = null;

        selectedTile.SetGridPos(emptyCell.x, emptyCell.y);
        selectedTile.transform.localPosition = GridToLocal(emptyCell.x, emptyCell.y);

        emptyCell = oldPos;

        CheckWin();
    }

    public void FlipSelected()
    {
        if (selectedTile == null) return;
        selectedTile.Flip();
        CheckWin();
    }


    public void Shuffle()
    {
        for (int i = 0; i < shuffleMoves; i++)
        {
            MakeRandomValidMove();
        }
    }

    private void MakeRandomValidMove()
    {
        var neighbors = GetMovableTiles();

        if (neighbors.Count == 0) return;

        var tile = neighbors[Random.Range(0, neighbors.Count)];
        Move(tile);
    }

    public bool Move(PuzzleTile tile)
    {
        if (tile == null) return false;

        if (!CanMove(tile.GridPos))
            return false;

        Vector2Int oldPos = tile.GridPos;

        grid[emptyCell.x, emptyCell.y] = tile;
        grid[oldPos.x, oldPos.y] = null;

        tile.SetGridPos(emptyCell.x, emptyCell.y);
        tile.transform.localPosition = GridToLocal(emptyCell.x, emptyCell.y);

        emptyCell = oldPos;
        return true;
    }

    public bool CanMove(Vector2Int tilePos)
    {
        return Vector2Int.Distance(tilePos, emptyCell) == 1;
    }

    private System.Collections.Generic.List<PuzzleTile> GetMovableTiles()
    {
        var list = new System.Collections.Generic.List<PuzzleTile>();

        Vector2Int[] dirs =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        foreach (var d in dirs)
        {
            Vector2Int p = emptyCell + d;
            if (p.x < 0 || p.x >= col || p.y < 0 || p.y >= row)
                continue;

            var t = grid[p.x, p.y];
            if (t != null)
                list.Add(t);
        }

        return list;
    }

    private void RandomizeFlips()
    {
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                var tile = grid[x, y];
                if (tile == null) continue; 

                bool front = Random.value > randomFlipChance;
                tile.SetFront(front);
            }
        }
    }

    public bool IsSolved()
    {
        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < col; x++)
            {
                if (x == emptyCell.x && y == emptyCell.y)
                    continue;

                var tile = grid[x, y];
                if (tile == null)
                    return false;

                if (tile.GridPos != tile.SolvedPos)
                    return false;

                if (!tile.IsFront)
                    return false;
            }
        }

        return true;
    }

    private void CheckWin()
    {
        if (IsSolved())
        {
            Debug.Log("PUZZLE SOLVED");
            OnSolved();
        }
    }

    private void OnSolved()
    {
        enabled = false;

        // TODO:
        // - play sound
        // - highlight board
        // - notify ArcadeModeController
    }

}
