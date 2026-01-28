using System;
using UnityEngine;

namespace BA.Telemetry
{
    [Serializable]
    public class TelemetryEvent
    {
        public string sessionId;
        public string profileId;
        public string uiVariant;

        public string timestampUtc;     // ISO 8601
        public TelemetryEventType eventType;
        public string mode;             // "Explore" | "Match" | "Arcade" | "Menu"
        public string payloadJson;

        public static TelemetryEvent Create(SessionInfo session, TelemetryEventType type, string mode, object payload = null)
        {
            return new TelemetryEvent
            {
                sessionId = session.sessionId,
                profileId = session.profileId,
                uiVariant = session.uiVariant,
                timestampUtc = DateTime.UtcNow.ToString("o"),
                eventType = type,
                mode = mode,
                payloadJson = payload == null ? "{}" : JsonUtility.ToJson(payload)
            };
        }
    }
}
