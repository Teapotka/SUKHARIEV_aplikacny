using TMPro;
using UnityEngine;

namespace BA.Modes.Arcade
{
    public class ArcadeSideLabels : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Text body;

        [Header("Text when viewing FRONT")]
        [TextArea][SerializeField] private string frontHeader = "Front";
        [TextArea][SerializeField] private string frontBody = "Slide tiles to complete the portrait.";

        [Header("Text when viewing BACK")]
        [TextArea][SerializeField] private string backHeader = "Back";
        [TextArea][SerializeField] private string backBody = "Flip tiles to align the backside image.";

        [Header("Transform")]
        [SerializeField] private Transform labelRoot;
        [SerializeField] private Vector3 frontEuler = new Vector3(0f, 0f, 0f);
        [SerializeField] private Vector3 backEuler = new Vector3(0f, 180f, 0f);

        [Header("Positions (LOCAL)")]
        [SerializeField] private Vector3 frontLocalPos = new Vector3(+6f, 0f, 0f);
        [SerializeField] private Vector3 backLocalPos = new Vector3(-6f, 0f, 0f);

        private Coroutine moveCo;

        public void SetBackView(bool isBack, float moveDuration = 0.6f)
        {
            if (labelRoot == null) return;

            labelRoot.localRotation = Quaternion.Euler(isBack ? backEuler : frontEuler);

            if (header) header.text = isBack ? backHeader : frontHeader;
            if (body) body.text = isBack ? backBody : frontBody;

            if (moveCo != null) StopCoroutine(moveCo);
            moveCo = StartCoroutine(MoveRoutine(isBack ? backLocalPos : frontLocalPos, moveDuration));
        }

        private System.Collections.IEnumerator MoveRoutine(Vector3 targetLocalPos, float duration)
        {
            Vector3 start = labelRoot.localPosition;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, duration);
                labelRoot.localPosition = Vector3.Lerp(start, targetLocalPos, t);
                yield return null;
            }

            labelRoot.localPosition = targetLocalPos;
        }

        public void Configure(string frontText, string backText)
        {
            frontBody = frontText;
            backBody = backText;
        }
    }
}
