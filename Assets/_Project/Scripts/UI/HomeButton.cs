using UnityEngine;
using UnityEngine.SceneManagement;

namespace BA.UI
{
    public class HomeButton : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "01_MainMenu";

        public void GoHome()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
