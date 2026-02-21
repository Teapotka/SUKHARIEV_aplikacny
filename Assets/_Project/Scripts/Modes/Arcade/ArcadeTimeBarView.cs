using UnityEngine;
using UnityEngine.UI;

namespace BA.Modes.Arcade
{
    public class ArcadeTimeBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private void Reset()
        {
            fillImage = GetComponent<Image>();
        }

        public void SetNormalized(float t01)
        {
            if (fillImage == null) return;
            fillImage.fillAmount = Mathf.Clamp01(t01);
        }

        public void SetSeconds(float timeLeft, float timeLimit)
        {
            float limit = Mathf.Max(0.01f, timeLimit);
            SetNormalized(timeLeft / limit);
        }
    }
}