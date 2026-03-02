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
            SceneManager.LoadScene(exploreScene);
        }

        public void PlayMatch()
        {
            SceneManager.LoadScene(matchScene);
        }

        public void PlayArcade()
        {
            SceneManager.LoadScene(arcadeScene);
        }

        public void PlayHistory()
        {
            SceneManager.LoadScene(historyScene);
        }

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
