using System;

namespace BA.Telemetry
{
    [Serializable]
    public class SessionStartPayload
    {
        public string appVersion;
        public string unityVersion;
        public string platform;
    }

    [Serializable]
    public class SessionEndPayload
    {
        public string reason;
    }

    [Serializable]
    public class ModeStartPayload
    {
        public int itemCount;
        public float timeLimitSeconds;
    }

    [Serializable]
    public class UnlockPayload
    {
        public int from;
        public int to;
        public int max;
        public string unlockedItemId;
        public int streakRequired;
    }

    [Serializable]
    public class ViewedPayload
    {
        public string itemId;
        public int viewedCount;
        public int unlockedCount;
    }

    [Serializable]
    public class DifficultyPayload
    {
        public int from;
        public int to;
        public int unlockedCount;
    }


    [Serializable]
    public class ArcadeRoundStartPayload
    {
        public string roundId;
        public float timeLimitSeconds;
        public int col;
        public int row;
        public string frontId;
        public string backId;

        public int presetIndex;
        public int unlockedCount;
        public int viewedCount;
    }

    [Serializable]
    public class ArcadeRoundEndPayload
    {
        public string roundId;
        public bool win;
        public string reason;

        public float timeLimitSeconds;
        public float timeLeftSeconds;
        public float durationSeconds;

        public int moves;
        public int score;
        public string medal;

        public int slideMoveCount;
        public int flipCount;
        public int initialBackTileCount;
        public int errorCount;

        public int col;
        public int row;
        public string frontId;
        public string backId;

        public int presetIndex;
    }

    [Serializable]
    public class ExploreModeEndPayload
    {
        public string reason;
        public float durationSeconds;

        public int unlockedCountStart;
        public int unlockedCountEnd;

        public int viewedCountStart;
        public int viewedCountEnd;

        public int newlyViewedInSession;
    }

    [Serializable]
    public class MatchModeEndPayload
    {
        public string reason;
        public float durationSeconds;

        public int unlockedCount;
        public int viewedCount;

        public int effectiveDifficulty;
        public int ddaOffset;
    }

    [Serializable]
    public class MatchRageQuitPayload
    {
        public string reason;
        public string roundId;
        public string questionId;
        public float durationSeconds;
    }

    [Serializable]
    public class MatchTaskStartPayload
    {
        public string roundId;
        public string questionId;

        public int poolCount;
        public int totalCards;
        public int correctCards;

        public int unlockedCount;
        public int viewedCount;

        public int baseDifficulty;
        public int ddaOffset;
        public int effectiveDifficulty;
    }


    [Serializable]
    public class MatchTaskEndPayload
    {
        public string roundId;
        public string questionId;

        public bool win;
        public int incorrect;
        public bool allSlotsFilled;
        public float durationSeconds;

        public int effectiveDifficulty;
    }

    [Serializable]
    public class MatchResultPayload
    {
        public string roundId;
        public string questionId;

        public bool win;
        public int incorrect;
        public bool allSlotsFilled;
        public float durationSeconds;
    }

    [Serializable]
    public class ArcadeRageQuitPayload
    {
        public string reason;
        public string roundId;
        public float durationSeconds;
        public int moves;
        public float timeLeftSeconds;
        public float timeLimitSeconds;
        public string frontId;
        public string backId;

        public int slideMoveCount;
        public int flipCount;
        public int initialBackTileCount;
        public int errorCount;
    }

    [Serializable]
    public class ArcadeModeEndPayload
    {
        public string reason;
        public float durationSeconds;
        public int presetIndex;
        public int unlockedCount;
        public int viewedCount;
    }

    [Serializable]
    public class ArcadeActionPayload
    {
        public string roundId;
        public string action; // move_up/move_down/move_left/move_right/flip_tile
        public int moves;
        public float timeLeftSeconds;
    }

    [Serializable]
    public class UiSwitchPayload
    {
        public string from;
        public string to;
    }

    [Serializable]
    public class MatchDdaChangedPayload
    {
        public int fromOffset;
        public int toOffset;

        public int windowSize;
        public int wins;
        public float avgIncorrect;

        public string reason; // "harder" | "easier"
    }

    [Serializable]
    public class ArcadeDdaChangedPayload
    {
        public int fromPreset;
        public int toPreset;

        public int windowSize;
        public int wins;
        public int timeouts;
        public float avgWinTimeRatio;
        public float avgWinMoves;

        public string reason; // "harder" | "easier"
    }
}
