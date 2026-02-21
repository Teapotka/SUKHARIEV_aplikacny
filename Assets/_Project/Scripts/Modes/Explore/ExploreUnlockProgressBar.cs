using BA.Core;
using BA.Core.Progress;
using UnityEngine;

namespace BA.Modes.Explore
{
    public class ExploreUnlockProgressBar : MonoBehaviour
    {
        [Header("Bar images (Minimal/Gamified or only Gamified)")]
        [SerializeField] private RectTransform[] barRects;

        [Header("Width at 100% unlocked (px)")]
        [SerializeField] private float maxWidth = 265f;

        [Header("Keep height fixed (px)")]
        [SerializeField] private float fixedHeight = 6f;

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

            float t = total > 0 ? (unlocked / (float)total) : 0f;
            float w = Mathf.Clamp01(t) * maxWidth;

            if (barRects == null) return;

            for (int i = 0; i < barRects.Length; i++)
            {
                var rt = barRects[i];
                if (rt == null) continue;

                var size = rt.sizeDelta;
                size.x = w;
                size.y = fixedHeight;
                rt.sizeDelta = size;
            }
        }
    }
}
