using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BA.Modes.Explore
{
    public class ExploreCameraRail : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float speedZ = 4f;
        [SerializeField] private float speedX = 4f;

        [SerializeField] private float minLocalZ = 0f;
        [SerializeField] private float maxLocalZ = 5f;

        [SerializeField] private float minLocalX = -2f;
        [SerializeField] private float maxLocalX = 2f;

        [Header("Head Lift (Space / 2-finger hold)")]
        [SerializeField] private float liftPitchDegrees = 8f;
        [SerializeField] private float liftSmooth = 8f;

        [Header("Touch control")]
        [SerializeField] private float dragSensitivity = 0.032f;
        [SerializeField] private bool ignoreTouchOverUI = true;
        [SerializeField] private bool enableTwoFingerLift = true;

        private Quaternion _baseLocalRotation;

        private Vector2 _lastPointerPos;
        private bool _pointerDown;

        private void Awake()
        {
            _baseLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            GetMoveInput(out float zInput, out float xInput);

            if (Mathf.Abs(zInput) > 0.01f || Mathf.Abs(xInput) > 0.01f)
            {
                var local = transform.localPosition;

                local.z += zInput * speedZ * Time.deltaTime;
                local.z = Mathf.Clamp(local.z, minLocalZ, maxLocalZ);

                local.x += xInput * speedX * Time.deltaTime;
                local.x = Mathf.Clamp(local.x, minLocalX, maxLocalX);

                transform.localPosition = local;
            }

            bool lift = IsLiftHeld();

            float targetPitch = lift ? -liftPitchDegrees : 0f;
            Quaternion targetRot = _baseLocalRotation * Quaternion.Euler(0f, 0f, targetPitch);

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRot,
                1f - Mathf.Exp(-liftSmooth * Time.deltaTime)
            );
        }

        private void GetMoveInput(out float zInput, out float xInput)
        {
            zInput = 0f;
            xInput = 0f;

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) zInput -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) zInput += 1f;

                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) xInput -= 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) xInput += 1f;

                if (Mathf.Abs(zInput) > 0.01f || Mathf.Abs(xInput) > 0.01f)
                    return;
            }

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                if (touch.press.wasPressedThisFrame)
                {
                    if (ignoreTouchOverUI && IsTouchOverUI()) return;

                    _pointerDown = true;
                    _lastPointerPos = touch.position.ReadValue();
                }

                if (touch.press.isPressed && _pointerDown)
                {
                    Vector2 pos = touch.position.ReadValue();
                    Vector2 delta = pos - _lastPointerPos;
                    _lastPointerPos = pos;

                    zInput = delta.x * dragSensitivity;
                    xInput = delta.y * dragSensitivity;
                    return;
                }

                if (touch.press.wasReleasedThisFrame)
                {
                    _pointerDown = false;
                    return;
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    _pointerDown = true;
                    _lastPointerPos = mouse.position.ReadValue();
                }

                if (mouse.leftButton.isPressed && _pointerDown)
                {
                    Vector2 pos = mouse.position.ReadValue();
                    Vector2 delta = pos - _lastPointerPos;
                    _lastPointerPos = pos;

                    zInput = delta.x * dragSensitivity;
                    xInput = delta.y * dragSensitivity;
                    return;
                }

                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    _pointerDown = false;
                    return;
                }
            }
        }

        private bool IsLiftHeld()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.spaceKey.isPressed) return true;

            if (enableTwoFingerLift && Touchscreen.current != null)
            {
                int pressedCount = 0;
                foreach (var t in Touchscreen.current.touches)
                {
                    if (t != null && t.press.isPressed) pressedCount++;
                    if (pressedCount >= 2) return true;
                }
            }

            return false;
        }

        private static bool IsTouchOverUI()
        {
            if (EventSystem.current == null || Touchscreen.current == null) return false;
            int id = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(id);
        }

    }
}