using BA.Core;
using BA.Data;
using BA.Telemetry;
using UnityEngine;

namespace BA.Modes
{
    public abstract class ModeControllerBase : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] protected string modeName = "Unknown";

        protected ModeState State { get; private set; } = ModeState.Idle;
        protected virtual GameModeType ModeType => GameModeType.Explore;

        protected virtual void Start()
        {
            var cfg = GameContext.Instance?.GameData?.GetConfig(ModeType);

            if (cfg != null)
            {
                TelemetryService.Instance?.Log(
                    TelemetryEventType.MODE_START,
                    modeName,
                    new ModeStartPayload
                    {
                        itemCount = cfg.BaseItemCount,
                        timeLimitSeconds = cfg.TimeLimitSeconds,
                        helpEnabled = cfg.HelpEnabled
                    });
            }
            else
            {
                TelemetryService.Instance?.Log(TelemetryEventType.MODE_START, modeName);
            }

            TelemetryService.Instance?.Flush();

            TransitionTo(ModeState.Intro);
        }

        protected virtual void OnDestroy()
        {
            TelemetryService.Instance?.Log(
                TelemetryEventType.MODE_END,
                modeName,
                new SessionEndPayload { reason = "scene_unload" }
            );
            TelemetryService.Instance?.Flush();
        }

        protected void TransitionTo(ModeState next)
        {
            ExitState(State);
            State = next;
            EnterState(State);
        }

        protected virtual void EnterState(ModeState state)
        {
        }

        protected virtual void ExitState(ModeState state)
        {
        }
    }
}
