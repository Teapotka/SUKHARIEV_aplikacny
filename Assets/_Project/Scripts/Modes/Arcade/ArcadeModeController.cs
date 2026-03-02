using BA.Core.Progress;
using BA.Data;
using BA.Telemetry;
using BA.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BA.Modes.Arcade
{
    public class ArcadeModeController : BA.Modes.ModeControllerBase
    {

        [SerializeField] private PuzzleBoard board;

        [Header("Content")]
        [SerializeField] private ArtCollectionSO collection;
        [SerializeField] private ArcadeSideLabels sideLabels;

        [Header("Timer UI (optional)")]
        [SerializeField] private ArcadeTimerView timerView;

        [Header("Moves UI (optional)")]
        [SerializeField] private ArcadeMovesView movesView;

        [Header("Score UI (Gamified, optional)")]
        [SerializeField] private ArcadeScoreView scoreView;

        [Header("Timer")]
        [SerializeField] private float timeLimitSeconds = 120f;

        [Header("Time Bar")]
        [SerializeField] private ArcadeTimeBarView timeBarView;

        [SerializeField] private string mainMenuSceneName = "01_MainMenu";
        [SerializeField] private ModeResultPopupView resultPopup;

        private float _timeLeft;
        private float _roundStartedAtRealtime;
        private bool _finished;

        private readonly HashSet<string> usedIds = new();

        private ArtItemSO currentFront;
        private ArtItemSO currentBack;

        // ----- Telemetry: mode -----
        private bool _modeStartLogged;
        private bool _modeEndLogged;
        private float _modeStartedAtRealtime;

        // ----- Telemetry: round -----
        private string _roundId;
        private bool _roundActive;
        private bool _roundEnded;

        // ----- Telemetry: actions -----
        private string _lastAction = "";
        private int _actionsSinceFlush = 0;
        private const int FlushEveryNActions = 25;

        // ------ Telemetry: rage quit cache ------
        private int _initialBackBaseline;
        private int _lastMovesTotal;
        private int _lastSlideMoves;
        private int _lastFlipCount;

        private void Reset()
        {
            modeName = "Arcade";
        }

        private void OnEnable()
        {
            if (board != null)
            {
                board.Solved += HandleSolved;
                board.MovesChanged += HandleMovesChanged;
            }
        }

        private void OnDisable()
        {
            if (board != null)
            {
                board.Solved -= HandleSolved;
                board.MovesChanged -= HandleMovesChanged;
            }

            if (!_modeEndLogged && _roundActive && !_roundEnded)
            {
                LogRageQuitIfNeeded("scene_unload");
            }

            LogModeEndIfNeeded("scene_unload");
        }

        private void Update()
        {
            if (_finished) return;
            if (board == null) return;

            ResolveTimerViewIfNeeded();
            ResolveScoreViewIfNeeded();

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                timerView?.SetSeconds(_timeLeft);

                UpdateLiveScore(); 
                Finish(win: false, reason: "timeout");
                return;
            }

            timerView?.SetSeconds(_timeLeft);

            timeBarView?.SetSeconds(_timeLeft, timeLimitSeconds);

            UpdateLiveScore();
        }

        protected override void EnterState(ModeState state)
        {
            switch (state)
            {
                case ModeState.Intro:
                    TransitionTo(ModeState.Play);
                    break;

                case ModeState.Play:
                    StartGame();
                    break;
            }
        }

        private void StartGame()
        {
            if (collection == null || board == null)
            {
                Debug.LogError("[Arcade] Missing collection or board reference.");
                return;
            }

            var preset = ProgressService.Instance != null ? ProgressService.Instance.GetArcadePreset() : null;
            if (preset != null)
            {
                timeLimitSeconds = preset.timeLimit;

                board.SetShuffleMoves(preset.shuffleMoves);
            }

            LogModeStartIfNeeded();

            _finished = false;
            _roundActive = false;
            _roundEnded = false;

            _finished = false;
            _timeLeft = Mathf.Max(1f, timeLimitSeconds);
            _roundStartedAtRealtime = Time.realtimeSinceStartup;

            _initialBackBaseline = 0;
            _lastMovesTotal = 0;
            _lastSlideMoves = 0;
            _lastFlipCount = 0;

            timeBarView?.SetSeconds(_timeLeft, timeLimitSeconds);

            ResolveTimerViewIfNeeded();
            ResolveMovesViewIfNeeded();
            ResolveScoreViewIfNeeded();

            timerView?.SetSeconds(_timeLeft);
            movesView?.SetMoves(0);
            scoreView?.ResetView();

            var pool = BuildKnownPool(collection);

            if (pool.Count < 2)
            {
                Debug.LogWarning("[Arcade] Known pool < 2, falling back to full collection.");
                pool = new List<ArtItemSO>(collection.Items);
            }

            if (pool.Count < 2)
            {
                Debug.LogError("[Arcade] Not enough ArtItemSO to pick two different items.");
                return;
            }

            var pair = PickTwoDifferent(pool);
            currentFront = pair.front;
            currentBack = pair.back;

            if (currentFront == null || currentBack == null)
            {
                Debug.LogError("[Arcade] Failed to pick two items.");
                return;
            }

            _roundId = System.Guid.NewGuid().ToString("N");

            var frontTex = currentFront.Image != null ? currentFront.Image.texture : null;
            var backTex = currentBack.Image != null ? currentBack.Image.texture : null;

            board.SetTextures(frontTex, backTex);
            board.RebuildNewPuzzle();
            board.SetInputBlocked(false);

            _initialBackBaseline = board.InitialBackTileCount;

            if (sideLabels != null)
            {
                sideLabels.Configure(
                    frontText: BuildLabel(currentFront),
                    backText: BuildLabel(currentBack)
                );
                sideLabels.SetBackView(false);
            }

            UpdateLiveScore();

            TelemetryService.Instance?.Log(
                TelemetryEventType.ROUND_START,
                modeName,
                new ArcadeRoundStartPayload
                {
                    roundId = _roundId,
                    timeLimitSeconds = timeLimitSeconds,
                    col = board.col,
                    row = board.row,
                    frontId = currentFront.Id,
                    backId = currentBack.Id,
                    presetIndex = ProgressService.Instance != null ? ProgressService.Instance.ArcadePresetIndex : 0,
                    unlockedCount = ProgressService.Instance != null ? ProgressService.Instance.UnlockedCount : 0,
                    viewedCount = ProgressService.Instance != null ? ProgressService.Instance.ViewedCount : 0
                }
            );
            TelemetryService.Instance?.Flush();

            _roundActive = true;
            _roundEnded = false;
        }

        private void HandleSolved()
        {
            if (_finished) return;

            UpdateLiveScore();
            Finish(win: true, reason: "solved");
        }

        private void HandleMovesChanged(int moves)
        {
            ResolveMovesViewIfNeeded();
            movesView?.SetMoves(moves);

            ResolveScoreViewIfNeeded();
            UpdateLiveScore();

            _lastMovesTotal = moves;
            if (board != null)
            {
                _lastSlideMoves = board.SlideMoveCount;
                _lastFlipCount = board.FlipCount;
            }

            // ARCADE_ACTION: log move count changes
            if (_roundActive && !_roundEnded && !string.IsNullOrWhiteSpace(_roundId))
            {
                TelemetryService.Instance?.Log(
                    TelemetryEventType.ARCADE_ACTION,
                    modeName,
                    new ArcadeActionPayload
                    {
                        roundId = _roundId,
                        action = string.IsNullOrWhiteSpace(_lastAction) ? "moves_changed" : _lastAction,
                        moves = moves,
                        timeLeftSeconds = _timeLeft
                    }
                );

                _actionsSinceFlush++;
                if (_actionsSinceFlush >= FlushEveryNActions)
                {
                    _actionsSinceFlush = 0;
                    TelemetryService.Instance?.Flush();
                }
            }

            _lastAction = ""; 
        }

        private void Finish(bool win, string reason)
        {
            _finished = true;

            if (board != null)
                board.SetInputBlocked(true);

            float duration = Time.realtimeSinceStartup - _roundStartedAtRealtime;
            int moves = board != null ? board.MoveCount : 0;

            int finalScore = ComputeScore(_timeLeft, board != null ? board.MoveCount : 0);
            string medal = win ? GetMedal(finalScore) : "No medal";

            ResolveResultPopupIfNeeded();

            string title = win ? $"You won\nScore: {finalScore} Medal: {medal}" : $"You lost\nReason: {reason}";
            string body = win ? $"You won\nScore: {finalScore} Medal: {medal}" : $"You lost\nReason: {reason}";

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

            ResolveScoreViewIfNeeded();
            scoreView?.SetFinalScore(finalScore, medal);

            Debug.Log(win ? "[Arcade] WIN" : "[Arcade] LOSE");

            int movesTotal = board != null ? board.MoveCount : _lastMovesTotal;
            int slideMoves = board != null ? board.SlideMoveCount : _lastSlideMoves;
            int flipCount = board != null ? board.FlipCount : _lastFlipCount;
            int initialBack = board != null ? board.InitialBackTileCount : _initialBackBaseline;

            int errorCount = Mathf.Max(0, flipCount - initialBack);

            TelemetryService.Instance?.Log(
                TelemetryEventType.ROUND_END,
                modeName,
                new ArcadeRoundEndPayload
                {
                    roundId = _roundId,
                    win = win,
                    reason = reason,
                    timeLimitSeconds = timeLimitSeconds,
                    timeLeftSeconds = _timeLeft,
                    durationSeconds = duration,
                    score = finalScore,
                    medal = medal,
                    moves = movesTotal,
                    slideMoveCount = slideMoves,
                    flipCount = flipCount,
                    initialBackTileCount = initialBack,
                    errorCount = errorCount,
                    col = board != null ? board.col : 0,
                    row = board != null ? board.row : 0,
                    frontId = currentFront != null ? currentFront.Id : "",
                    backId = currentBack != null ? currentBack.Id : "",
                    presetIndex = ProgressService.Instance != null ? ProgressService.Instance.ArcadePresetIndex : 0
                }
            );

            _roundEnded = true;

            if (ProgressService.Instance != null && board != null)
            {
                ProgressService.Instance.RecordArcadeRound(
                    win: win,
                    reason: reason,
                    timeLimit: timeLimitSeconds,
                    timeLeft: _timeLeft,
                    duration: duration,
                    moves: board.MoveCount,
                    col: board.col,
                    row: board.row,
                    score: finalScore, 
                    medal: medal
                );
            }

            TelemetryService.Instance?.Flush();
        }

        // ---------- Scoring ----------
        private int ComputeScore(float timeLeft, int moves)
        {
            float raw = (4f * Mathf.Max(0f, timeLeft)) - (0.9f * Mathf.Max(0, moves));
            return Mathf.Max(0, Mathf.FloorToInt(raw));
        }

        private void UpdateLiveScore()
        {
            if (scoreView == null) return;
            int moves = board != null ? board.MoveCount : 0;
            int live = ComputeScore(_timeLeft, moves);
            scoreView.SetLiveScore(live);
        }

        private string GetMedal(int finalScore)
        {
            int max = Mathf.RoundToInt(Mathf.Max(1f, timeLimitSeconds) * 4f);
            float r = max > 0 ? (float)finalScore / max : 0f; 

            if (r >= 0.75f) return "Gold";   
            if (r >= 0.55f) return "Silver";  
            if (r >= 0.35f) return "Bronze"; 
            return "No medal";
        }

        // ---------- UI Buttons ----------
        public void MoveUp() {
            _lastAction = "move_up";
            board.MoveSelected(Vector2Int.down); 
        }
        public void MoveDown() { 
            _lastAction = "move_down";
            board.MoveSelected(Vector2Int.up); 
        }
        public void MoveLeft() { 
            _lastAction = "move_left";
            board.MoveSelected(Vector2Int.left); 
        }
        public void MoveRight() {
            _lastAction = "move_right";
            board.MoveSelected(Vector2Int.right);
        }
        public void FlipTile() {
            _lastAction = "flip";
            board.FlipSelected(); 
        }

        // ---------- View resolving ----------
        private void ResolveTimerViewIfNeeded()
        {
            if (timerView != null && timerView.isActiveAndEnabled)
                return;

            var all = Object.FindObjectsByType<ArcadeTimerView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].isActiveAndEnabled)
                {
                    timerView = all[i];
                    return;
                }
            }

            if (all.Length > 0)
                timerView = all[0];
        }

        private void ResolveMovesViewIfNeeded()
        {
            if (movesView != null && movesView.isActiveAndEnabled)
                return;

            var all = Object.FindObjectsByType<ArcadeMovesView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].isActiveAndEnabled)
                {
                    movesView = all[i];
                    return;
                }
            }

            if (all.Length > 0)
                movesView = all[0];
        }

        private void ResolveScoreViewIfNeeded()
        {
            if (scoreView != null && scoreView.isActiveAndEnabled)
                return;

            var all = Object.FindObjectsByType<ArcadeScoreView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].isActiveAndEnabled)
                {
                    scoreView = all[i];
                    return;
                }
            }

            if (all.Length > 0)
                scoreView = all[0];
        }

        // ---------- Pool from progress ----------
        private List<ArtItemSO> BuildKnownPool(ArtCollectionSO col)
        {
            if (col == null) return new List<ArtItemSO>();

            if (ProgressService.Instance == null)
                return new List<ArtItemSO>(col.Items);

            var unlockedIds = ProgressService.Instance.GetUnlockedExploreItemIds();
            if (unlockedIds == null || unlockedIds.Count == 0)
                return new List<ArtItemSO>(col.Items);

            var pool = new List<ArtItemSO>(unlockedIds.Count);
            for (int i = 0; i < unlockedIds.Count; i++)
            {
                var it = col.GetById(unlockedIds[i]);
                if (it != null) pool.Add(it);
            }
            return pool;
        }

        // ---------- Picking  ----------
        private ArtItemSO PickRandomUnused(List<ArtItemSO> pool)
        {
            if (pool == null || pool.Count == 0) return null;

            var candidates = new List<ArtItemSO>(pool.Count);
            for (int i = 0; i < pool.Count; i++)
            {
                var it = pool[i];
                if (it == null) continue;
                if (string.IsNullOrWhiteSpace(it.Id)) continue;
                if (!usedIds.Contains(it.Id))
                    candidates.Add(it);
            }

            if (candidates.Count == 0)
            {
                usedIds.Clear();
                for (int i = 0; i < pool.Count; i++)
                {
                    var it = pool[i];
                    if (it != null && !string.IsNullOrWhiteSpace(it.Id))
                        candidates.Add(it);
                }
                if (candidates.Count == 0) return null;
            }

            var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            usedIds.Add(chosen.Id);
            return chosen;
        }

        private (ArtItemSO front, ArtItemSO back) PickTwoDifferent(List<ArtItemSO> pool)
        {
            var a = PickRandomUnused(pool);
            if (a == null) return (null, null);

            ArtItemSO b = null;

            for (int i = 0; i < 30; i++)
            {
                b = PickRandomUnused(pool);
                if (b != null && b.Id != a.Id) break;
            }

            if (b == null || b.Id == a.Id)
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    var it = pool[i];
                    if (it != null && it.Id != a.Id)
                    {
                        b = it;
                        usedIds.Add(it.Id);
                        break;
                    }
                }
            }

            return (a, b);
        }

        private void ResolveResultPopupIfNeeded()
        {
            if (resultPopup != null && resultPopup.isActiveAndEnabled) return;

            var all = Object.FindObjectsByType<ModeResultPopupView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].isActiveAndEnabled) { resultPopup = all[i]; return; }

            if (all.Length > 0) resultPopup = all[0];
        }

        private string BuildLabel(ArtItemSO it)
        {
            if (it == null) return "";
            var tags = it.Tags != null ? string.Join(", ", it.Tags) : "";
            return $"{it.Title}\n{it.Author}\n{it.Style}\n{tags}";
        }

        // ----------- Telemetry helpers -----------
        private void LogRageQuitIfNeeded(string reason)
        {
            if (string.IsNullOrWhiteSpace(_roundId)) return;

            float duration = Time.realtimeSinceStartup - _roundStartedAtRealtime;
            int moves = board.MoveCount;

            int movesTotal = _lastMovesTotal;
            int slideMoves = _lastSlideMoves;
            int flipCount = _lastFlipCount;
            int initialBack = _initialBackBaseline;

            int errorCount = Mathf.Max(0, flipCount - initialBack);

            TelemetryService.Instance?.Log(
                TelemetryEventType.RAGE_QUIT,
                modeName,
                new ArcadeRageQuitPayload
                {
                    reason = reason,
                    roundId = _roundId,
                    durationSeconds = duration,
                    moves = movesTotal,
                    slideMoveCount = slideMoves,
                    flipCount = flipCount,
                    initialBackTileCount = initialBack,
                    errorCount = errorCount,
                    timeLeftSeconds = _timeLeft,
                    timeLimitSeconds = timeLimitSeconds,
                    frontId = currentFront != null ? currentFront.Id : "",
                    backId = currentBack != null ? currentBack.Id : ""
                }
            );
            TelemetryService.Instance?.Flush();
        }

        private void LogModeStartIfNeeded()
        {
            if (_modeStartLogged) return;
            _modeStartLogged = true;
            _modeStartedAtRealtime = Time.realtimeSinceStartup;

            int itemCount = (board != null) ? (board.col * board.row) : 0;

            TelemetryService.Instance?.Log(
                TelemetryEventType.MODE_START,
                modeName,
                new ModeStartPayload
                {
                    itemCount = itemCount,
                    timeLimitSeconds = timeLimitSeconds,
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
                new ArcadeModeEndPayload
                {
                    reason = reason,
                    durationSeconds = duration,
                    presetIndex = ProgressService.Instance != null ? ProgressService.Instance.ArcadePresetIndex : 0,
                    unlockedCount = ProgressService.Instance != null ? ProgressService.Instance.UnlockedCount : 0,
                    viewedCount = ProgressService.Instance != null ? ProgressService.Instance.ViewedCount : 0
                }
            );
            TelemetryService.Instance?.Flush();
        }
    }
}