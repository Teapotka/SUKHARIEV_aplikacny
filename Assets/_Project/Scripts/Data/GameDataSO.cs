using UnityEngine;

namespace BA.Data
{
    [CreateAssetMenu(menuName = "BA/Data/Game Data", fileName = "SO_GameData")]
    public class GameDataSO : ScriptableObject
    {
        [SerializeField] private ArtCollectionSO activeCollection;

        [Header("Mode configs")]
        [SerializeField] private ModeConfigSO exploreConfig;
        [SerializeField] private ModeConfigSO matchConfig;
        [SerializeField] private ModeConfigSO arcadeConfig;

        public ArtCollectionSO ActiveCollection => activeCollection;

        public ModeConfigSO GetConfig(GameModeType mode) => mode switch
        {
            GameModeType.Explore => exploreConfig,
            GameModeType.Match => matchConfig,
            GameModeType.Arcade => arcadeConfig,
            _ => null
        };
    }
}
