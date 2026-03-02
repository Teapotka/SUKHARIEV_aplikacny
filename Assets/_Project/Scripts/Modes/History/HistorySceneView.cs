using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using BA.Core;       
using BA.Data;
using BA.Core.Progress;

namespace BA.Modes.History
{
    public class HistorySceneView : MonoBehaviour
    {
        [Header("Optional: Collection for totals (if null, tries GameContext)")]
        [SerializeField] private ArtCollectionSO collection;

        [Header("File")]
        [SerializeField] private string fileName = "progress.json";

        [Header("History rendering")]
        [SerializeField, Range(3, 20)] private int showRecentMatch = 10;
        [SerializeField, Range(3, 20)] private int showRecentArcade = 10;

        // ---------------- GLOBAL (TMP) ----------------
        [Header("GLOBAL (TMP)")]
        [SerializeField] private TMP_Text savePathText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private TMP_Text xpToNextText;
        [SerializeField] private TMP_Text unlockedText;
        [SerializeField] private TMP_Text viewedText;

        // ---------------- MATCH (TMP) ----------------
        [Header("MATCH (TMP)")]
        [SerializeField] private TMP_Text matchBaseDifficultyText;
        [SerializeField] private TMP_Text matchDdaOffsetText;
        [SerializeField] private TMP_Text matchEffectiveDifficultyText;
        [SerializeField] private TMP_Text matchStreakText;

        [SerializeField] private TMP_Text matchWinRateText;
        [SerializeField] private TMP_Text matchAvgIncorrectText;
        [SerializeField] private TMP_Text matchAvgDurationText;

        [SerializeField] private TMP_Text matchRecentHistoryText;

        // ---------------- ARCADE (TMP) ----------------
        [Header("ARCADE (TMP)")]
        [SerializeField] private TMP_Text arcadePresetText;
        [SerializeField] private TMP_Text arcadeBestScoreText;
        [SerializeField] private TMP_Text arcadeBestMedalText;

        [SerializeField] private TMP_Text arcadeWinRateText;
        [SerializeField] private TMP_Text arcadeAvgMovesText;
        [SerializeField] private TMP_Text arcadeAvgDurationText;

        [SerializeField] private TMP_Text arcadeRecentHistoryText;


        private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

        private void OnEnable()
        {
            Refresh();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            Set(savePathText, SavePath);

            if (!File.Exists(SavePath))
            {
                WriteNoData();
                return;
            }

            ProgressData data;
            try
            {
                var json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<ProgressData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HistorySceneView] Failed to read/parse progress file: {e.Message}");
                WriteNoData();
                return;
            }

            if (data == null)
            {
                WriteNoData();
                return;
            }

            // ---------- GLOBAL ----------
            int unlockedCount = data.unlockedItemIds != null ? data.unlockedItemIds.Count : 0;
            int viewedCount = data.viewedItemIds != null ? data.viewedItemIds.Count : 0;

            int total = ResolveTotalCountFallback(unlockedCount);

            int xp = Mathf.Max(0, data.xp);
            int xpPerLevel = 200; 
            int level = 1 + (xp / Mathf.Max(1, xpPerLevel));
            int inLevel = xp % Mathf.Max(1, xpPerLevel);
            int toNext = Mathf.Max(0, xpPerLevel - inLevel);

            Set(levelText, $"Level: {level}");
            Set(xpText, $"XP: {xp}  ({inLevel}/{xpPerLevel})");
            Set(xpToNextText, $"To next: {toNext}");

            Set(unlockedText, $"Unlocked: {unlockedCount}/{total}");
            Set(viewedText, $"Viewed: {viewedCount}/{Mathf.Max(1, unlockedCount)}");

            // ---------- MATCH ----------
            int baseDiff = Mathf.Clamp(data.matchDifficultyLevel, 1, 3);
            int dda = Mathf.Clamp(data.matchDdaOffset, -1, 1);
            int eff = Mathf.Clamp(baseDiff + dda, 1, 3);

            Set(matchBaseDifficultyText, $"Base difficulty: {baseDiff}");
            Set(matchDdaOffsetText, $"DDA offset: {dda:+#;-#;0}");
            Set(matchEffectiveDifficultyText, $"Effective: {eff}");
            Set(matchStreakText, $"Streak: {Mathf.Max(0, data.matchStreak)}");

            var matchList = data.matchHistory ?? new List<MatchRoundStat>();
            WriteMatchAggregates(matchList);
            WriteMatchRecent(matchList);

            // ---------- ARCADE ----------
            Set(arcadePresetText, $"Preset: {data.arcadePresetIndex}");

            var arcadeList = data.arcadeHistory ?? new List<ArcadeRoundStat>();
            WriteArcadeAggregates(arcadeList, data);
            WriteArcadeRecent(arcadeList);
        }

        private int ResolveTotalCountFallback(int unlockedCount)
        {
            if (collection != null && collection.Count > 0)
                return collection.Count;

            var ctxCol = GameContext.Instance?.GameData?.ActiveCollection;
            if (ctxCol != null && ctxCol.Count > 0)
                return ctxCol.Count;

            return Mathf.Max(unlockedCount, 1);
        }

        // ---------------- MATCH helpers ----------------
        private void WriteMatchAggregates(List<MatchRoundStat> list)
        {
            if (list == null || list.Count == 0)
            {
                Set(matchWinRateText, "Win rate: —");
                Set(matchAvgIncorrectText, "Avg incorrect: —");
                Set(matchAvgDurationText, "Avg duration: —");
                return;
            }

            int wins = 0;
            float sumIncorrect = 0f;
            float sumDur = 0f;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].win) wins++;
                sumIncorrect += Mathf.Max(0, list[i].incorrect);
                sumDur += Mathf.Max(0f, list[i].duration);
            }

            float winRate = 100f * wins / Mathf.Max(1, list.Count);
            float avgIncorrect = sumIncorrect / Mathf.Max(1, list.Count);
            float avgDur = sumDur / Mathf.Max(1, list.Count);

            Set(matchWinRateText, $"Win rate (last {list.Count}): {winRate:0}%");
            Set(matchAvgIncorrectText, $"Avg incorrect: {avgIncorrect:0.00}");
            Set(matchAvgDurationText, $"Avg duration: {avgDur:0.0}s");
        }

        private void WriteMatchRecent(List<MatchRoundStat> list)
        {
            if (matchRecentHistoryText == null) return;

            if (list == null || list.Count == 0)
            {
                matchRecentHistoryText.text = "No Match history yet.";
                return;
            }

            int n = Mathf.Min(showRecentMatch, list.Count);
            var sb = new System.Text.StringBuilder(256);

            for (int i = list.Count - n; i < list.Count; i++)
            {
                var s = list[i];
                string icon = s.win ? "✓" : "✗";
                string time = ShortTimeLocal(s.timestampUtc);
                sb.Append(icon)
                  .Append("  D").Append(Mathf.Clamp(s.effectiveDifficulty, 1, 3))
                  .Append("  err ").Append(Mathf.Max(0, s.incorrect))
                  .Append("  ").Append(Mathf.Max(0f, s.duration).ToString("0.0", CultureInfo.InvariantCulture)).Append("s")
                  .Append("  Q").Append(string.IsNullOrEmpty(s.questionId) ? "?" : s.questionId)
                  .Append("  ").Append(time)
                  .AppendLine();
            }

            matchRecentHistoryText.text = sb.ToString();
        }

        // ---------------- ARCADE helpers ----------------
        private void WriteArcadeAggregates(List<ArcadeRoundStat> list, ProgressData data)
        {
            int bestScore = 0;
            string bestMedal = "";

            bool hasExplicitBest = data.arcadeBestScore > 0 || !string.IsNullOrEmpty(data.arcadeBestMedal);
            if (hasExplicitBest)
            {
                bestScore = Mathf.Max(0, data.arcadeBestScore);
                bestMedal = data.arcadeBestMedal ?? "";
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int sc = Mathf.Max(0, list[i].score);
                    if (sc > bestScore)
                    {
                        bestScore = sc;
                        bestMedal = list[i].medal ?? "";
                    }
                }
            }

            Set(arcadeBestScoreText, $"Best score: {bestScore}");
            Set(arcadeBestMedalText, $"Best medal: {(string.IsNullOrWhiteSpace(bestMedal) ? "—" : bestMedal)}");

            if (list == null || list.Count == 0)
            {
                Set(arcadeWinRateText, "Win rate: —");
                Set(arcadeAvgMovesText, "Avg moves: —");
                Set(arcadeAvgDurationText, "Avg duration: —");
                return;
            }

            int wins = 0;
            float sumMovesAll = 0f;
            float sumDurAll = 0f;

            float sumMovesWins = 0f;
            int winCount = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var s = list[i];
                if (s.win) wins++;

                sumMovesAll += Mathf.Max(0, s.moves);
                sumDurAll += Mathf.Max(0f, s.duration);

                if (s.win)
                {
                    sumMovesWins += Mathf.Max(0, s.moves);
                    winCount++;
                }
            }

            float winRate = 100f * wins / Mathf.Max(1, list.Count);
            float avgMoves = (winCount > 0) ? (sumMovesWins / winCount) : (sumMovesAll / Mathf.Max(1, list.Count));
            float avgDur = sumDurAll / Mathf.Max(1, list.Count);

            Set(arcadeWinRateText, $"Win rate (last {list.Count}): {winRate:0}%");
            Set(arcadeAvgMovesText, $"Avg moves: {avgMoves:0.0}" + (winCount > 0 ? " (wins)" : ""));
            Set(arcadeAvgDurationText, $"Avg duration: {avgDur:0.0}s");
        }

        private void WriteArcadeRecent(List<ArcadeRoundStat> list)
        {
            if (arcadeRecentHistoryText == null) return;

            if (list == null || list.Count == 0)
            {
                arcadeRecentHistoryText.text = "No Arcade history yet.";
                return;
            }

            int n = Mathf.Min(showRecentArcade, list.Count);
            var sb = new System.Text.StringBuilder(256);

            for (int i = list.Count - n; i < list.Count; i++)
            {
                var s = list[i];
                string icon = s.win ? "✓" : "✗";
                string time = ShortTimeLocal(s.timestampUtc);

                string medal = string.IsNullOrWhiteSpace(s.medal) ? "" : $" {s.medal}";
                int score = Mathf.Max(0, s.score);

                sb.Append(icon)
                  .Append("  ").Append(string.IsNullOrWhiteSpace(s.reason) ? "-" : s.reason)
                  .Append("  mv ").Append(Mathf.Max(0, s.moves))
                  .Append("  ").Append(Mathf.Max(0f, s.duration).ToString("0.0", CultureInfo.InvariantCulture)).Append("s")
                  .Append("  sc ").Append(score)
                  .Append(medal)
                  .Append("  ").Append(time)
                  .AppendLine();
            }

            arcadeRecentHistoryText.text = sb.ToString();
        }

        // ---------------- utilities ----------------
        private static void Set(TMP_Text t, string value)
        {
            if (t != null) t.text = value ?? "";
        }

        private void WriteNoData()
        {
            Set(levelText, "Level: —");
            Set(xpText, "XP: —");
            Set(xpToNextText, "To next: —");
            Set(unlockedText, "Unlocked: —");
            Set(viewedText, "Viewed: —");

            Set(matchBaseDifficultyText, "Base difficulty: —");
            Set(matchDdaOffsetText, "DDA offset: —");
            Set(matchEffectiveDifficultyText, "Effective: —");
            Set(matchStreakText, "Streak: —");
            Set(matchWinRateText, "Win rate: —");
            Set(matchAvgIncorrectText, "Avg incorrect: —");
            Set(matchAvgDurationText, "Avg duration: —");
            if (matchRecentHistoryText) matchRecentHistoryText.text = "No data.";

            Set(arcadePresetText, "Preset: —");
            Set(arcadeBestScoreText, "Best score: —");
            Set(arcadeBestMedalText, "Best medal: —");
            Set(arcadeWinRateText, "Win rate: —");
            Set(arcadeAvgMovesText, "Avg moves: —");
            Set(arcadeAvgDurationText, "Avg duration: —");
            if (arcadeRecentHistoryText) arcadeRecentHistoryText.text = "No data.";
        }

        private static string ShortTimeLocal(string utcIso)
        {
            if (string.IsNullOrWhiteSpace(utcIso)) return "";

            if (DateTime.TryParse(utcIso, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dtUtc))
            {
                var local = dtUtc.ToLocalTime();
                return local.ToString("HH:mm", CultureInfo.InvariantCulture);
            }
            return "";
        }
    }
}