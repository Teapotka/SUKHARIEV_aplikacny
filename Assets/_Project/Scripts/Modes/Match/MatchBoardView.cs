using TMPro;
using UnityEngine;

namespace BA.Modes.Match
{
    public class MatchBoardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text resultText;

        public void SetPrompt(string prompt)
        {
            if (promptText) promptText.text = prompt ?? "";
            if (resultText) resultText.text = "";
        }

        public void SetResult(bool success, int incorrect)
        {
            if (!resultText) return;
            resultText.text = success ? "Correct!" : $"Not quite. Incorrect: {incorrect}";
        }
    }
}
