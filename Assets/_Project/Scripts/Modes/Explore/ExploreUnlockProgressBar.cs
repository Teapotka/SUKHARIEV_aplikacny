using BA.Core;
using BA.Core.Progress;
using UnityEngine;
using UnityEngine.UI;

namespace BA.Modes.Explore
{
    public class ExploreUnlockProgressBar : MonoBehaviour
    {
        [Header("Unlocked bar (Minimal/Gamified or only Gamified)")]
        [SerializeField] private Image unlockedBar;

        [Header("Optional")]
        [SerializeField] private bool updateEveryFrame = false;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (updateEveryFrame) Refresh();
        }

        public void Refresh()
        {
            var ps = ProgressService.Instance;
            var col = GameContext.Instance?.GameData?.ActiveCollection;

            if (ps == null || col == null || col.Count <= 0) return;

            int total = col.Count;
            int unlocked = Mathf.Clamp(ps.UnlockedCount, 0, total);

            unlockedBar.fillAmount = total > 0 ? (unlocked / (float)total) : 0f;
        }
    }
}
