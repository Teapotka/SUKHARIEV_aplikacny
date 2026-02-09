using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }

    private PuzzleBoard board;
    private Renderer rend;
    public Vector2Int SolvedPos { get; private set; }
    public bool IsFront => isFront;
    private bool isFront = true;
    private bool isSelected;

    private MaterialPropertyBlock mpb;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    public void Init(PuzzleBoard board, int x, int y, int col, int row)
    {
        this.board = board;
        GridPos = new Vector2Int(x, y);
        SolvedPos = new Vector2Int(x, y);

        var mats = rend.materials;
        if (mats == null || mats.Length == 0) return;

        int frontIndex = 2;
        int backIndex = 0;

        Vector2 tileScale = new Vector2(1f / col, 1f / row);
        Vector2 tileOffset = new Vector2(x / (float)col, (row - 1 - y) / (float)row);

        float boardAspect = col / (float)row;
        float imageAspect = boardAspect;
        Texture tex = mats[frontIndex].GetTexture("_BaseMap");
        if (tex is Texture2D t2d)
            imageAspect = t2d.width / (float)t2d.height;

        float cropScaleX = 1f, cropScaleY = 1f, cropOffX = 0f, cropOffY = 0f;

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
        Vector2 finalOffset = new Vector2(
            cropOffX + tileOffset.x * cropScaleX,
            cropOffY + tileOffset.y * cropScaleY
        );

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

    public void SetFront(bool front)
    {
        if (isFront == front) return;

        isFront = front;
        transform.localRotation = front
            ? Quaternion.identity
            : Quaternion.Euler(0f, 270f, -90f);
    }

    public void SetGridPos(int x, int y)
    {
        GridPos = new Vector2Int(x, y);
    }

    void OnMouseDown()
    {
        board.SelectTile(this);
    }


    public void Select()
    {
        isSelected = true;
        SetHighlight(true);
    }

    public void Deselect()
    {
        isSelected = false;
        SetHighlight(false);
    }

    private void SetHighlight(bool on)
    {
        rend.GetPropertyBlock(mpb);
        mpb.SetColor(EmissionColor, on ? Color.cyan * 2.5f : Color.black);
        rend.SetPropertyBlock(mpb);
    }


    public void Flip()
    {
        isFront = !isFront;
        transform.Rotate(Vector3.right * 180f);
    }

    public void SetTexturesForMaterials(Texture front, Texture back)
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();

        var mats = rend.materials;
        if (mats == null || mats.Length == 0) return;

        int frontIndex = 2;
        int backIndex = 0;

        if (front != null && mats.Length > frontIndex)
            mats[frontIndex].SetTexture("_BaseMap", front);

        if (back != null && mats.Length > backIndex)
            mats[backIndex].SetTexture("_BaseMap", back);
    }
}
