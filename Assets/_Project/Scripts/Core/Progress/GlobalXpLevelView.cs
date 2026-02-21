using BA.Core.Progress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BA.UI
{
    public class GlobalXpLevelView : MonoBehaviour
    {
        [Header("UI refs")]
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text xpLabel;
        [SerializeField] private Image xpFill;

        private void OnEnable()
        {
            if (ProgressService.Instance != null)
                ProgressService.Instance.MetaChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (ProgressService.Instance != null)
                ProgressService.Instance.MetaChanged -= Refresh;
        }

        public void Refresh()
        {
            var p = ProgressService.Instance;
            if (p == null) return;

            if (levelLabel != null)
                levelLabel.text = $"LVL {p.Level}";

            if (xpLabel != null)
                xpLabel.text = $"{p.XpInCurrentLevel}/{Mathf.Max(1, GetXpPerLevel(p))} XP";

            if (xpFill != null)
                xpFill.fillAmount = Mathf.Clamp01(p.XpProgress01);
        }

        private static int GetXpPerLevel(ProgressService p)
        {
            return Mathf.Max(1, p.XpInCurrentLevel + p.XpToNextLevel);
        }
    }
}