using BA.Core.Progress;
using BA.Data;
using BA.Telemetry;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BA.Modes.Explore
{
    public class ExploreModeController : BA.Modes.ModeControllerBase
    {

        private bool _modeStartLogged;
        private float _modeStartRealtime;

        private int _startUnlockedCount;
        private int _startViewedCount;
        private void Reset()
        {
            modeName = "Explore";
        }

        private void OnEnable()
        {
            LogModeStartIfNeeded();
        }

        private void OnDisable()
        {
            LogModeEndIfNeeded("scene_unload");
        }

        protected override void EnterState(ModeState state)
        {
            switch (state)
            {
                case ModeState.Intro:
                    TransitionTo(ModeState.Play);
                    break;

                case ModeState.Play:
                    break;
            }
        }
        private void Update()
        {
            if (State != ModeState.Play) return;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                TrySelect();
        }

        private void TrySelect()
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            var ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out var hit, 200f)) return;

            var view = hit.collider.GetComponentInParent<ExploreArtworkView>();
            if (view == null || view.Item == null) return;

            view.ToggleFlip();
        }


        private void LogModeStartIfNeeded()
        {
            if (_modeStartLogged) return;

            ProgressService.Instance?.EnsureExploreInitializedForActiveCollection();

            _modeStartLogged = true;
            _modeStartRealtime = Time.realtimeSinceStartup;

            _startUnlockedCount = ProgressService.Instance != null ? ProgressService.Instance.UnlockedCount : 0;
            _startViewedCount = ProgressService.Instance != null ? ProgressService.Instance.ViewedCount : 0;

            TelemetryService.Instance?.Log(
                TelemetryEventType.MODE_START,
                modeName,
                new ModeStartPayload
                {
                    itemCount = _startUnlockedCount,
                    timeLimitSeconds = 0f,
                }
            );

            TelemetryService.Instance?.Flush();
        }

        private void LogModeEndIfNeeded(string reason)
        {
            if (!_modeStartLogged) return;

            var durationSeconds = Time.realtimeSinceStartup - _modeStartRealtime;

            int endUnlocked = ProgressService.Instance != null ? ProgressService.Instance.UnlockedCount : _startUnlockedCount;
            int endViewed = ProgressService.Instance != null ? ProgressService.Instance.ViewedCount : _startViewedCount;

            int viewedDelta = Mathf.Max(0, endViewed - _startViewedCount);

            TelemetryService.Instance?.Log(
                TelemetryEventType.MODE_END,
                modeName,
                new ExploreModeEndPayload
                {
                    reason = reason,
                    durationSeconds = durationSeconds,
                    unlockedCountStart = _startUnlockedCount,
                    unlockedCountEnd = endUnlocked,
                    viewedCountStart = _startViewedCount,
                    viewedCountEnd = endViewed,
                    newlyViewedInSession = viewedDelta
                }
            );

            TelemetryService.Instance?.Flush();
        }
    }
}

