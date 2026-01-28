using UnityEngine;
using BA.Core;
using BA.Core.Progress;

namespace BA.Modes.Explore
{
    public class ExploreGallerySpawner : MonoBehaviour
    {
        [Header("Frame prefabs (4 variants)")]
        [SerializeField] private ExploreArtworkView[] framePrefabs;

        [Header("Slots on the wall (size 10)")]
        [SerializeField] private Transform[] slots;

        [Header("Determinism")]
        [SerializeField] private int randomSeed = 12345;
        [SerializeField] private bool randomFramePerItem = true;

        private System.Random _rng;

        private void Start()
        {
            _rng = new System.Random(randomSeed);
            SpawnUnlocked();
        }

        public void SpawnUnlocked()
        {
            var collection = GameContext.Instance?.GameData?.ActiveCollection;

            if (collection == null)
            {
                Debug.LogWarning("[ExploreGallerySpawner] ActiveCollection is null.");
                return;
            }

            if (slots == null || slots.Length == 0)
            {
                Debug.LogWarning("[ExploreGallerySpawner] Slots not assigned.");
                return;
            }

            if (framePrefabs == null || framePrefabs.Length == 0)
            {
                Debug.LogWarning("[ExploreGallerySpawner] Frame prefabs not assigned.");
                return;
            }

            // Clear old
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                for (int c = slots[i].childCount - 1; c >= 0; c--)
                    Destroy(slots[i].GetChild(c).gameObject);
            }

            int unlocked = ProgressService.Instance != null ? ProgressService.Instance.UnlockedCount : 5;

            int hardMax = Mathf.Min(10, collection.Count, slots.Length);
            int spawnCount = Mathf.Min(unlocked, hardMax);

            for (int i = 0; i < spawnCount; i++)
            {
                var item = collection.Items[i];
                if (item == null) continue;

                var slot = slots[i];
                if (slot == null) continue;

                var prefab = PickFramePrefab();

                var view = Instantiate(prefab, slot);
                view.transform.localPosition = Vector3.zero;
                view.transform.localRotation = Quaternion.identity;
                view.transform.localScale = Vector3.one;

                view.Bind(item);
            }
        }

        private ExploreArtworkView PickFramePrefab()
        {
            if (!randomFramePerItem)
                return framePrefabs[0];

            int idx = _rng.Next(0, framePrefabs.Length);
            return framePrefabs[idx];
        }
    }
}
