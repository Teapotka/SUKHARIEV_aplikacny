using UnityEngine;
using UnityEngine.InputSystem;

public class MatchInputController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask cardLayer;
        [SerializeField] private float dragPlaneHeight = 0.8f;

        private DraggableCard3D active;

        public bool IsFrozen { get; private set; }


    private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (IsFrozen) return;

        Vector2 screenPos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            var ray = cam.ScreenPointToRay(screenPos);
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
                    active.BeginDrag(cam, dragPlaneHeight, screenPos);
                }
            }
        }

        if (mouse.leftButton.isPressed && active != null)
        {
            active.Drag(cam, dragPlaneHeight, screenPos);

            if (active.JustSnapped)
                active = null;
        }

        if (mouse.leftButton.wasReleasedThisFrame && active != null)
        {
            active.EndDrag(cam, screenPos);
            active = null;
        }
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

