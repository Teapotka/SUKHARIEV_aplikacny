using UnityEngine;

namespace BA.Data
{
    public enum GameModeType
    {
        Explore,
        Match,
        Arcade
    }

    [CreateAssetMenu(menuName = "BA/Data/Mode Config", fileName = "SO_ModeConfig_")]
    public class ModeConfigSO : ScriptableObject
    {
        [Header("Mode")]
        [SerializeField] private GameModeType mode;

        [Header("Core difficulty knobs")]
        [Min(1)]
        [SerializeField] private int baseItemCount = 6;

        [Min(0)]
        [SerializeField] private float timeLimitSeconds = 0f;

        [SerializeField] private bool helpEnabled = true;

        public GameModeType Mode => mode;
        public int BaseItemCount => baseItemCount;
        public float TimeLimitSeconds => timeLimitSeconds;
        public bool HelpEnabled => helpEnabled;
    }
}
