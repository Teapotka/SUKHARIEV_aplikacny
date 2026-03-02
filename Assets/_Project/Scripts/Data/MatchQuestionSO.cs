using System;
using UnityEngine;

namespace BA.Data
{
    public enum MatchRuleType
    {
        Style,
        Author,
        Tag
    }

    public enum TextMatchMode
    {
        Equals,
        Contains
    }

    public enum MatchQuestionComplexity
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    [CreateAssetMenu(menuName = "BA/Data/Match Question", fileName = "SO_MatchQuestion_")]
    public class MatchQuestionSO : ScriptableObject
    {
        [SerializeField] private string questionId;

        [Header("Difficulty gating (progression)")]
        [Range(1, 3)]
        [SerializeField] private int minDifficulty = 1;

        [Range(1, 3)]
        [SerializeField] private int maxDifficulty = 3;

        [TextArea(2, 6)]
        [SerializeField] private string promptText;

        [Header("Rule")]
        [SerializeField] private MatchRuleType ruleType;
        [SerializeField] private string ruleValue;

        [Header("Smart matching")]
        [SerializeField] private TextMatchMode matchMode = TextMatchMode.Equals;

        [Header("Negation")]
        [Tooltip("If enabled, the rule is negated (NOT). Example: NOT by da Vinci.")]
        [SerializeField] private bool negate = false;

        public string QuestionId => questionId;
        public string PromptText => promptText;

        public TextMatchMode MatchMode => matchMode;
        public bool Negate => negate;

        public bool IsAllowedForDifficulty(int difficulty)
        {
            difficulty = Mathf.Clamp(difficulty, 1, 3);
            return difficulty >= minDifficulty && difficulty <= maxDifficulty;
        }

        public bool IsMatch(ArtItemSO item)
        {
            if (item == null) return false;

            bool baseMatch = ruleType switch
            {
                MatchRuleType.Style => MatchText(item.Style, ruleValue, matchMode),
                MatchRuleType.Author => MatchText(item.Author, ruleValue, matchMode),
                MatchRuleType.Tag => MatchAnyTag(item, ruleValue, matchMode),
                _ => false
            };

            return negate ? !baseMatch : baseMatch;
        }

        public bool IsFeasibleForPool(System.Collections.Generic.IReadOnlyList<ArtItemSO> pool, int totalCards, int correctCount)
        {
            if (pool == null || pool.Count == 0) return false;

            totalCards = Mathf.Clamp(totalCards, 2, 10);
            correctCount = Mathf.Clamp(correctCount, 1, totalCards - 1);
            int needWrong = totalCards - correctCount;

            int matches = 0;
            int nonMatches = 0;

            for (int i = 0; i < pool.Count; i++)
            {
                var it = pool[i];
                if (it == null) continue;

                if (IsMatch(it)) matches++;
                else nonMatches++;
            }

            return matches >= correctCount && nonMatches >= needWrong;
        }

        private static bool MatchAnyTag(ArtItemSO item, string value, TextMatchMode mode)
        {
            var tags = item.Tags;
            if (tags == null) return false;

            for (int i = 0; i < tags.Count; i++)
            {
                if (MatchText(tags[i], value, mode))
                    return true;
            }
            return false;
        }

        private static bool MatchText(string source, string value, TextMatchMode mode)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(value)) return false;

            source = source.Trim();
            value = value.Trim();

            if (mode == TextMatchMode.Equals)
                return string.Equals(source, value, StringComparison.OrdinalIgnoreCase);

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
