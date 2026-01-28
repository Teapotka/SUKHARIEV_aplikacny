using UnityEngine;
using BA.Telemetry;

namespace BA.Core.Settings
{
    public class SettingsService : MonoBehaviour
    {
        public static SettingsService Instance { get; private set; }

        private const string Key = "BA_APP_SETTINGS_V1";

        public AppSettings Current { get; private set; } = new AppSettings();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        public void SetUiVariant(UiVariant variant)
        {
            var old = Current.uiVariant;
            Current.uiVariant = variant;
            Save();

            TelemetryService.Instance?.SetUiVariant(variant.ToString());
        }

        public void SetProfileId(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            Current.profileId = profileId.Trim();
            Save();
        }

        private void Save()
        {
            var json = JsonUtility.ToJson(Current);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return;

            var json = PlayerPrefs.GetString(Key);
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                Current = JsonUtility.FromJson<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }
        }
    }
}
