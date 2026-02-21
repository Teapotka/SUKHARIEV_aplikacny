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

        [SerializeField] private bool forceDefaultOnStart = true;

        private List<CinemachineCamera> _order;
        private int _index = 0;

    private CinemachineCamera _current;
        private readonly Stack<CinemachineCamera> _history = new();

        private void Awake()
        {
        _order = new List<CinemachineCamera>(3);
        if (camDefault) _order.Add(camDefault);
        if (camZoom) _order.Add(camZoom);
        if (camRotate) _order.Add(camRotate);

        if (_order.Count == 0)
        {
            Debug.LogError("[MatchCameraViews] No cameras assigned.");
            return;
        }

        if (forceDefaultOnStart && camDefault != null)
            _index = _order.IndexOf(camDefault) >= 0 ? _order.IndexOf(camDefault) : 0;

        ApplyActive(_order[_index]);
        }

        // ---------- Button API ----------

        public void ShowDefault() => Activate(camDefault);
        public void ShowZoom() => Activate(camZoom);
        public void ShowRotate() => Activate(camRotate);

    private void SetView(CinemachineCamera cam)
    {
        if (cam == null || _order == null) return;

        int i = _order.IndexOf(cam);
        if (i < 0) return;

        _index = i;
        ApplyActive(_order[_index]);
    }

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

    public void NextView()
    {
        if (_order == null || _order.Count == 0) return;

        _index = (_index + 1) % _order.Count;
        ApplyActive(_order[_index]);
    }

    private void SetPriority(CinemachineCamera cam, bool active)
        {
            if (cam == null) return;
            cam.Priority = active ? activePriority : inactivePriority;
        }

    private void ApplyActive(CinemachineCamera active)
    {
        SetPriority(camDefault, active == camDefault);
        SetPriority(camZoom, active == camZoom);
        SetPriority(camRotate, active == camRotate);
    }
}



