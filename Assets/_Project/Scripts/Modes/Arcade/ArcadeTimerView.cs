using TMPro;
using UnityEngine;

namespace BA.Modes.Arcade
{
    public class ArcadeTimerView : MonoBehaviour
    {
        [Header("Assign BOTH skins here (minimal + gamified). Can be 1 or many.")]
        [SerializeField] private TMP_Text[] timerTexts;

        private void Reset()
        {
            timerTexts = GetComponentsInChildren<TMP_Text>(true);
        }

        public void SetSeconds(float seconds)
        {

            Debug.Log($"[ArcadeTimerView] SetSeconds({seconds}) on {name}", this);

            seconds = Mathf.Max(0f, seconds);

            int total = Mathf.CeilToInt(seconds);
            int mm = total / 60;
            int ss = total % 60;

            SetTextInternal($"{mm:00}:{ss:00}");
        }

        public void SetText(string text)
        {
            SetTextInternal(text);
        }

        private void SetTextInternal(string text)
        {
            if (timerTexts == null) return;

            for (int i = 0; i < timerTexts.Length; i++)
            {
                if (timerTexts[i] == null) continue;
                timerTexts[i].text = text;
            }
        }
    }
}
