using System.Collections.Generic;
using BA.Data;

    public class MatchRound
    {
        public MatchQuestionSO Question { get; set; }

        public List<ArtItemSO> Correct { get; set; } = new();

        public List<ArtItemSO> Distractors { get; set; } = new();

        public List<ArtItemSO> TableItems { get; set; } = new();
    }
