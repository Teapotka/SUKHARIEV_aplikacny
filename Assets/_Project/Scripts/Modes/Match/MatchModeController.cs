using BA.Core.Progress;
using BA.Data;
using BA.Telemetry;
using BA.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BA.Modes.Match
{
    public class MatchModeController : BA.Modes.ModeControllerBase
    {

        [Header("Content")]
        [SerializeField] private ArtCollectionSO collection;
        [SerializeField] private MatchQuestionSO[] questions;

        [Header("Scene refs")]
        [SerializeField] private MatchBoardView boardView;
        [SerializeField] private SocketSpawner socketSpawner;
        [SerializeField] private CardSpawner cardSpawner;
        [SerializeField] private MatchAnswerTracker answerTracker;

        [SerializeField] private MatchActionButtonView actionButton;
        [SerializeField] private MatchInputController inputController;

        [SerializeField] private string mainMenuSceneName = "01_MainMenu";
        [SerializeField] private ModeResultPopupView resultPopup;

        private MatchRound currentRound;
        private ModeState _state;

        private bool _modeStartLogged;
        private bool _modeEndLogged;
        private float _modeStartedAtRealtime;

        private string _roundId;
        private float _roundStartedAtRealtime;
        private bool _roundActive;
        private bool _roundEnded;

        private void Reset() => modeName = "Match";

        private void OnEnable()
        {
            LogModeStartIfNeeded();
        }

        private void OnDisable()
        {
            if (!_modeEndLogged && _roundActive && !_roundEnded)
            {
                LogRageQuitIfNeeded("rage_quit");
            }

            LogModeEndIfNeeded("scene_unload");
        }

        protected override void EnterState(ModeState state)
        {
            _state = state;

            switch (state)
            {
                case ModeState.Intro:
                    TransitionTo(ModeState.Play);
                    break;

                case ModeState.Play:
                    StartRound();
                    ApplyUIForPlay();
                    break;

                case ModeState.Feedback:
                    EvaluateRound();
                    break;

                case ModeState.Complete:
                    ApplyUIForFeedback();
                    break;
            }
        }

        private void StartRound()
        {
            if (!ValidateRefs()) return;

            ProgressService.Instance?.RegisterCollection(collection);

            var pool = BuildKnownPool(collection);

            int totalCards = 4;
            int correctCount = 3;

            if (ProgressService.Instance != null)
            {
                var cfg = ProgressService.Instance.GetMatchConfig();
                totalCards = cfg.total;
                correctCount = cfg.correct;
            }

            totalCards = Mathf.Clamp(totalCards, 2, 12);
            correctCount = Mathf.Clamp(correctCount, 1, totalCards - 1);

            var q = PickAdaptiveQuestion(pool, totalCards, correctCount);
            if (q == null)
            {
                Debug.LogWarning("[MatchModeController] No feasible question found, falling back to any random.");
                q = PickRandomQuestion();
            }

            if (q == null)
            {
                Debug.LogError("[MatchModeController] PickRandomQuestion returned NULL");
                return;
            }

            currentRound = MatchRoundBuilder.Build(pool, q, correctCount, totalCards);

            if (currentRound == null)
            {
                Debug.LogWarning("[MatchModeController] Round build failed, falling back to full collection pool.");
                currentRound = MatchRoundBuilder.Build(collection.Items, q, correctCount, totalCards);
            }

            if (currentRound == null)
            {
                Debug.LogError("[MatchModeController] Round build failed even with full pool.");
                return;
            }

            _roundId = System.Guid.NewGuid().ToString("N");
            _roundStartedAtRealtime = Time.realtimeSinceStartup;
            _roundActive = false;
            _roundEnded = false;

            boardView.SetPrompt(currentRound.Question.PromptText);

            socketSpawner.BuildSockets(currentRound.Correct.Count);
            cardSpawner.SpawnCards(currentRound.TableItems);

            answerTracker.Bind(socketSpawner);
            answerTracker.ClearPlacements();

            LogTaskStart(poolCount: pool.Count, totalCards: totalCards, correctCount: correctCount);

            _roundActive = true;
        }

        public void OnCheckPressed()
        {
            if (_state == ModeState.Play)
            {
                TransitionTo(ModeState.Feedback);
                return;
            }

            if (_state == ModeState.Complete || _state == ModeState.Feedback)
            {
                TransitionTo(ModeState.Play);
                return;
            }
        }

        private void EvaluateRound()
        {
            if (currentRound == null)
            {
                TransitionTo(ModeState.Play);
                return;
            }

            var placed = answerTracker.GetPlacedItems();
            int incorrect = 0;

            for (int i = 0; i < placed.Count; i++)
                if (!currentRound.Question.IsMatch(placed[i])) incorrect++;

            bool allSlotsFilled = placed.Count == currentRound.Correct.Count;
            bool success = allSlotsFilled && incorrect == 0;

            boardView.SetResult(success, incorrect);

            ResolveResultPopupIfNeeded();

            string title = success ? "Correct" : $"Wrong, {incorrect} incorrect";
            string body = success ? "Correct" : $"Wrong, {incorrect} incorrect";

            resultPopup?.Show(
                title,
                body,
                onContinue: () =>
                {
                    resultPopup?.Hide();
                    TransitionTo(ModeState.Play);
                },
                onHome: () =>
                {
                    resultPopup?.Hide();
                    SceneManager.LoadScene(mainMenuSceneName);
                }
            );

            float duration = Time.realtimeSinceStartup - _roundStartedAtRealtime;

            LogMatchResult(success, incorrect, allSlotsFilled, duration);
            LogTaskEnd(success, incorrect, allSlotsFilled, duration);

            _roundEnded = true;

            ProgressService.Instance?.RecordMatchRound(
                success: success,
                incorrect: incorrect,
                durationSeconds: duration,
                questionId: currentRound.Question != null ? currentRound.Question.QuestionId : "",
                sourceMode: "Match"
            );

            Debug.LogWarning($"[MatchModeController] Round result: success={success}, incorrect={incorrect}, duration={duration:0.00}s");

            FindFirstObjectByType<BA.Modes.Match.MatchStreakLabel>()?.Refresh();

            TransitionTo(ModeState.Complete);
        }

        // ---------- DDA question picking ----------
        private MatchQuestionSO PickAdaptiveQuestion(List<ArtItemSO> pool, int totalCards, int correctCount)
        {
            if (questions == null || questions.Length == 0) return null;

            int effectiveLvl = ProgressService.Instance != null
                ? ProgressService.Instance.EffectiveMatchDifficultyLevel
                : 1;

            int matchDifficultyLevel = ProgressService.Instance != null
                ? ProgressService.Instance.MatchDifficultyLevel : 0;

            int ddaOffset = ProgressService.Instance != null
                ? ProgressService.Instance.MatchDdaOffset
                : 0;

            var feasible = new List<MatchQuestionSO>();
            var preferEasy = new List<MatchQuestionSO>();
            var preferHard = new List<MatchQuestionSO>();

            for (int i = 0; i < questions.Length; i++)
            {
                var q = questions[i];
                if (q == null) continue;
                if (!q.IsAllowedForDifficulty(matchDifficultyLevel)) continue;

                if (!IsQuestionFeasible(q, pool, totalCards, correctCount))
                    continue;

                feasible.Add(q);

                bool isHard = q.Negate || q.MatchMode == TextMatchMode.Contains;
                bool isEasy = !q.Negate && q.MatchMode == TextMatchMode.Equals;

                if (isEasy) preferEasy.Add(q);
                if (isHard) preferHard.Add(q);
            }

            if (feasible.Count == 0) return null;

            if (ddaOffset < 0 && preferEasy.Count > 0)
                return preferEasy[Random.Range(0, preferEasy.Count)];

            if (ddaOffset > 0 && preferHard.Count > 0)
                return preferHard[Random.Range(0, preferHard.Count)];

            return feasible[Random.Range(0, feasible.Count)];
        }

        private bool IsQuestionFeasible(MatchQuestionSO q, List<ArtItemSO> pool, int totalCards, int correctCount)
        {
            if (q == null || pool == null || pool.Count == 0) return false;

            int needWrong = totalCards - correctCount;

            int matches = 0;
            int nonMatches = 0;

            for (int i = 0; i < pool.Count; i++)
            {
                var it = pool[i];
                if (it == null) continue;

                if (q.IsMatch(it)) matches++;
                else nonMatches++;
            }

            return matches >= correctCount && nonMatches >= needWrong;
        }

        // ---------- Pool ----------
        private List<ArtItemSO> BuildKnownPool(ArtCollectionSO col)
        {
            if (ProgressService.Instance == null || col == null)
                return col != null ? new List<ArtItemSO>(col.Items) : new List<ArtItemSO>();

            var unlockedIds = ProgressService.Instance.GetUnlockedExploreItemIds();
            if (unlockedIds == null || unlockedIds.Count == 0)
                return new List<ArtItemSO>(col.Items);

            var pool = new List<ArtItemSO>(unlockedIds.Count);
            for (int i = 0; i < unlockedIds.Count; i++)
            {
                var it = col.GetById(unlockedIds[i]);
                if (it != null) pool.Add(it);
            }

            if (pool.Count < 4)
                return new List<ArtItemSO>(col.Items);

            return pool;
        }

        private MatchQuestionSO PickRandomQuestion()
        {
            if (questions == null || questions.Length == 0) return null;
            return questions[Random.Range(0, questions.Length)];
        }

        private bool ValidateRefs()
        {
            if (collection == null)
            {
                Debug.LogError("[MatchModeController] Missing ArtCollectionSO");
                return false;
            }
            if (questions == null || questions.Length == 0)
            {
                Debug.LogError("[MatchModeController] Missing questions");
                return false;
            }
            if (boardView == null || socketSpawner == null || cardSpawner == null || answerTracker == null)
            {
                Debug.LogError("[MatchModeController] Missing scene references");
                return false;
            }
            return true;
        }

        private void ResolveResultPopupIfNeeded()
        {
            if (resultPopup != null && resultPopup.isActiveAndEnabled) return;

            var all = Object.FindObjectsByType<ModeResultPopupView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].isActiveAndEnabled) { resultPopup = all[i]; return; }

            if (all.Length > 0) resultPopup = all[0];
        }

        private void ApplyUIForPlay()
        {
            actionButton?.SetText("Check");
            inputController?.SetFrozen(false);
        }

        private void ApplyUIForFeedback()
        {
            actionButton?.SetText("Next");
            inputController?.SetFrozen(true);
        }

        // ---------- Telemetry helpers ----------
        private void LogModeStartIfNeeded()
        {
            if (_modeStartLogged) return;
            _modeStartLogged = true;
            _modeStartedAtRealtime = Time.realtimeSinceStartup;

            ProgressService.Instance?.EnsureExploreInitializedForActiveCollection();

            int totalCards = 0;
            int correctCount = 0;

            if (ProgressService.Instance != null)
            {
                var cfg = ProgressService.Instance.GetMatchConfig();
                totalCards = cfg.total;
                correctCount = cfg.correct;

            }

            TelemetryService.Instance?.Log(
                TelemetryEventType.MODE_START,
                modeName,
                new ModeStartPayload
                {
                    itemCount = totalCards,
                    timeLimitSeconds = 0f,
                }
            );
            TelemetryService.Instance?.Flush();
        }

        private void LogModeEndIfNeeded(string reason)
        {
            if (_modeEndLogged) return;
            _modeEndLogged = true;

            float duration = Time.realtimeSinceStartup - _modeStartedAtRealtime;

            TelemetryService.Instance?.Log(
                TelemetryEventType.MODE_END,
                modeName,
                new MatchModeEndPayload
                {
                    reason = reason,
                    durationSeconds = duration,
                    unlockedCount = ProgressService.Instance != null ? ProgressService.Instance.UnlockedCount : 0,
                    viewedCount = ProgressService.Instance != null ? ProgressService.Instance.ViewedCount : 0,
                    effectiveDifficulty = ProgressService.Instance != null ? ProgressService.Instance.EffectiveMatchDifficultyLevel : 1,
                    ddaOffset = ProgressService.Instance != null ? ProgressService.Instance.MatchDdaOffset : 0
                }
            );
            TelemetryService.Instance?.Flush();
        }

        private void LogRageQuitIfNeeded(string reason)
        {
            if (string.IsNullOrWhiteSpace(_roundId)) return;

            float roundDuration = Time.realtimeSinceStartup - _roundStartedAtRealtime;

            TelemetryService.Instance?.Log(
                TelemetryEventType.RAGE_QUIT,
                modeName,
                new MatchRageQuitPayload
                {
                    reason = reason,
                    roundId = _roundId,
                    questionId = currentRound?.Question != null ? currentRound.Question.QuestionId : "",
                    durationSeconds = roundDuration
                }
            );
            TelemetryService.Instance?.Flush();
        }

        private void LogTaskStart(int poolCount, int totalCards, int correctCount)
        {
            var ps = ProgressService.Instance;

            TelemetryService.Instance?.Log(
                TelemetryEventType.TASK_START,
                modeName,
                new MatchTaskStartPayload
                {
                    roundId = _roundId,
                    questionId = currentRound?.Question != null ? currentRound.Question.QuestionId : "",
                    poolCount = poolCount,
                    totalCards = totalCards,
                    correctCards = correctCount,
                    unlockedCount = ps != null ? ps.UnlockedCount : 0,
                    viewedCount = ps != null ? ps.ViewedCount : 0,
                    baseDifficulty = ps != null ? ps.MatchDifficultyLevel : 1,
                    ddaOffset = ps != null ? ps.MatchDdaOffset : 0,
                    effectiveDifficulty = ps != null ? ps.EffectiveMatchDifficultyLevel : 1,
                }
            );
            TelemetryService.Instance?.Flush();
        }

        private void LogTaskEnd(bool success, int incorrect, bool allSlotsFilled, float durationSeconds)
        {
            var ps = ProgressService.Instance;

            TelemetryService.Instance?.Log(
                TelemetryEventType.TASK_END,
                modeName,
                new MatchTaskEndPayload
                {
                    roundId = _roundId,
                    questionId = currentRound?.Question != null ? currentRound.Question.QuestionId : "",
                    win = success,
                    incorrect = incorrect,
                    allSlotsFilled = allSlotsFilled,
                    durationSeconds = durationSeconds,
                    effectiveDifficulty = ps != null ? ps.EffectiveMatchDifficultyLevel : 1
                }
            );
            TelemetryService.Instance?.Flush();
        }

        private void LogMatchResult(bool success, int incorrect, bool allSlotsFilled, float durationSeconds)
        {
            TelemetryService.Instance?.Log(
                TelemetryEventType.MATCH_RESULT,
                modeName,
                new MatchResultPayload
                {
                    roundId = _roundId,
                    questionId = currentRound?.Question != null ? currentRound.Question.QuestionId : "",
                    win = success,
                    incorrect = incorrect,
                    allSlotsFilled = allSlotsFilled,
                    durationSeconds = durationSeconds,
                }
            );
        }
    }
}
