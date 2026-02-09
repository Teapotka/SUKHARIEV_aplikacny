using System;
using UnityEngine;
using BA.Core;
using BA.Data;
using BA.Telemetry;

namespace BA.Core.Progress
{
    [Serializable]
    public class ProgressData
    {
        public int unlockedCount = 5;
        public int matchTotalCards = 4;
        public int matchCorrectCount = 2;
    }

    public class ProgressService : MonoBehaviour
    {
        public static ProgressService Instance { get; private set; }

        private const string Key = "BA_PROGRESS_V1";

        [Header("Defaults")]
        [SerializeField] private int startUnlocked = 5;
        [SerializeField] private int maxUnlocked = 10;

        [Header("Match defaults")]
        [SerializeField] private int defaultMatchTotalCards = 4;
        [SerializeField] private int defaultMatchCorrectCount = 2;

        private ProgressData _data = new ProgressData();

        public int UnlockedCount => _data.unlockedCount;

        public int MatchTotalCards => Mathf.Clamp(_data.matchTotalCards, 2, 12);

        public int MatchCorrectCount
        {
            get
            {
                int total = MatchTotalCards;
                return Mathf.Clamp(_data.matchCorrectCount, 1, total - 1);
            }
        }

        public int MatchWrongCount => Mathf.Max(0, MatchTotalCards - MatchCorrectCount);

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadOrInit();
            ClampToCollection();
        }

        private void LoadOrInit()
        {
            if (!PlayerPrefs.HasKey(Key))
            {
                _data.unlockedCount = startUnlocked;
                _data.matchTotalCards = defaultMatchTotalCards;
                _data.matchCorrectCount = defaultMatchCorrectCount;
                Save();
                return;
            }

            var json = PlayerPrefs.GetString(Key);
            try
            {
                _data = JsonUtility.FromJson<ProgressData>(json) ?? new ProgressData();
            }
            catch
            {
                _data = new ProgressData();
                _data.unlockedCount = startUnlocked;
            }

            if (_data.unlockedCount <= 0)
                _data.unlockedCount = startUnlocked;

            if (_data.matchTotalCards < 2) _data.matchTotalCards = defaultMatchTotalCards;
            if (_data.matchCorrectCount < 1) _data.matchCorrectCount = defaultMatchCorrectCount;

            if (_data.matchCorrectCount >= _data.matchTotalCards)
                _data.matchCorrectCount = Mathf.Max(1, _data.matchTotalCards - 1);
        }

        private void ClampToCollection()
        {
            var collectionCount = GameContext.Instance?.GameData?.ActiveCollection?.Count ?? maxUnlocked;
            var hardMax = Mathf.Min(maxUnlocked, collectionCount);

            _data.unlockedCount = Mathf.Clamp(_data.unlockedCount, 1, hardMax);
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }

        public bool TryUnlockOne(string sourceMode)
        {
            var collectionCount = GameContext.Instance?.GameData?.ActiveCollection?.Count ?? maxUnlocked;
            var hardMax = Mathf.Min(maxUnlocked, collectionCount);

            if (_data.unlockedCount >= hardMax)
                return false;

            var before = _data.unlockedCount;
            _data.unlockedCount++;
            Save();

            TelemetryService.Instance?.Log(
                TelemetryEventType.TASK_END,
                sourceMode,
                new UnlockPayload { from = before, to = _data.unlockedCount, max = hardMax }
            );
            TelemetryService.Instance?.Flush();

            return true;
        }

        public void ResetProgress()
        {
            _data.unlockedCount = startUnlocked;
            ClampToCollection();
        }
    }

    [Serializable]
    public class UnlockPayload
    {
        public int from;
        public int to;
        public int max;
    }
}
