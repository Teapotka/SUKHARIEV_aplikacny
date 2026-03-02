using System.Collections.Generic;
using BA.Data;
using UnityEngine;

namespace BA.Modes.Match
{
    public class CardSpawner : MonoBehaviour
    {
        [SerializeField] private ArtCardView prefab;
        [SerializeField] private Transform parent;
        [SerializeField] private Transform[] spawnPoints;

        private readonly List<ArtCardView> spawned = new();

        public void SpawnCards(List<ArtItemSO> items)
        {
            Clear();

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                var p = spawnPoints != null && spawnPoints.Length > 0
                    ? spawnPoints[i % spawnPoints.Length]
                    : parent;

                var card = Instantiate(prefab, p.position, p.rotation, parent);
                card.SetData(it);
                spawned.Add(card);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < spawned.Count; i++)
                if (spawned[i]) Destroy(spawned[i].gameObject);
            spawned.Clear();
        }
    }
}