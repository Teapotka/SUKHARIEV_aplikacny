using BA.Data;
using TMPro;
using UnityEngine;
using System.Collections;

namespace BA.Modes.Explore
{
    public class ExploreArtworkView : MonoBehaviour
    {
        [Header("Back description text (TMP)")]
        [SerializeField] private TMP_Text backDescriptionText;

        [Header("Flip")]
        [SerializeField] private float flipDuration = 0.35f;

        [Header("Assign the Plane renderer here")]
        [SerializeField] private MeshRenderer paintingRenderer;

        [SerializeField] private ArtItemSO item;

        private Material _runtimeMat;
        private bool _isFlipped;
        private bool _isFlipping;
        private Quaternion _frontRotation;

        public ArtItemSO Item => item;

        public void Awake()
        {
            _frontRotation = transform.localRotation;
        }

        private void Reset()
        {
            if (paintingRenderer == null)
            {
                var t = transform.Find("Plane");
                if (t != null) paintingRenderer = t.GetComponent<MeshRenderer>();
            }
        }

        public void Bind(ArtItemSO artItem)
        {
            item = artItem;
            if (paintingRenderer == null) return;

            if (_runtimeMat == null)
            {
                _runtimeMat = new Material(paintingRenderer.sharedMaterial);
                paintingRenderer.material = _runtimeMat;
            }

            if (paintingRenderer == null)
            {
                Debug.LogWarning($"[ExploreArtworkView] paintingRenderer is NULL on {name}");
                return;
            }

            if (item == null)
            {
                Debug.LogWarning($"[ExploreArtworkView] item is NULL on {name}");
                return;
            }

            if (item.Image == null)
            {
                Debug.LogWarning($"[ExploreArtworkView] item.Image is NULL for id={item.Id} on {name}");
                return;
            }

            if (item != null && item.Image != null)
            {
                _runtimeMat.mainTexture = item.Image.texture;
            }

            if (backDescriptionText != null)
            {
                var tags = item.Tags != null ? string.Join(", ", item.Tags) : "";
                backDescriptionText.text =
                    $"{item.Title}\n{item.Author}\n{item.Style}\n{tags}";
            }

            transform.localRotation = _frontRotation;
            _isFlipped = false;

            gameObject.name = $"Painting_{item?.Id ?? "NULL"}";
        }

        public void ToggleFlip()
        {
            if (_isFlipping) return;

            if (item != null)
                BA.Core.Progress.ProgressService.Instance?.MarkViewed(item.Id, "Explore");

            StartCoroutine(FlipRoutine());
        }

        private IEnumerator FlipRoutine()
        {
            _isFlipping = true;

            var start = transform.localRotation;
            var target = _isFlipped
                ? _frontRotation
                : _frontRotation * Quaternion.Euler(0f, 180f, 0f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, flipDuration);
                transform.localRotation = Quaternion.Slerp(start, target, t);
                yield return null;
            }

            transform.localRotation = target;
            _isFlipped = !_isFlipped;
            _isFlipping = false;
        }
    }
}
