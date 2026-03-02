using UnityEngine;
using UnityEngine.SceneManagement;
using BA.Telemetry;
using BA.Core.Settings;

namespace BA.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private string firstScene = "01_MainMenu";
        [SerializeField] private string appVersion = "0.1.0";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            var settings = SettingsService.Instance?.Current;

            var uiVariant = settings?.uiVariant.ToString() ?? "Minimal";
            var profileId = settings?.profileId ?? "anon";

            if (TelemetryService.Instance != null)
            {
                TelemetryService.Instance.StartSession(uiVariant, profileId, appVersion);

                Debug.Log("PersistentDataPath: " + Application.persistentDataPath);
            }
            else
            {
                Debug.LogWarning("[Bootstrapper] TelemetryService not found in scene.");
            }

            SceneManager.LoadScene(firstScene);
        }
    }
}
