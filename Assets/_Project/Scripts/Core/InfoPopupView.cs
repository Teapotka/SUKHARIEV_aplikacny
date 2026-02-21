using TMPro;
using UnityEngine;

namespace BA.UI
{
    public class InfoPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        public void Show(string title, string body)
        {
            if (titleText) titleText.text = title ?? "";
            if (bodyText) bodyText.text = body ?? "";
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public void OnClosePressed() => Hide();
    }
}