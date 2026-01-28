using BA.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BA.Modes.Explore
{
    public class ExploreModeController : BA.Modes.ModeControllerBase
    {
        protected override GameModeType ModeType => GameModeType.Explore;
        private void Reset()
        {
            modeName = "Explore";
        }

        protected override void EnterState(ModeState state)
        {
            switch (state)
            {
                case ModeState.Intro:
                    // TODO: show intro UI
                    TransitionTo(ModeState.Play);
                    break;

                case ModeState.Play:
                    // TODO: gallery interaction
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
    }
}
