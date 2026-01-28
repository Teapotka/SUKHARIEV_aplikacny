using BA.Data;
using UnityEngine;

namespace BA.Modes.Match
{
    public class MatchModeController : BA.Modes.ModeControllerBase
    {
        protected override GameModeType ModeType => GameModeType.Match;
        private void Reset()
        {
            modeName = "Match";
        }

        protected override void EnterState(ModeState state)
        {
            if (state == ModeState.Intro)
                TransitionTo(ModeState.Play);
        }
    }
}
