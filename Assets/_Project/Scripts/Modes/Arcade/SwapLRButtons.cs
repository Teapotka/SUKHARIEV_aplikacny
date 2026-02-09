using TMPro;
using UnityEngine;

public class SwapLRButtons : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private RectTransform leftButton;
    [SerializeField] private RectTransform rightButton;

    [Header("Optional: text labels on buttons")]
    [SerializeField] private TMP_Text leftLabel;
    [SerializeField] private TMP_Text rightLabel;

    private Vector2 leftPos;
    private Vector2 rightPos;

    private void Awake()
    {
        if (leftButton) leftPos = leftButton.anchoredPosition;
        if (rightButton) rightPos = rightButton.anchoredPosition;
    }

    public void Apply(bool flipped)
    {
        Debug.Log("[SwapLRButtons] Apply flipped=" + flipped);
        if (!leftButton || !rightButton) return;

        leftButton.anchoredPosition = flipped ? rightPos : leftPos;
        rightButton.anchoredPosition = flipped ? leftPos : rightPos;

        if (leftLabel && rightLabel)
        {
            leftLabel.text = flipped ? "R" : "L";
            rightLabel.text = flipped ? "L" : "R";
        }
    }
}
