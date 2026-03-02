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

        protected virtual void Start()
        {
            TransitionTo(ModeState.Intro);
        }

        protected virtual void OnDestroy()
        {
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
