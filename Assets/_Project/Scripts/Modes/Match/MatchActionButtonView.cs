using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BA.Modes.Match
{
    public class MatchActionButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;

        private void Reset()
        {
            if (button == null) button = GetComponent<Button>();
            if (label == null) label = GetComponentInChildren<TMP_Text>();
        }

        public void SetText(string text)
        {
            if (label) label.text = text;
        }

        public void SetInteractable(bool value)
        {
            if (button) button.interactable = value;
        }
    }
}
