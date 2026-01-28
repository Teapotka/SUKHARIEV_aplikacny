using UnityEngine;
using TMPro;
using BA.Core.Settings;

namespace BA.UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField profileIdInput;

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
