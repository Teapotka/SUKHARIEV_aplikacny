using UnityEngine;
using TMPro;
using BA.Core.Settings;
using UnityEngine.UI;

namespace BA.UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField profileIdInput;

        [SerializeField] private Toggle gamifiedToggle;
        [SerializeField] private Toggle minimalToggle;

        public void OnGamifiedToggleChanged(bool isOn)
        {
            if (!isOn) return;
            SettingsService.Instance?.SetUiVariant(UiVariant.Gamified);


        }

        public void OnMinimalToggleChanged(bool isOn)
        {
            if (!isOn) return;
            SettingsService.Instance?.SetUiVariant(UiVariant.Minimal);


        }

        public void SetMinimal()
        {
            SettingsService.Instance?.SetUiVariant(UiVariant.Minimal);
        }

        public void SetGamified()
        {
            SettingsService.Instance?.SetUiVariant(UiVariant.Gamified);
        }

        public void ApplyProfileId()
        {
            if (profileIdInput == null) return;
            SettingsService.Instance?.SetProfileId(profileIdInput.text);
        }

        private void OnEnable()
        {
            if (profileIdInput != null)
                profileIdInput.text = SettingsService.Instance?.Current.profileId ?? "anon";

            var v = SettingsService.Instance != null ? SettingsService.Instance.Current.uiVariant : UiVariant.Gamified;

            if (gamifiedToggle != null) gamifiedToggle.SetIsOnWithoutNotify(v == UiVariant.Gamified);
            if (minimalToggle != null) minimalToggle.SetIsOnWithoutNotify(v == UiVariant.Minimal);


        }

        private void Start()
        {
            if (profileIdInput != null)
                profileIdInput.text = SettingsService.Instance?.Current.profileId ?? "anon";
        }

        public void OnUiVariantDropdownChanged(int index)
        {

            if (index == 0)
                SettingsService.Instance?.SetUiVariant(UiVariant.Minimal);
            else
                SettingsService.Instance?.SetUiVariant(UiVariant.Gamified);


        }

    }
}
