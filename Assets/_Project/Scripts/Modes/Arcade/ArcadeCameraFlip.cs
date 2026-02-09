using UnityEngine;
using UnityEngine.InputSystem;


    public class ArcadeCameraFlip : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float rotateDuration = 0.6f;

        private bool flipped = false;
        private bool rotating = false;

        private Quaternion frontRot;
        private Quaternion backRot;

    [SerializeField] private ArcadeSideLabels sideLabels;
    [SerializeField] private SwapLRButtons swapLR;

    private void Awake()
        {
            frontRot = transform.rotation;
            backRot = frontRot * Quaternion.Euler(0f, 180f, 0f);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (rotating) return;

            flipped = !flipped;
            StopAllCoroutines();
            StartCoroutine(RotateRoutine(flipped ? backRot : frontRot));

            sideLabels?.SetBackView(flipped, rotateDuration);
            swapLR?.Apply(flipped);
    }

        private System.Collections.IEnumerator RotateRoutine(Quaternion target)
        {
            rotating = true;

            Quaternion start = transform.rotation;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, rotateDuration);
                transform.rotation = Quaternion.Slerp(start, target, t);
                yield return null;
            }

            transform.rotation = target;
            rotating = false;
        }
    }
