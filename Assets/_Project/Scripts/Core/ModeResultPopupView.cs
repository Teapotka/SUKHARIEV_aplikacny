using System;
using TMPro;
using UnityEngine;

namespace BA.UI
{
    public class ModeResultPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        private Action _onContinue;
        private Action _onHome;

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        public void Show(string title, string body, Action onContinue, Action onHome)
        {
            if (titleText) titleText.text = title ?? "";
            if (bodyText) bodyText.text = body ?? "";

            _onContinue = onContinue;
            _onHome = onHome;

            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            _onContinue = null;
            _onHome = null;
        }

        public void OnContinuePressed() => _onContinue?.Invoke();
        public void OnHomePressed() => _onHome?.Invoke();
    }
}