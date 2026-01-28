using UnityEngine;
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

        [Header("Head Lift (Space)")]
        [SerializeField] private float liftPitchDegrees = 8f; 
        [SerializeField] private float liftSmooth = 8f;        

        private Quaternion _baseLocalRotation;

        private void Awake()
        {
            _baseLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            float zInput = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) zInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) zInput += 1f;

            float xInput = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) xInput -= 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) xInput += 1f;

            if (Mathf.Abs(zInput) > 0.01f || Mathf.Abs(xInput) > 0.01f)
            {
                var local = transform.localPosition;

                local.z += zInput * speedZ * Time.deltaTime;
                local.z = Mathf.Clamp(local.z, minLocalZ, maxLocalZ);

                local.x += xInput * speedX * Time.deltaTime;
                local.x = Mathf.Clamp(local.x, minLocalX, maxLocalX);

                transform.localPosition = local;
            }

            
            bool lift = Keyboard.current.spaceKey.isPressed;
            float targetPitch = lift ? -liftPitchDegrees : 0f; 
            Quaternion targetRot = _baseLocalRotation * Quaternion.Euler(0f, 0f, targetPitch);

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRot,
                1f - Mathf.Exp(-liftSmooth * Time.deltaTime)
            );
        }
    }
}
