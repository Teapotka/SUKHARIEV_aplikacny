using UnityEngine;
using UnityEngine.SceneManagement;
using BA.Telemetry;
using BA.Data;

namespace BA.UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string exploreScene = "10_Explore";
        [SerializeField] private string matchScene = "20_Match";
        [SerializeField] private string arcadeScene = "30_Arcade";
        [SerializeField] private string historyScene = "02_ModeHistory";


        [Header("Data")]
        [SerializeField] private GameDataSO gameData;

        public void PlayExplore()
        {
            //LogModeStart("Explore");
            SceneManager.LoadScene(exploreScene);
        }

        public void PlayMatch()
        {
            //LogModeStart("Match");
            SceneManager.LoadScene(matchScene);
        }

        public void PlayArcade()
        {
            //LogModeStart("Arcade");
            SceneManager.LoadScene(arcadeScene);
        }

        public void PlayHistory()
        {
            //LogModeStart("Arcade");
            SceneManager.LoadScene(historyScene);
        }

        //private void LogModeStart(string mode)
        //{
        //    if (TelemetryService.Instance == null || gameData == null) return;

        //    var cfg = mode switch
        //    {
        //        "Explore" => gameData.GetConfig(GameModeType.Explore),
        //        "Match" => gameData.GetConfig(GameModeType.Match),
        //        "Arcade" => gameData.GetConfig(GameModeType.Arcade),
        //        _ => null
        //    };

        //    if (cfg == null) return;

        //    TelemetryService.Instance.Log(
        //        TelemetryEventType.MODE_START,
        //        mode,
        //        new ModeStartPayload
        //        {
        //            itemCount = cfg.BaseItemCount,
        //            timeLimitSeconds = cfg.TimeLimitSeconds,
        //            helpEnabled = cfg.HelpEnabled
        //        });

        //    TelemetryService.Instance.Flush();
        //}

        public void Quit()
        {
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
        }
    }
}
