using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BA.Telemetry
{
    public class TelemetryService : MonoBehaviour
    {
        public static TelemetryService Instance { get; private set; }

        public SessionInfo Session { get; private set; }

        private readonly List<string> _ndjsonBuffer = new();
        private string _filePath;

        private const string DefaultMode = "Menu";

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.quitting += OnAppQuitting;
        }

        public void StartSession(string uiVariant, string profileId = "anon", string appVersion = "0.1.0")
        {
            Session = SessionInfo.Create(uiVariant, profileId, appVersion);

            var dir = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(dir);

            var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            _filePath = Path.Combine(dir, $"telemetry_{Session.profileId}_{stamp}_{Session.sessionId}.ndjson");

            Log(TelemetryEventType.SESSION_START, DefaultMode, new SessionStartPayload
            {
                appVersion = Session.appVersion,
                unityVersion = UnityEngine.Application.unityVersion,
                platform = UnityEngine.Application.platform.ToString()
            });
            Flush();
        }

        public void SetUiVariant(string uiVariant)
        {
            if (Session == null) return;
            if (string.IsNullOrWhiteSpace(uiVariant)) return;

            uiVariant = uiVariant.Trim();

            var old = Session.uiVariant;

            if (string.Equals(old, uiVariant, StringComparison.OrdinalIgnoreCase))
                return;

            Session.uiVariant = uiVariant;

            Log(TelemetryEventType.UI_SWITCH, DefaultMode, new UiSwitchPayload
            {
                from = old,
                to = uiVariant
            });

            Flush();
        }

        public void Log(TelemetryEventType type, string mode, object payload = null)
        {
            if (Session == null)
            {
                Debug.LogWarning("[Telemetry] Session not started. Call StartSession() in Bootstrapper.");
                return;
            }

            var header = new TelemetryHeader
            {
                sessionId = Session.sessionId,
                profileId = Session.profileId,
                uiVariant = Session.uiVariant,

                timestampUtc = DateTime.UtcNow.ToString("o"),
                eventType = type.ToString(),
                mode = mode
            };

            string headerJson = JsonUtility.ToJson(header);
            string payloadJson = payload == null ? "{}" : JsonUtility.ToJson(payload);

            string line = ComposeWithRawPayload(headerJson, payloadJson);

            //var ev = TelemetryEvent.Create(Session, type, mode, payload);

            //var line = JsonUtility.ToJson(ev);
            _ndjsonBuffer.Add(line);
        }

        private static string ComposeWithRawPayload(string headerJson, string payloadJson)
        {
            if (string.IsNullOrEmpty(headerJson)) headerJson = "{}";
            if (string.IsNullOrEmpty(payloadJson)) payloadJson = "{}";

            // Ensure header ends with "}"
            if (headerJson[headerJson.Length - 1] != '}')
                headerJson += "}";

            // Remove last "}" and append ,"payload":<raw> }
            return headerJson.Substring(0, headerJson.Length - 1) + ",\"payload\":" + payloadJson + "}";
        }

        public void Flush()
        {
            if (string.IsNullOrEmpty(_filePath) || _ndjsonBuffer.Count == 0)
                return;

            try
            {
                File.AppendAllLines(_filePath, _ndjsonBuffer, Encoding.UTF8);
                _ndjsonBuffer.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Telemetry] Failed to write telemetry: {e.Message}");
            }
        }

        private void OnAppQuitting()
        {
            if (Session == null) return;

            Log(TelemetryEventType.SESSION_END, DefaultMode, new SessionEndPayload { reason = "quit" });
            Flush();
        }
    }
    [Serializable]
    internal class TelemetryHeader
    {
        public string sessionId;
        public string profileId;
        public string uiVariant;

        public string timestampUtc;
        public string eventType;
        public string mode;
    }
}