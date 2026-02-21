using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BA.Modes.Arcade
{
    public class ArcadeScoreView : MonoBehaviour
    {
        [Header("Live score label (Gamified UI)")]
        [SerializeField] private Text liveScoreText;
        [SerializeField] private TMP_Text liveScoreTmp;

        [Header("Final score label (Gamified UI)")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private TMP_Text finalScoreTmp;

        [Header("Medal label (Gamified UI)")]
        [SerializeField] private Text medalText;
        [SerializeField] private TMP_Text medalTmp;

        [Header("Optional: show final block only on finish")]
        [SerializeField] private GameObject finalRoot;

        private void Awake()
        {
            
        }

        public void ResetView()
        {
            SetLiveScore(0);

            SetText(finalScoreText, finalScoreTmp, "");
            SetText(medalText, medalTmp, "");

            if (finalRoot != null)
                finalRoot.SetActive(false);
        }

        public void SetLiveScore(int score)
        {
            score = Mathf.Max(0, score);
            SetText(liveScoreText, liveScoreTmp, $"Score: {score}");
        }

        public void SetFinalScore(int score, string medal)
        {
            score = Mathf.Max(0, score);
            medal ??= "";

            SetText(finalScoreText, finalScoreTmp, $"Final: {score}");
            SetText(medalText, medalTmp, medal);

            if (finalRoot != null)
                finalRoot.SetActive(true);
        }

        private static void SetText(Text ui, TMP_Text tmp, string value)
        {
            if (ui != null) ui.text = value;
            if (tmp != null) tmp.text = value;
        }
    }
}