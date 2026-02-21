using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "01_MainMenu";

    public void GoHome()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
