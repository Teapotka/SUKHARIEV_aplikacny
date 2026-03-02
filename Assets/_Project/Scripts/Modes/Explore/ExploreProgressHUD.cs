using BA.Core;
using BA.Core.Progress;
using TMPro;
using UnityEngine;

namespace BA.Modes.Explore
{
    public class ExploreProgressHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] targets; 

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            var ps = ProgressService.Instance;
            var col = GameContext.Instance?.GameData?.ActiveCollection;

            int total = col != null ? col.Count : 0;
            int unlocked = ps != null ? ps.UnlockedCount : 0;

            if (total > 0) unlocked = Mathf.Clamp(unlocked, 0, total);

            string text = $"Unlocked: {unlocked}/{total}";

            if (targets == null) return;
            for (int i = 0; i < targets.Length; i++)
                if (targets[i] != null)
                    targets[i].text = text;
        }
    }
}
