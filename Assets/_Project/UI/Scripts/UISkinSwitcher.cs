using UnityEngine;
using BA.Core.Settings;

namespace BA.UI
{
    public class UISkinSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject minimalRoot;
        [SerializeField] private GameObject gamifiedRoot;

        private void Start()
        {
            ApplyFromSettings();
        }

        public void ApplyFromSettings()
        {
            var variant = SettingsService.Instance != null
                ? SettingsService.Instance.Current.uiVariant
                : UiVariant.Gamified;

            Debug.LogError("[UISkinSwitcher] Theme " + variant);


            if (minimalRoot) minimalRoot.SetActive(variant == UiVariant.Minimal);
            if (gamifiedRoot) gamifiedRoot.SetActive(variant == UiVariant.Gamified);
        }
    }
}
