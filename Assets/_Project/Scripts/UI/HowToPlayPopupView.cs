using UnityEngine;

namespace BA.UI
{
    public class HowToPlayPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject dim;


        private void Awake()
        {
            if (root != null)
                root.SetActive(false);
                dim.SetActive(false);
        }

        public void Show()
        {
            if (root != null)
                root.SetActive(true);
                dim.SetActive(true);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
                dim.SetActive(false);
        }

        public void Toggle()
        {
            if (root != null)
                root.SetActive(!root.activeSelf);
                dim.SetActive(!dim.activeSelf);
        }

        public void OnClosePressed()
        {
            Hide();
        }
    }
}