using BA.UI;
using UnityEngine;

namespace BA.UI
{
    public class ModeInfoButton : MonoBehaviour
    {
        [Header("Popup (optional). If null, it will Find in scene.")]
        [SerializeField] private InfoPopupView popup;

        [Header("Content")]
        [SerializeField] private string title = "Info";
        [TextArea(3, 12)]
        [SerializeField] private string body = "";

        public void OnPressed()
        {
            if (popup == null)
                popup = Object.FindFirstObjectByType<InfoPopupView>(FindObjectsInactive.Include);

            if (popup == null)
            {
                Debug.LogWarning("[ModeInfoButton] No InfoPopupView found in scene.");
                return;
            }

            popup.Show(title, body);
        }
    }
}