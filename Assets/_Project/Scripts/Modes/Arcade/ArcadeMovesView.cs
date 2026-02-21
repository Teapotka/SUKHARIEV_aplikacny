using TMPro;
using UnityEngine;

namespace BA.Modes.Arcade
{
    public class ArcadeMovesView : MonoBehaviour
    {
        [Tooltip("Assign move TMP labels here (you can put 1 label, or 2 labels for both skins).")]
        [SerializeField] private TMP_Text[] labels;

        [Tooltip("Optional format, e.g. 'Moves: {0}' or just '{0}'.")]
        [SerializeField] private string format = "Moves: {0}";

        public void SetMoves(int moves)
        {
            string value = moves >= 999 ? "999+" : Mathf.Max(0, moves).ToString();
            string text = string.IsNullOrWhiteSpace(format) ? value : string.Format(format, value);

            if (labels == null) return;

            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null)
                    labels[i].text = text;
            }
        }
    }
}
