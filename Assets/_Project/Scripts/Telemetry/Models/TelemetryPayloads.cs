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
        public bool helpEnabled;
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
        public float timeLimitSeconds;
        public int col;
        public int row;
        public string frontId;
        public string backId;
    }

    [Serializable]
    public class ArcadeRoundEndPayload
    {
        public bool win;
        public string reason; // "solved" | "timeout"
        public float timeLimitSeconds;
        public float timeLeftSeconds;
        public float durationSeconds;
        public int col;
        public int row;
        public string frontId;
        public string backId;
    }
}
