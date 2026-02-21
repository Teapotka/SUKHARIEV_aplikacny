using System;
using UnityEngine;
using BA.Core.Settings;

namespace BA.UI
{
    public class UIThemeManager : MonoBehaviour
    {
        public static UIThemeManager Instance { get; private set; }

        [Header("Assign both Theme assets here")]
        [SerializeField] private UIThemeSO minimalTheme;
        [SerializeField] private UIThemeSO gamifiedTheme;

        public UIThemeSO CurrentTheme { get; private set; }

        public event Action<UIThemeSO> OnThemeChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplyFromSettings();
        }

        public void ApplyFromSettings()
        {

            Debug.LogError("[UIThemeManager] instamce: " + SettingsService.Instance);

            var variant = SettingsService.Instance != null
                ? SettingsService.Instance.Current.uiVariant
                : UiVariant.Gamified;

            Apply(variant);
        }

        public void Apply(UiVariant variant)
        {
            UIThemeSO next = variant == UiVariant.Minimal ? minimalTheme : gamifiedTheme;

            if (next == null)
            {
                Debug.LogError("[UIThemeManager] Missing theme asset for: " + variant);
                return;
            }

            if (CurrentTheme == next) return;
            Debug.LogError("[UIThemeManager] Theme " + variant);

            CurrentTheme = next;
            OnThemeChanged?.Invoke(CurrentTheme);
        }

        public void ToggleVariant()
        {
            if (SettingsService.Instance == null) return;

            var cur = SettingsService.Instance.Current.uiVariant;
            var next = cur == UiVariant.Gamified ? UiVariant.Minimal : UiVariant.Gamified;

            SettingsService.Instance.SetUiVariant(next);
            Apply(next);
        }
    }
}
