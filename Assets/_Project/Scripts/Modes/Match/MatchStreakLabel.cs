using BA.Core.Progress;
using TMPro;
using UnityEngine;

namespace BA.Modes.Match
{
    public class MatchStreakLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] targets; // minimal + gamified

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            int streak = ProgressService.Instance != null ? ProgressService.Instance.MatchStreak : 0;
            string text = $"Streak: x{streak}";

            if (targets == null) return;
            for (int i = 0; i < targets.Length; i++)
                if (targets[i] != null)
                    targets[i].text = text;
        }
    }
}
