using System.Collections.Generic;
using UnityEngine;
using BA.Data;

namespace BA.Modes.Match
{
    public static class MatchRoundBuilder
    {
        public static MatchRound Build(ArtCollectionSO collection, MatchQuestionSO q, int correctCount, int wrongCount)
        {
            if (collection == null) return null;
            int total = Mathf.Max(2, correctCount + wrongCount);
            return Build(collection.Items, q, correctCount, total);
        }

        public static MatchRound Build(IReadOnlyList<ArtItemSO> pool, MatchQuestionSO q, int correctCount, int totalCards)
        {
            if (pool == null || pool.Count == 0 || q == null) return null;

            totalCards = Mathf.Clamp(totalCards, 2, 12);
            correctCount = Mathf.Clamp(correctCount, 1, totalCards - 1);

            var matches = new List<ArtItemSO>();
            var nonMatches = new List<ArtItemSO>();

            for (int i = 0; i < pool.Count; i++)
            {
                var it = pool[i];
                if (it == null) continue;

                if (q.IsMatch(it)) matches.Add(it);
                else nonMatches.Add(it);
            }

            if (matches.Count == 0)
            {
                return null;
            }

            while (correctCount > matches.Count && correctCount > 1)
                correctCount--;

            int needWrong = totalCards - correctCount;
            while (needWrong > nonMatches.Count && totalCards > (correctCount + 1))
            {
                totalCards--;
                needWrong = totalCards - correctCount;
            }

            if (correctCount > matches.Count) return null;
            if (needWrong > nonMatches.Count) return null;

            var correct = PickRandomUnique(matches, correctCount);
            var wrong = PickRandomUnique(nonMatches, needWrong);

            var table = new List<ArtItemSO>(correct.Count + wrong.Count);
            table.AddRange(correct);
            table.AddRange(wrong);
            Shuffle(table);

            return new MatchRound
            {
                Question = q,
                Correct = correct,
                TableItems = table
            };
        }

        // ---------- Helpers ----------

        private static List<ArtItemSO> PickRandomUnique(List<ArtItemSO> src, int count)
        {
            var copy = new List<ArtItemSO>(src);
            Shuffle(copy);

            if (count >= copy.Count) return copy;

            var res = new List<ArtItemSO>(count);
            for (int i = 0; i < count; i++)
                res.Add(copy[i]);

            return res;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
