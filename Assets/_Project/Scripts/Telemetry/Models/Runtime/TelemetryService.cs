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
            Flush(); // write immediately so the file exists
        }

        public void SetUiVariant(string uiVariant)
        {
            if (Session == null) return;

            var old = Session.uiVariant;
            Session.uiVariant = uiVariant;

            Log(TelemetryEventType.UI_SWITCH, DefaultMode, new { from = old, to = uiVariant });
            Flush();
        }

        public void Log(TelemetryEventType type, string mode, object payload = null)
        {
            if (Session == null)
            {
                Debug.LogWarning("[Telemetry] Session not started. Call StartSession() in Bootstrapper.");
                return;
            }

            var ev = TelemetryEvent.Create(Session, type, mode, payload);

            var line = JsonUtility.ToJson(ev);
            _ndjsonBuffer.Add(line);
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
}
