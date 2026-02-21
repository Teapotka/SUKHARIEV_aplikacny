using UnityEngine;
using BA.Core;
using BA.Core.Progress;
using BA.Data;

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
            var collection = GameContext.Instance?.GameData?.ActiveCollection;
            ProgressService.Instance?.RegisterCollection(collection);
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

            if (ProgressService.Instance != null)
                ProgressService.Instance.EnsureExploreInitializedForActiveCollection();

            var unlockedIds = ProgressService.Instance != null
                ? ProgressService.Instance.GetUnlockedExploreItemIds()
                : null;

            if (unlockedIds == null || unlockedIds.Count == 0)
            {
                Debug.LogWarning("[ExploreGallerySpawner] No unlocked IDs. Falling back to first 3 items.");
                unlockedIds = new System.Collections.Generic.List<string>();
                int fallback = Mathf.Min(3, collection.Count);
                for (int i = 0; i < fallback; i++)
                    if (collection.Items[i] != null && !string.IsNullOrWhiteSpace(collection.Items[i].Id))
                        unlockedIds.Add(collection.Items[i].Id);
            }

            ClearSlots();

            int hardMax = Mathf.Min(10, collection.Count, slots.Length);
            int spawnCount = Mathf.Min(unlockedIds.Count, hardMax);

            for (int i = 0; i < spawnCount; i++)
            {
                var id = unlockedIds[i];
                if (string.IsNullOrWhiteSpace(id)) continue;

                ArtItemSO item = collection.GetById(id);
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

        private void ClearSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s == null) continue;

                for (int c = s.childCount - 1; c >= 0; c--)
                    Destroy(s.GetChild(c).gameObject);
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
