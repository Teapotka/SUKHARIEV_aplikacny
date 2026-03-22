using UnityEngine;
using UnityEngine.InputSystem;

namespace BA.Modes.Arcade
{
    public class ArcadeInputController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask tileLayer;
        [SerializeField] private PuzzleBoard board;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null || board == null) return;

            if (!TryGetPrimaryPressScreenPosition(out var screenPos))
                return;

            var ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 200f, tileLayer, QueryTriggerInteraction.Collide))
            {
                var tile = hit.collider.GetComponentInParent<PuzzleTile>();
                if (tile != null)
                    board.SelectTile(tile);
            }
        }

        private static bool TryGetPrimaryPressScreenPosition(out Vector2 screenPos)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                {
                    screenPos = touch.position.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
                return true;
            }

            screenPos = default;
            return false;
        }
    }
}