using BA.Core.Progress;
using BA.Data;
using UnityEngine;

namespace BA.Modes.Match
{
    public class MatchModeController : BA.Modes.ModeControllerBase
    {
        protected override GameModeType ModeType => GameModeType.Match;

        [Header("Content")]
        [SerializeField] private ArtCollectionSO collection;
        [SerializeField] private MatchQuestionSO[] questions;

        [Header("Scene refs")]
        [SerializeField] private MatchBoardView boardView;      // writes text on board
        [SerializeField] private SocketSpawner socketSpawner;    // builds sockets
        [SerializeField] private CardSpawner cardSpawner;        // spawns miniatures
        [SerializeField] private MatchAnswerTracker answerTracker; // knows what is placed in sockets

        [SerializeField] private MatchActionButtonView actionButton;
        [SerializeField] private MatchInputController inputController;

        private MatchRound currentRound;
        private ModeState _state;


        private void Reset()
        {
            modeName = "Match";
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
                    //TransitionTo(ModeState.Play);
                    ApplyUIForFeedback();

                    break;
            }
        }
        private void StartRound()
        {
            if (!ValidateRefs()) return;

            var q = PickRandomQuestion();
            if (q == null)
            {
                Debug.LogError("[MatchModeController] PickRandomQuestion returned NULL");
                return;
            }

            // Single source of truth: ProgressService
            int correct = ProgressService.Instance != null ? ProgressService.Instance.MatchCorrectCount : 2;
            int wrong = ProgressService.Instance != null ? ProgressService.Instance.MatchWrongCount : 2;

            currentRound = MatchRoundBuilder.Build(collection, q, correct, wrong);

            Debug.Log($"Q: {currentRound.Question.PromptText} | correct={currentRound.Correct.Count} | table={currentRound.TableItems.Count}");
            for (int i = 0; i < currentRound.Correct.Count; i++)
                Debug.Log($"  + {currentRound.Correct[i].Title}");

            boardView.SetPrompt(currentRound.Question.PromptText);

            socketSpawner.BuildSockets(currentRound.Correct.Count);
            cardSpawner.SpawnCards(currentRound.TableItems);

            answerTracker.Bind(socketSpawner); 
            answerTracker.ClearPlacements();

            // telemetry: ROUND_START (optional)
        }
        public void OnCheckPressed()
        {
            
                if (_state == ModeState.Play) // if your base class exposes it
                {
                    TransitionTo(ModeState.Feedback);
                    return;
                }

                if (_state == ModeState.Feedback || _state == ModeState.Complete)
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

            var placed = answerTracker.GetPlacedItems(); // List<ArtItemSO>
            int incorrect = 0;

            for (int i = 0; i < placed.Count; i++)
                if (!currentRound.Question.IsMatch(placed[i])) incorrect++;

            int requiredCorrect = ProgressService.Instance != null ? ProgressService.Instance.MatchCorrectCount : currentRound.Correct.Count;

            bool allSlotsFilled = placed.Count == currentRound.Correct.Count;
            bool success = allSlotsFilled && incorrect == 0;

            boardView.SetResult(success, incorrect);

            // telemetry: ROUND_END (success, incorrect, time, etc.)

            TransitionTo(ModeState.Complete);
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
                Debug.LogError("[MatchModeController] Missing scene references (boardView/socketSpawner/cardSpawner/answerTracker)");
                return false;
            }
            return true;
        }

        private void ApplyUIForPlay()
        {
            actionButton?.SetText("Check");
            //if (inputController) inputController.enabled = true; // allow drag
            inputController?.SetFrozen(false);
        }

        private void ApplyUIForFeedback()
        {
            actionButton?.SetText("Next");
            //if (inputController) inputController.enabled = false; // freeze drag

            inputController?.SetFrozen(true);
        }
    }
}
