using UnityEngine;
using UnityEngine.InputSystem;

namespace BA.Modes.Match
{
    public class MatchInputController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask cardLayer;
        [SerializeField] private float dragPlaneHeight = 0.8f;

        private DraggableCard3D active;

        private bool usingTouch;

        public bool IsFrozen { get; private set; }

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null) return;
            if (IsFrozen) return;

            if (!TryGetPointerState(out var pointer))
                return;

            if (pointer.pressedThisFrame)
            {
                var ray = cam.ScreenPointToRay(pointer.screenPos);
                if (Physics.Raycast(ray, out var hit, 200f, cardLayer, QueryTriggerInteraction.Collide))
                {
                    var card = hit.collider.GetComponentInParent<DraggableCard3D>();
                    if (card != null)
                    {
                        if (card.IsPlaced)
                        {
                            card.RemoveToStart();
                            return;
                        }

                        active = card;
                        active.BeginDrag(cam, dragPlaneHeight, pointer.screenPos);
                    }
                }
            }

            if (pointer.isPressed && active != null)
            {
                active.Drag(cam, dragPlaneHeight, pointer.screenPos);

                if (active.JustSnapped)
                    active = null;
            }

            if (pointer.releasedThisFrame && active != null)
            {
                active.EndDrag(cam, pointer.screenPos);
                active = null;
            }
        }

        private struct PointerState
        {
            public Vector2 screenPos;
            public bool pressedThisFrame;
            public bool releasedThisFrame;
            public bool isPressed;
        }

        private bool TryGetPointerState(out PointerState state)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame)
                {
                    usingTouch = true;

                    state = new PointerState
                    {
                        screenPos = touch.position.ReadValue(),
                        pressedThisFrame = touch.press.wasPressedThisFrame,
                        releasedThisFrame = touch.press.wasReleasedThisFrame,
                        isPressed = touch.press.isPressed
                    };
                    return true;
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                usingTouch = false;

                state = new PointerState
                {
                    screenPos = mouse.position.ReadValue(),
                    pressedThisFrame = mouse.leftButton.wasPressedThisFrame,
                    releasedThisFrame = mouse.leftButton.wasReleasedThisFrame,
                    isPressed = mouse.leftButton.isPressed
                };
                return true;
            }

            state = default;
            return false;
        }

        public void SetFrozen(bool frozen)
        {
            IsFrozen = frozen;

            if (active != null)
            {
                active.CancelDragToStart();
                active = null;
            }
        }
    }
}