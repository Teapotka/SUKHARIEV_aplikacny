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
}
