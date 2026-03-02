using UnityEngine;

namespace BA.Data
{
    [CreateAssetMenu(menuName = "BA/Data/Game Data", fileName = "SO_GameData")]
    public class GameDataSO : ScriptableObject
    {
        [SerializeField] private ArtCollectionSO activeCollection;

        public ArtCollectionSO ActiveCollection => activeCollection;
    }
}
