using System;

namespace BA.Telemetry
{
    [Serializable]
    public class SessionInfo
    {
        public string sessionId;
        public string profileId;
        public string uiVariant;    // "Minimal" | "Gamified"
        public string appVersion;
        public string startedAtUtc;

        public static SessionInfo Create(string uiVariant, string profileId = "anon", string appVersion = "0.1.0")
        {
            return new SessionInfo
            {
                sessionId = Guid.NewGuid().ToString("N"),
                profileId = profileId,
                uiVariant = uiVariant,
                appVersion = appVersion,
                startedAtUtc = DateTime.UtcNow.ToString("o"),
            };
        }
    }
}
