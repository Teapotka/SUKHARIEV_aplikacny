using BA.Data;
using UnityEngine;

namespace BA.Modes.Arcade
{
    public class ArcadeModeController : BA.Modes.ModeControllerBase
    {
        protected override GameModeType ModeType => GameModeType.Arcade;

        [SerializeField] private PuzzleBoard board;

        [Header("Content")]
        [SerializeField] private ArtCollectionSO collection;

        private readonly System.Collections.Generic.HashSet<string> usedIds = new();

        [SerializeField] private ArcadeSideLabels sideLabels;

        private ArtItemSO currentFront;
        private ArtItemSO currentBack;



        private void Reset()
        {
            modeName = "Arcade";
        }

        private void StartGame()
        {
            if (collection == null || board == null) return;

            var pair = PickTwoDifferent();
            currentFront = pair.front;
            currentBack = pair.back;

            if (currentFront == null || currentBack == null)
            {
                Debug.LogError("[Arcade] Not enough ArtItemSO to pick two different items.");
                return;
            }

            var frontTex = currentFront.Image != null ? currentFront.Image.texture : null;
            var backTex = currentBack.Image != null ? currentBack.Image.texture : null;

            board.SetTextures(frontTex, backTex);
            board.RebuildNewPuzzle();

            sideLabels.Configure(
                frontText: BuildLabel(currentFront),
                backText: BuildLabel(currentBack)
            );
            sideLabels.SetBackView(false);
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


        public void MoveUp() => board.MoveSelected(Vector2Int.down);
        public void MoveDown() => board.MoveSelected(Vector2Int.up);
        public void MoveLeft() => board.MoveSelected(Vector2Int.left);
        public void MoveRight() => board.MoveSelected(Vector2Int.right);

        public void FlipTile() => board.FlipSelected();


        private ArtItemSO PickRandomUnused()
        {
            var items = collection != null ? collection.Items : null;
            if (items == null || items.Count == 0) return null;

            var candidates = new System.Collections.Generic.List<ArtItemSO>();
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it == null) continue;
                if (string.IsNullOrWhiteSpace(it.Id)) continue;

                if (!usedIds.Contains(it.Id))
                    candidates.Add(it);
            }

            if (candidates.Count == 0)
            {
                usedIds.Clear();
                for (int i = 0; i < items.Count; i++)
                    if (items[i] != null && !string.IsNullOrWhiteSpace(items[i].Id))
                        candidates.Add(items[i]);
                if (candidates.Count == 0) return null;
            }

            var chosen = candidates[Random.Range(0, candidates.Count)];
            usedIds.Add(chosen.Id);
            return chosen;
        }

        private (ArtItemSO front, ArtItemSO back) PickTwoDifferent()
        {
            var a = PickRandomUnused();
            if (a == null) return (null, null);

            ArtItemSO b = null;

            for (int i = 0; i < 20; i++)
            {
                b = PickRandomUnused();
                if (b != null && b.Id != a.Id) break;
            }

            if (b == null || b.Id == a.Id)
            {
                var items = collection.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    var it = items[i];
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

        private string BuildLabel(ArtItemSO it)
        {
            if (it == null) return "";
            var tags = it.Tags != null ? string.Join(", ", it.Tags) : "";
            return $"{it.Title}\n{it.Author}\n{it.Style}\n{tags}";
        }

    }
}
