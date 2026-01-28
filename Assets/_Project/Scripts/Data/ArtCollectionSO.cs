using System.Collections.Generic;
using UnityEngine;

namespace BA.Data
{
    [CreateAssetMenu(menuName = "BA/Data/Art Collection", fileName = "SO_ArtCollection_")]
    public class ArtCollectionSO : ScriptableObject
    {
        [SerializeField] private List<ArtItemSO> items = new();

        public IReadOnlyList<ArtItemSO> Items => items;

        public int Count => items?.Count ?? 0;

        public ArtItemSO GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || items == null) return null;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it != null && it.Id == id) return it;
            }
            return null;
        }
    }
}
