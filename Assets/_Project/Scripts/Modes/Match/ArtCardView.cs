using BA.Data;
using UnityEngine;

namespace BA.Modes.Match
{
    public class ArtCardView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer paintingRenderer;
        [SerializeField] private ArtItemSO item;

        private Material _runtimeMat;

        public ArtItemSO Item => item;

        private void Reset()
        {
            if (paintingRenderer == null)
            {
                var t = transform.Find("Plane");
                if (t != null) paintingRenderer = t.GetComponent<MeshRenderer>();
            }
        }

        public void SetData(ArtItemSO artItem)
        {
            item = artItem;

            if (paintingRenderer == null)
            {
                Debug.LogWarning($"[ArtCardView] paintingRenderer is NULL on {name}");
                return;
            }

            if (_runtimeMat == null)
            {
                _runtimeMat = new Material(paintingRenderer.sharedMaterial);
                paintingRenderer.material = _runtimeMat;
            }

            if (item == null || item.Image == null)
            {
                Debug.LogWarning($"[ArtCardView] item or item.Image is NULL on {name}");
                _runtimeMat.mainTexture = null;
                return;
            }

            _runtimeMat.mainTexture = item.Image.texture;

            gameObject.name = $"Card_{item.Id}";
        }
    }
}
