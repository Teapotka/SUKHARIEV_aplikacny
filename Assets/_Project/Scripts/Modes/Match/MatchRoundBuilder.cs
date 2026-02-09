using System;
using System.Collections.Generic;
using BA.Data;
using UnityEngine;

    public static class MatchRoundBuilder
    {

        public static MatchRound Build(
            ArtCollectionSO collection,
            MatchQuestionSO question,
            int correctCount,
            int distractorCount,
            int seed = -1)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (question == null) throw new ArgumentNullException(nameof(question));

            correctCount = Mathf.Max(0, correctCount);
            distractorCount = Mathf.Max(0, distractorCount);

            var all = collection.Items;
            var correctPool = new List<ArtItemSO>();
            var wrongPool = new List<ArtItemSO>();

            for (int i = 0; i < all.Count; i++)
            {
                var it = all[i];
                if (it == null) continue;

                if (question.IsMatch(it)) correctPool.Add(it);
                else wrongPool.Add(it);
            }

            if (correctPool.Count == 0)
                Debug.LogWarning($"[MatchRoundBuilder] No correct items for question '{question.QuestionId}'");

            var rng = seed == -1 ? new System.Random() : new System.Random(seed);

            var round = new MatchRound
            {
                Question = question,
                Correct = PickRandomUnique(correctPool, correctCount, rng),
                Distractors = PickRandomUnique(wrongPool, distractorCount, rng)
            };

            round.TableItems = new List<ArtItemSO>(round.Correct.Count + round.Distractors.Count);
            round.TableItems.AddRange(round.Correct);
            round.TableItems.AddRange(round.Distractors);

            Shuffle(round.TableItems, rng);

            return round;
        }

        private static List<ArtItemSO> PickRandomUnique(List<ArtItemSO> pool, int count, System.Random rng)
        {
            var result = new List<ArtItemSO>();
            if (pool == null || pool.Count == 0 || count <= 0) return result;

            count = Mathf.Min(count, pool.Count);

            var indices = new int[pool.Count];
            for (int i = 0; i < indices.Length; i++) indices[i] = i;

            for (int i = indices.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            for (int k = 0; k < count; k++)
                result.Add(pool[indices[k]]);

            return result;
        }

        private static void Shuffle(List<ArtItemSO> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
