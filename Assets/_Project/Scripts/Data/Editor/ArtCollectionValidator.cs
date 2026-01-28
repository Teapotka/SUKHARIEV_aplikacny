#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BA.Data.Editor
{
    [CustomEditor(typeof(ArtCollectionSO))]
    public class ArtCollectionValidator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var collection = (ArtCollectionSO)target;

            EditorGUILayout.Space(12);
            if (GUILayout.Button("Validate Collection"))
            {
                Validate(collection);
            }
        }

        private static void Validate(ArtCollectionSO collection)
        {
            if (collection == null)
                return;

            var items = collection.Items;
            var idSet = new HashSet<string>();
            int errorCount = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    Debug.LogWarning($"[Collection Validate] Null item at index {i} in {collection.name}", collection);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    Debug.LogError($"[Collection Validate] Missing ID: {item.name}", item);
                    errorCount++;
                }
                else if (!idSet.Add(item.Id))
                {
                    Debug.LogError($"[Collection Validate] Duplicate ID '{item.Id}': {item.name}", item);
                    errorCount++;
                }

                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    Debug.LogWarning($"[Collection Validate] Empty Title: {item.Id} ({item.name})", item);
                }

                if (item.Image == null)
                {
                    Debug.LogWarning($"[Collection Validate] Missing Image: {item.Id} ({item.name})", item);
                }
            }

            if (errorCount == 0)
                Debug.Log($"[Collection Validate] OK: {collection.name} ({items.Count} items)", collection);
            else
                Debug.LogError($"[Collection Validate] FAILED: {collection.name} — errors: {errorCount}", collection);
        }
    }
}
#endif
