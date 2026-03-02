using UnityEngine;


namespace BA.Modes.Match
{
    [RequireComponent(typeof(Collider))]
    public class DraggableCard3D : MonoBehaviour
    {
        [Header("Snap")]
        [SerializeField] private LayerMask socketLayer;
        [SerializeField] private float snapRadius = 0.25f;

        [Header("Drag constraints")]
        [SerializeField] private bool lockRotation = true;
        [SerializeField] private bool lockY = true;

        [Tooltip("If lockY = true, the card will stay at this Y while dragging.")]
        [SerializeField] private float fixedY = 0.8f;

        [Tooltip("Clamp in world space. Set minX <= maxX and minZ <= maxZ.")]
        [SerializeField] private bool clampXZ = true;
        [SerializeField] private float minX = -2f;
        [SerializeField] private float maxX = 2f;
        [SerializeField] private float minZ = -2f;
        [SerializeField] private float maxZ = 2f;

        [Header("Drag rotation")]
        [SerializeField] private Vector3 dragRotationOffsetEuler = new Vector3(0f, 0f, 90f);
        private Quaternion dragRot;

        private ArtCardView view;
        private Vector3 startPos;
        private Quaternion startRot;
        private bool dragging;

        public bool IsPlaced => currentSocket != null;
        public bool JustSnapped { get; private set; }

        private MatchSocket currentSocket;

        private void Awake() => view = GetComponent<ArtCardView>();

        public void BeginDrag(Camera cam, float planeY, Vector2 screenPos)
        {
            startPos = transform.position;
            startRot = transform.rotation;

            dragRot = startRot * Quaternion.Euler(dragRotationOffsetEuler);

            dragging = true;
        }

        public void Drag(Camera cam, float planeY, Vector2 screenPos)
        {
            if (!dragging) return;

            var plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
            var ray = cam.ScreenPointToRay(screenPos);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 p = ray.GetPoint(enter);

                if (lockY) p.y = fixedY;

                if (clampXZ)
                {
                    p.x = Mathf.Clamp(p.x, minX, maxX);
                    p.z = Mathf.Clamp(p.z, minZ, maxZ);
                }

                transform.position = p;
            }

            if (lockRotation)
                transform.rotation = dragRot;
        }

        public void EndDrag(Camera cam, Vector2 screenPos)
        {
            if (!dragging) return;
            dragging = false;

            var ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 200f, socketLayer, QueryTriggerInteraction.Collide))
            {
                var socket = hit.collider.GetComponentInParent<MatchSocket>();
                if (socket != null &&
                    Vector3.Distance(transform.position, socket.SnapPoint.position) <= snapRadius &&
                    socket.TryPlace(view))
                {
                    return;
                }
            }

            transform.position = startPos;
            transform.rotation = startRot;
        }
        public void RemoveToStart()
        {
            if (currentSocket != null)
            {
                currentSocket.Clear();
                currentSocket = null;
            }

            transform.position = startPos;
            transform.rotation = startRot;
            dragging = false;
            JustSnapped = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!dragging || currentSocket != null) return;

            var socket = other.GetComponentInParent<MatchSocket>();
            if (socket == null) return;

            if (socket.IsOccupied) return;

            if (socket.TryPlace(view))
            {
                currentSocket = socket;

                dragging = false;

                JustSnapped = true;
            }
        }

        public void CancelDragToStart()
        {

            if (!IsPlaced)
            {
                transform.position = startPos;
                transform.rotation = startRot;
            }

            dragging = false;
            JustSnapped = false;
        }
    }
}