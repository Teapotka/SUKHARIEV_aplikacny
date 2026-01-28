using BA.Data;
using UnityEngine;

namespace BA.Modes.Arcade
{
    public class ArcadeModeController : BA.Modes.ModeControllerBase
    {
        protected override GameModeType ModeType => GameModeType.Arcade;
        private void Reset()
        {
            modeName = "Arcade";
        }

        protected override void EnterState(ModeState state)
        {
            if (state == ModeState.Intro)
                TransitionTo(ModeState.Play);
        }
    }
}
