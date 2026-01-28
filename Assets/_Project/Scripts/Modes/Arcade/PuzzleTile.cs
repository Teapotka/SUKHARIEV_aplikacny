using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }

    private PuzzleBoard board;
    private Renderer rend;
    private bool isFront = true;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void Init(PuzzleBoard board, int x, int y, int col, int row)
    {
        this.board = board;
        GridPos = new Vector2Int(x, y);

        var mats = rend.materials;
        if (mats == null || mats.Length == 0) return;

        // --- Which material is front/back ---
        int frontIndex = 2;
        int backIndex = 0;

        Vector2 tileScale = new Vector2(1f / col, 1f / row);
        Vector2 tileOffset = new Vector2(x / (float)col, (row - 1 - y) / (float)row);

        float boardAspect = col / (float)row; 

        float imageAspect = boardAspect;
        Texture tex = mats[frontIndex].GetTexture("_BaseMap");
        if (tex is Texture2D t2d)
            imageAspect = t2d.width / (float)t2d.height;


        float cropScaleX = 1f;
        float cropScaleY = 1f;
        float cropOffX = 0f;
        float cropOffY = 0f;

        if (imageAspect > boardAspect)
        {
            cropScaleX = boardAspect / imageAspect;     
            cropOffX = (1f - cropScaleX) * 0.5f;      
        }
        else if (imageAspect < boardAspect)
        {
            cropScaleY = imageAspect / boardAspect;    
            cropOffY = (1f - cropScaleY) * 0.5f;      
        }

        Vector2 finalScale = new Vector2(tileScale.x * cropScaleX, tileScale.y * cropScaleY);
        Vector2 finalOffset = new Vector2(cropOffX + tileOffset.x * cropScaleX,
                                          cropOffY + tileOffset.y * cropScaleY);

        mats[frontIndex].SetTextureScale("_BaseMap", finalScale);
        mats[frontIndex].SetTextureOffset("_BaseMap", finalOffset);

        Vector2 backOffset = finalOffset;
        backOffset.x = 1f - finalScale.x - finalOffset.x;

        if (mats.Length > backIndex && mats[backIndex].GetTexture("_BaseMap") != null)
        {
            mats[backIndex].SetTextureScale("_BaseMap", finalScale);
            mats[backIndex].SetTextureOffset("_BaseMap", backOffset);
        }
    }

    public void SetGridPos(int x, int y)
    {
        GridPos = new Vector2Int(x, y);
    }

    void OnMouseDown()
    {
        board.Move(this);
    }

    public void Flip()
    {
        isFront = !isFront;
        transform.Rotate(Vector3.up * 180f);
    }
}
