using System;
using System.Collections.Generic;
using UnityEngine;

namespace BA.Data
{
    [CreateAssetMenu(menuName = "BA/Data/Art Item", fileName = "SO_ArtItem_")]
    public class ArtItemSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string author;

        [Header("Classification")]
        [SerializeField] private string style;
        [SerializeField] private string[] tags;

        [Header("Visual")]
        [SerializeField] private Sprite image;

        public string Id => id;
        public string Title => title;
        public string Author => author;
        public string Style => style;
        public IReadOnlyList<string> Tags => tags;
        public Sprite Image => image;

        private void OnValidate()
        {
            id = id?.Trim();
            title = title?.Trim();
            author = author?.Trim();
            style = style?.Trim();

            if (tags != null)
            {
                for (int i = 0; i < tags.Length; i++)
                    tags[i] = tags[i]?.Trim();
            }
        }
    }
}
