using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

    public class MatchCameraViews : MonoBehaviour
    {
        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera camDefault;
        [SerializeField] private CinemachineCamera camZoom;
        [SerializeField] private CinemachineCamera camRotate;

        [Header("Priority")]
        [SerializeField] private int activePriority = 20;
        [SerializeField] private int inactivePriority = 10;

        private CinemachineCamera _current;
        private readonly Stack<CinemachineCamera> _history = new();

        private void Awake()
        {
            ActivateInternal(camDefault, pushHistory: false);
        }

        // ---------- Button API ----------

        public void ShowDefault() => Activate(camDefault);
        public void ShowZoom() => Activate(camZoom);
        public void ShowRotate() => Activate(camRotate);

        public void Back()
        {
            if (_history.Count == 0) return;

            var previous = _history.Pop();
            ActivateInternal(previous, pushHistory: false);
        }

        // ---------- Internal ----------

        private void Activate(CinemachineCamera next)
        {
            if (next == null || next == _current) return;

            if (_current != null)
                _history.Push(_current);

            ActivateInternal(next, pushHistory: false);
        }

        private void ActivateInternal(CinemachineCamera next, bool pushHistory)
        {
            _current = next;

            SetPriority(camDefault, next == camDefault);
            SetPriority(camZoom, next == camZoom);
            SetPriority(camRotate, next == camRotate);
        }

        private void SetPriority(CinemachineCamera cam, bool active)
        {
            if (cam == null) return;
            cam.Priority = active ? activePriority : inactivePriority;
        }
    }

