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

    [CreateAssetMenu(menuName = "BA/Data/Match Question", fileName = "SO_MatchQuestion_")]
    public class MatchQuestionSO : ScriptableObject
    {
        [SerializeField] private string questionId;
        [TextArea(2, 6)]
        [SerializeField] private string promptText;

        [SerializeField] private MatchRuleType ruleType;
        [SerializeField] private string ruleValue;

        [Header("Smart matching")]
        [SerializeField] private TextMatchMode matchMode = TextMatchMode.Equals;

        public string QuestionId => questionId;
        public string PromptText => promptText;
        public MatchRuleType RuleType => ruleType;
        public string RuleValue => ruleValue;
        public TextMatchMode MatchMode => matchMode;

        public bool IsMatch(ArtItemSO item)
        {
            if (item == null) return false;

            switch (ruleType)
            {
                case MatchRuleType.Style:
                    return MatchText(item.Style, ruleValue, matchMode);

                case MatchRuleType.Author:
                    return MatchText(item.Author, ruleValue, matchMode);

                case MatchRuleType.Tag:
                    var tags = item.Tags;
                    if (tags == null) return false;

                    for (int i = 0; i < tags.Count; i++)
                    {
                        if (MatchText(tags[i], ruleValue, matchMode))
                            return true;
                    }
                    return false;

                default:
                    return false;
            }
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

        private void OnValidate()
        {
            questionId = questionId?.Trim();
            ruleValue = ruleValue?.Trim();
        }
    }
}
