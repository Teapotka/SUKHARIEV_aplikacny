using UnityEngine;

public class ProgressDebugDump : MonoBehaviour
{
    [SerializeField] private string progressKey = "BA_PROGRESS_V2";

    [ContextMenu("Dump Progress JSON")]
    public void Dump()
    {
        Debug.Log($"ProgressKey={progressKey}");
        Debug.Log(PlayerPrefs.HasKey(progressKey)
            ? PlayerPrefs.GetString(progressKey)
            : "NO PlayerPrefs key found");
    }

    [ContextMenu("Delete Progress JSON")]
    public void Delete()
    {
        PlayerPrefs.DeleteKey(progressKey);
        PlayerPrefs.Save();
        Debug.Log("Deleted progress key.");
    }
}
