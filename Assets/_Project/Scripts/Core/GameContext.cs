using UnityEngine;
using BA.Data;

namespace BA.Core
{
    public class GameContext : MonoBehaviour
    {
        public static GameContext Instance { get; private set; }

        [SerializeField] private GameDataSO gameData;
        public GameDataSO GameData => gameData;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
