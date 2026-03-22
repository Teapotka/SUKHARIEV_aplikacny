using UnityEngine;
using UnityEngine.InputSystem;

namespace BA.Modes.Arcade
{
    public class ArcadeCameraFlip : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float rotateDuration = 0.6f;

        [Header("Mobile shortcut (optional)")]
        [SerializeField] private bool enableTwoFingerTap = true;
        [SerializeField] private float twoFingerTapMaxTime = 0.25f;

        private bool flipped = false;
        private bool rotating = false;

        private Quaternion frontRot;
        private Quaternion backRot;

        [SerializeField] private ArcadeSideLabels sideLabels;
        [SerializeField] private SwapLRButtons swapLR;

        private float twoFingerTimer;
        private bool twoFingerActive;

        private void Awake()
        {
            frontRot = transform.rotation;
            backRot = frontRot * Quaternion.Euler(0f, 180f, 0f);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                Toggle();

            if (enableTwoFingerTap)
                DetectTwoFingerTap();
        }

        private void DetectTwoFingerTap()
        {
            if (Touchscreen.current == null) return;

            var touches = Touchscreen.current.touches;

            int pressedCount = 0;
            bool anyReleasedThisFrame = false;

            foreach (var t in touches)
            {
                if (t == null) continue;

                if (t.press.isPressed) pressedCount++;
                if (t.press.wasReleasedThisFrame) anyReleasedThisFrame = true;
            }

            if (pressedCount >= 2 && !twoFingerActive)
            {
                twoFingerActive = true;
                twoFingerTimer = 0f;
            }

            if (twoFingerActive)
            {
                twoFingerTimer += Time.deltaTime;

                if (anyReleasedThisFrame && twoFingerTimer <= twoFingerTapMaxTime)
                {
                    Toggle();
                    twoFingerActive = false;
                    return;
                }

                if (pressedCount < 2 || twoFingerTimer > twoFingerTapMaxTime)
                {
                    twoFingerActive = false;
                }
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
}