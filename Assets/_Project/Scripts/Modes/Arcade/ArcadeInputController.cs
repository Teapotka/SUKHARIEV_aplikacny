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

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (!mouse.leftButton.wasPressedThisFrame) return;

            Vector2 screenPos = mouse.position.ReadValue();
            var ray = cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 200f, tileLayer, QueryTriggerInteraction.Collide))
            {
                var tile = hit.collider.GetComponentInParent<PuzzleTile>();
                if (tile != null)
                    board.SelectTile(tile);
            }
        }
    }
}
