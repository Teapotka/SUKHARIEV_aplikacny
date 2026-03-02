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



            if (minimalRoot) minimalRoot.SetActive(variant == UiVariant.Minimal);
            if (gamifiedRoot) gamifiedRoot.SetActive(variant == UiVariant.Gamified);
        }
    }
}
