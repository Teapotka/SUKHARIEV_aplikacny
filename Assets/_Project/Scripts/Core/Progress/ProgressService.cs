using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BA.Core;
using BA.Data;
using BA.Telemetry;

namespace BA.Core.Progress
{
    [Serializable]
    public class ProgressData
    {
        // Explore
        public List<string> unlockedItemIds = new();
        public List<string> viewedItemIds = new();

        // Match difficulty + streak (progression + unlock rule)
        public int matchDifficultyLevel = 1; // 1..3 (progression)
        public int matchStreak = 0;          // consecutive successes (unlock rule)

        // Match DDA (performance)
        public int matchDdaOffset = 0;       // -1..+1
        public int matchGamesSinceChange = 0;
        public int matchFailStreak = 0;      // consecutive fails (for hints)
        public List<MatchRoundStat> matchHistory = new();

        // Arcade adaptation
        public int arcadePresetIndex = 0;           // 0..N-1
        public int arcadeGamesSinceChange = 0;      // cooldown
        public List<ArcadeRoundStat> arcadeHistory = new();

        // ---------- Global gamification ----------
        public int xp = 0;

        // ---------- Arcade gamification (saved) ----------
        public int arcadeLastScore = 0;
        public string arcadeLastMedal = "None";

        public int arcadeBestScore = 0;
        public string arcadeBestMedal = "None";

        public int version = 6;
    }

    [Serializable]
    public class MatchRoundStat
    {
        public string timestampUtc;
        public bool win;
        public int incorrect;
        public float duration;
        public int effectiveDifficulty;
        public string questionId;
        public bool hintUsed;
    }

    [Serializable]
    public class ArcadePreset
    {
        public string name;
        public int shuffleMoves = 200;
        public float flipChance = 0.5f;
        public float timeLimit = 120f;
    }

    [Serializable]
    public class ArcadeRoundStat
    {
        public string timestampUtc;
        public bool win;
        public string reason;

        public int moves;

        public float timeLimit;
        public float timeLeft;
        public float duration;

        public int score;
        public string medal;
    }

    public class ProgressService : MonoBehaviour
    {
        public static ProgressService Instance { get; private set; }

        [Header("File Save")]
        [SerializeField] private string fileName = "progress.json";

        [Header("Explore defaults")]
        [SerializeField] private int startUnlocked = 3;
        [SerializeField] private int maxUnlocked = 50;

        [Header("Match difficulty thresholds (by unlocked count)")]
        [SerializeField] private int unlocksForLevel2 = 6;
        [SerializeField] private int unlocksForLevel3 = 10;

        [Header("Match DDA")]
        [SerializeField] private int matchWindow = 5;
        [SerializeField] private int matchCooldownGames = 2;

        [Header("History size caps (prevents huge progress.json)")]
        [SerializeField] private int maxMatchHistoryRecords = 20;
        [SerializeField] private int maxArcadeHistoryRecords = 20;

        [Header("Arcade presets (Difficulty)")]
        [SerializeField] private List<ArcadePreset> arcadePresets = new();
        [SerializeField] private int arcadeWindow = 5;
        [SerializeField] private int arcadeCooldownGames = 2;

        [Header("XP / Level (Global)")]
        [SerializeField] private int xpPerLevel = 200;

        [Header("XP rewards")]
        [SerializeField] private int xpOnViewArt = 5;
        [SerializeField] private int xpOnMatchWin = 25;
        [SerializeField] private int xpOnMatchLose = 10;
        [SerializeField] private int xpOnArcadeWin = 40;
        [SerializeField] private int xpOnArcadeLose = 15;

        private ProgressData _data = new ProgressData();

        private HashSet<string> _unlocked = new();
        private HashSet<string> _viewed = new();

        private ArtCollectionSO _fallbackCollection;

        public string SavePath => Path.Combine(Application.persistentDataPath, fileName);

        public int UnlockedCount => _unlocked.Count;
        public int ViewedCount => _viewed.Count;

        public int MatchDifficultyLevel => Mathf.Clamp(_data.matchDifficultyLevel, 1, 3);
        public int MatchStreak => Mathf.Max(0, _data.matchStreak);

        public int MatchDdaOffset => Mathf.Clamp(_data.matchDdaOffset, -1, 1);
        public int EffectiveMatchDifficultyLevel => Mathf.Clamp(MatchDifficultyLevel + MatchDdaOffset, 1, 3);

        // ---------- Global XP/Level ----------
        public int XP => Mathf.Max(0, _data.xp);

        public int Level
        {
            get
            {
                int per = Mathf.Max(1, xpPerLevel);
                return 1 + (XP / per);
            }
        }

        public int XpInCurrentLevel
        {
            get
            {
                int per = Mathf.Max(1, xpPerLevel);
                return XP % per;
            }
        }

        public int XpToNextLevel
        {
            get
            {
                int per = Mathf.Max(1, xpPerLevel);
                return per - XpInCurrentLevel;
            }
        }

        public float XpProgress01
        {
            get
            {
                int per = Mathf.Max(1, xpPerLevel);
                return Mathf.Clamp01((float)XpInCurrentLevel / per);
            }
        }

        // ---------- Arcade stored score/medal ----------
        public int ArcadeLastScore => Mathf.Max(0, _data.arcadeLastScore);
        public string ArcadeLastMedal => string.IsNullOrWhiteSpace(_data.arcadeLastMedal) ? "None" : _data.arcadeLastMedal;

        public int ArcadeBestScore => Mathf.Max(0, _data.arcadeBestScore);
        public string ArcadeBestMedal => string.IsNullOrWhiteSpace(_data.arcadeBestMedal) ? "None" : _data.arcadeBestMedal;

        public event Action MetaChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadOrInit();
            RebuildCachesFromData();
        }

        public void RegisterCollection(ArtCollectionSO collection)
        {
            if (collection != null) _fallbackCollection = collection;
        }

        // ---------- Match Config ----------
        public (int total, int correct, int wrongMin) GetMatchConfig()
        {
            int lvl = EffectiveMatchDifficultyLevel;

            if (lvl == 1) return (4, 3, 1);
            if (lvl == 2) return (6, 4, 2);
            return (8, 4, 4);
        }

        public int GetMatchHintBudget()
        {
            if (MatchDdaOffset <= -1) return 2;
            if (MatchDdaOffset == 0) return (_data.matchFailStreak >= 2) ? 1 : 0;
            return 0;
        }

        public void RecordMatchRound(
            bool success,
            int incorrect,
            float durationSeconds,
            string questionId,
            bool hintUsed,
            string sourceMode = "Match")
        {
            EnsureExploreInitializedForActiveCollection();

            if (!success)
            {
                _data.matchStreak = 0;
                _data.matchFailStreak++;

                AddXPInternal(xpOnMatchLose);
            }
            else
            {
                _data.matchStreak++;
                _data.matchFailStreak = 0;
                TryUnlockIfEligible(sourceMode);

                AddXPInternal(xpOnMatchWin);
            }

            _data.matchHistory ??= new List<MatchRoundStat>();
            _data.matchHistory.Add(new MatchRoundStat
            {
                timestampUtc = DateTime.UtcNow.ToString("o"),
                win = success,
                incorrect = Mathf.Max(0, incorrect),
                duration = Mathf.Max(0f, durationSeconds),
                effectiveDifficulty = EffectiveMatchDifficultyLevel,
                questionId = questionId ?? "",
                hintUsed = hintUsed
            });

            TrimTail(_data.matchHistory, Mathf.Max(1, maxMatchHistoryRecords));

            UpdateMatchDdaOffsetIfNeeded();
            SaveToFile();
        }

        private void UpdateMatchDdaOffsetIfNeeded()
        {
            _data.matchHistory ??= new List<MatchRoundStat>();

            if (_data.matchGamesSinceChange < matchCooldownGames)
            {
                _data.matchGamesSinceChange++;
                return;
            }

            int n = Mathf.Min(matchWindow, _data.matchHistory.Count);
            if (n < 3) return;

            var window = _data.matchHistory.GetRange(_data.matchHistory.Count - n, n);

            int wins = 0;
            float avgIncorrect = 0f;

            for (int i = 0; i < window.Count; i++)
            {
                if (window[i].win) wins++;
                avgIncorrect += window[i].incorrect;
            }

            avgIncorrect /= n;

            bool shouldHarder = (wins >= n - 1) && (avgIncorrect <= 0.1f);
            bool shouldEasier = (wins <= 1) || (avgIncorrect >= 1.0f);

            int old = MatchDdaOffset;
            int next = old;

            if (shouldEasier) next = old - 1;
            else if (shouldHarder) next = old + 1;

            next = Mathf.Clamp(next, -1, 1);

            if (next != old)
            {
                _data.matchDdaOffset = next;
                _data.matchGamesSinceChange = 0;
                Debug.Log($"[ProgressService] MatchDDA offset changed: {old} -> {next} (wins={wins}/{n}, avgIncorrect={avgIncorrect:0.00})");
            }
        }

        private void RecalculateMatchDifficultyIfNeeded(string sourceMode)
        {
            int newLevel = 1;

            if (_unlocked.Count >= unlocksForLevel3) newLevel = 3;
            else if (_unlocked.Count >= unlocksForLevel2) newLevel = 2;

            newLevel = Mathf.Clamp(newLevel, 1, 3);

            if (_data.matchDifficultyLevel != newLevel)
            {
                int old = _data.matchDifficultyLevel;
                _data.matchDifficultyLevel = newLevel;
                SaveToFile();

                Debug.Log($"[ProgressService] MatchDifficulty changed: {old} -> {newLevel} (unlocked={_unlocked.Count})");

                TelemetryService.Instance?.Log(
                    TelemetryEventType.TASK_END,
                    sourceMode,
                    new DifficultyPayload { from = old, to = newLevel, unlockedCount = _unlocked.Count }
                );
                TelemetryService.Instance?.Flush();
            }
        }

        // ---------- Explore ----------
        public void EnsureExploreInitializedForActiveCollection()
        {
            var ids = GetCollectionIds();
            if (ids.Count == 0) return;

            _unlocked.IntersectWith(ids);
            _viewed.IntersectWith(ids);

            if (_unlocked.Count == 0)
            {
                int n = Mathf.Clamp(startUnlocked, 1, Mathf.Min(ids.Count, maxUnlocked));
                foreach (var id in ids.OrderBy(x => x, StringComparer.Ordinal).Take(n))
                    _unlocked.Add(id);

                SyncBackToDataLists();
                SaveToFile();
            }
            else
            {
                SyncBackToDataLists();
                SaveToFile();
            }

            RecalculateMatchDifficultyIfNeeded("ProgressInit");
        }

        public List<string> GetUnlockedExploreItemIds()
        {
            EnsureExploreInitializedForActiveCollection();
            return _unlocked.OrderBy(x => x, StringComparer.Ordinal).ToList();
        }

        public void MarkViewed(string itemId, string sourceMode = "Explore")
        {
            EnsureExploreInitializedForActiveCollection();

            if (string.IsNullOrWhiteSpace(itemId)) return;
            if (!_unlocked.Contains(itemId)) return;

            if (_viewed.Add(itemId))
            {
                AddXPInternal(xpOnViewArt);

                SyncBackToDataLists();
                SaveToFile();

                TelemetryService.Instance?.Log(
                    TelemetryEventType.ITEM_INTERACT,
                    sourceMode,
                    new ViewedPayload { itemId = itemId, viewedCount = _viewed.Count, unlockedCount = _unlocked.Count }
                );
            }
        }

        // ---------- Unlock Rule ----------
        private void TryUnlockIfEligible(string sourceMode)
        {
            var ids = GetCollectionIds();
            if (ids.Count == 0)
            {
                Debug.LogWarning("[ProgressService] Unlock skipped: collection ids not available (GameContext + fallback are null). Call RegisterCollection().");
                return;
            }

            bool allUnlockedViewed = (_unlocked.Count > 0) && (_viewed.Count == _unlocked.Count);
            bool streakEnough = _data.matchStreak >= _unlocked.Count;

            if (!allUnlockedViewed || !streakEnough) return;

            int hardMax = Mathf.Min(ids.Count, maxUnlocked);
            if (_unlocked.Count >= hardMax) return;

            var next = ids.Where(id => !_unlocked.Contains(id))
                          .OrderBy(id => id, StringComparer.Ordinal)
                          .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(next)) return;

            int before = _unlocked.Count;

            _unlocked.Add(next);

            _data.matchStreak = 0;

            RecalculateMatchDifficultyIfNeeded(sourceMode);

            SyncBackToDataLists();
            SaveToFile();

            Debug.Log($"[ProgressService] UNLOCKED new item: {next} (before={before} after={_unlocked.Count}).");

            TelemetryService.Instance?.Log(
                TelemetryEventType.TASK_END,
                sourceMode,
                new UnlockPayload
                {
                    from = before,
                    to = _unlocked.Count,
                    max = hardMax,
                    unlockedItemId = next,
                    streakRequired = before
                }
            );
            TelemetryService.Instance?.Flush();
        }

        // ---------- Collection IDs ----------
        private List<string> GetCollectionIds()
        {
            var col = GameContext.Instance?.GameData?.ActiveCollection;
            if (col == null) col = _fallbackCollection;

            if (col == null || col.Count == 0) return new List<string>();

            var ids = new List<string>();
            var items = col.Items;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                if (string.IsNullOrWhiteSpace(it.Id)) continue;
                if (!ids.Contains(it.Id)) ids.Add(it.Id);
            }

            return ids;
        }

        // ---------- File I/O ----------
        private void LoadOrInit()
        {
            if (!File.Exists(SavePath))
            {
                _data = new ProgressData { version = 6 };
                SaveToFile();
                return;
            }

            try
            {
                var json = File.ReadAllText(SavePath);
                _data = JsonUtility.FromJson<ProgressData>(json) ?? new ProgressData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProgressService] Failed to load {SavePath}. Resetting. {e.Message}");
                _data = new ProgressData { version = 6 };
                SaveToFile();
            }

            _data.unlockedItemIds ??= new List<string>();
            _data.viewedItemIds ??= new List<string>();
            _data.matchHistory ??= new List<MatchRoundStat>();
            _data.arcadeHistory ??= new List<ArcadeRoundStat>();

            if (_data.matchDifficultyLevel < 1) _data.matchDifficultyLevel = 1;
            if (_data.matchStreak < 0) _data.matchStreak = 0;

            _data.matchDdaOffset = Mathf.Clamp(_data.matchDdaOffset, -1, 1);
            if (_data.matchGamesSinceChange < 0) _data.matchGamesSinceChange = 0;
            if (_data.matchFailStreak < 0) _data.matchFailStreak = 0;

            _data.xp = Mathf.Max(0, _data.xp);

            if (string.IsNullOrWhiteSpace(_data.arcadeLastMedal)) _data.arcadeLastMedal = "None";
            if (string.IsNullOrWhiteSpace(_data.arcadeBestMedal)) _data.arcadeBestMedal = "None";
            _data.arcadeLastScore = Mathf.Max(0, _data.arcadeLastScore);
            _data.arcadeBestScore = Mathf.Max(0, _data.arcadeBestScore);

            TrimTail(_data.matchHistory, Mathf.Max(1, maxMatchHistoryRecords));
            TrimTail(_data.arcadeHistory, Mathf.Max(1, maxArcadeHistoryRecords));
        }

        private void SaveToFile()
        {
            try
            {
                var dir = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                SyncBackToDataLists();

                TrimTail(_data.matchHistory, Mathf.Max(1, maxMatchHistoryRecords));
                TrimTail(_data.arcadeHistory, Mathf.Max(1, maxArcadeHistoryRecords));

                var json = JsonUtility.ToJson(_data, prettyPrint: true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProgressService] Failed to save progress: {e.Message}");
            }
        }

        private void RebuildCachesFromData()
        {
            _unlocked = new HashSet<string>(_data.unlockedItemIds ?? new List<string>());
            _viewed = new HashSet<string>(_data.viewedItemIds ?? new List<string>());
        }

        private void SyncBackToDataLists()
        {
            _data.unlockedItemIds = _unlocked.ToList();
            _data.viewedItemIds = _viewed.ToList();
        }

        // ---------- Debug ----------
        [ContextMenu("Dump Progress")]
        public void DumpProgress()
        {
            Debug.Log(
                $"[ProgressService] SavePath={SavePath}\n" +
                $"Unlocked={UnlockedCount} Viewed={ViewedCount}\n" +
                $"XP={XP} Level={Level} XPInLevel={XpInCurrentLevel}/{Mathf.Max(1, xpPerLevel)}\n" +
                $"Arcade: last={ArcadeLastScore}({ArcadeLastMedal}) best={ArcadeBestScore}({ArcadeBestMedal})\n" +
                $"Match: base={MatchDifficultyLevel} ddaOffset={MatchDdaOffset} effective={EffectiveMatchDifficultyLevel} streak={MatchStreak} failStreak={_data.matchFailStreak}\n" +
                $"MatchHistory={_data.matchHistory?.Count ?? 0} ArcadeHistory={_data.arcadeHistory?.Count ?? 0}"
            );
        }

        // ---------- Arcade DDA ---------
        public ArcadePreset GetArcadePreset()
        {
            EnsureArcadePresets();
            int idx = Mathf.Clamp(_data.arcadePresetIndex, 0, arcadePresets.Count - 1);
            return arcadePresets[idx];
        }

        public int ArcadePresetIndex
        {
            get
            {
                EnsureArcadePresets();
                return Mathf.Clamp(_data.arcadePresetIndex, 0, arcadePresets.Count - 1);
            }
        }

        public (int score, string medal) ComputeArcadeScoreAndMedal(bool win, float timeLimit, float timeLeft, int moves)
        {
            if (!win) return (0, "None");

            float limit = Mathf.Max(0.01f, timeLimit);
            float timeRatio = Mathf.Clamp01(timeLeft / limit);

            float moveRatio = Mathf.Clamp01(1f - (moves / 300f));

            // Score 0..1000
            int score = Mathf.RoundToInt((timeRatio * 700f) + (moveRatio * 300f));
            score = Mathf.Clamp(score, 0, 1000);

            string medal;
            if (score >= 800) medal = "Gold";
            else if (score >= 600) medal = "Silver";
            else medal = "Bronze";

            return (score, medal);
        }

        public void RecordArcadeRound(bool win, string reason, float timeLimit, float timeLeft, float duration, int moves, int col, int row, int score, string medal)
        {
            _data.arcadeHistory ??= new List<ArcadeRoundStat>();


            _data.arcadeLastScore = score;
            _data.arcadeLastMedal = medal;

            if (score > _data.arcadeBestScore)
            {
                _data.arcadeBestScore = score;
                _data.arcadeBestMedal = medal;
            }

            _data.arcadeHistory.Add(new ArcadeRoundStat
            {
                timestampUtc = DateTime.UtcNow.ToString("o"),
                win = win,
                reason = reason ?? "",
                timeLimit = timeLimit,
                timeLeft = timeLeft,
                duration = duration,
                moves = moves,

                score = Mathf.Max(0, score),
                medal = medal ?? ""
            });

            AddXPInternal(win ? xpOnArcadeWin : xpOnArcadeLose);

            TrimTail(_data.arcadeHistory, Mathf.Max(1, maxArcadeHistoryRecords));

            UpdateArcadePresetIndex();
            SaveToFile();

            MetaChanged?.Invoke();
        }

        private void EnsureArcadePresets()
        {
            if (arcadePresets != null && arcadePresets.Count > 0) return;

            arcadePresets = new List<ArcadePreset>
            {
                new ArcadePreset { name="Easy",   shuffleMoves=5,  flipChance=0.25f, timeLimit=300f },
                new ArcadePreset { name="Medium", shuffleMoves=10, flipChance=0.5f,  timeLimit=500f },
                new ArcadePreset { name="Hard",   shuffleMoves=20, flipChance=0.75f, timeLimit=700f },
            };
        }

        private void UpdateArcadePresetIndex()
        {
            EnsureArcadePresets();
            _data.arcadeHistory ??= new List<ArcadeRoundStat>();

            if (_data.arcadeGamesSinceChange < arcadeCooldownGames)
            {
                _data.arcadeGamesSinceChange++;
                return;
            }

            int n = Mathf.Min(arcadeWindow, _data.arcadeHistory.Count);
            if (n < 3) return;

            var window = _data.arcadeHistory.GetRange(_data.arcadeHistory.Count - n, n);

            int wins = 0;
            int timeouts = 0;
            float avgWinTimeRatio = 0f;
            float avgWinMoves = 0f;
            int winCount = 0;

            for (int i = 0; i < window.Count; i++)
            {
                var s = window[i];
                if (s.reason == "timeout") timeouts++;

                if (s.win)
                {
                    wins++;
                    float ratio = s.timeLimit > 0.01f ? (s.duration / s.timeLimit) : 1f;
                    avgWinTimeRatio += ratio;
                    avgWinMoves += s.moves;
                    winCount++;
                }
            }

            if (winCount > 0)
            {
                avgWinTimeRatio /= winCount;
                avgWinMoves /= winCount;
            }

            int idx = ArcadePresetIndex;

            bool shouldEasier = (timeouts >= 2) || (wins <= 1);
            bool shouldHarder = (wins >= n - 1) && (avgWinTimeRatio <= 0.70f) && (avgWinMoves <= 220f);

            int newIdx = idx;
            if (shouldEasier) newIdx = idx - 1;
            else if (shouldHarder) newIdx = idx + 1;

            newIdx = Mathf.Clamp(newIdx, 0, arcadePresets.Count - 1);

            if (newIdx != idx)
            {
                _data.arcadePresetIndex = newIdx;
                _data.arcadeGamesSinceChange = 0;
            }
        }

        // ---------- XP helpers ----------
        private void AddXPInternal(int amount)
        {
            if (amount <= 0) return;
            _data.xp = Mathf.Max(0, _data.xp + amount);
            MetaChanged?.Invoke();
        }

        // ---------- helpers ----------
        private static void TrimTail<T>(List<T> list, int max)
        {
            if (list == null) return;
            if (max < 1) max = 1;
            int extra = list.Count - max;
            if (extra > 0)
                list.RemoveRange(0, extra);
        }
    }

    [Serializable]
    public class UnlockPayload
    {
        public int from;
        public int to;
        public int max;
        public string unlockedItemId;
        public int streakRequired;
    }

    [Serializable]
    public class ViewedPayload
    {
        public string itemId;
        public int viewedCount;
        public int unlockedCount;
    }

    [Serializable]
    public class DifficultyPayload
    {
        public int from;
        public int to;
        public int unlockedCount;
    }
}