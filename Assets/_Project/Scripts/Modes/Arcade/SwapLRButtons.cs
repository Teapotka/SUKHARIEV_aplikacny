using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BA.Modes.Arcade
{
    public class SwapLRButtons : MonoBehaviour
    {
        [Header("Buttons (all skins)")]
        [SerializeField] private Button[] leftButtons;
        [SerializeField] private Button[] rightButtons;

        [Header("Effects (normal mapping)")]
        [SerializeField] private UnityEvent onLeftEffect;
        [SerializeField] private UnityEvent onRightEffect;

        [Header("Safety")]
        [Tooltip("Recommended ON: remove existing onClick listeners so only this router controls movement.\n" +
                 "If OFF and your buttons already call movement methods, you'll get double actions.")]
        [SerializeField] private bool clearExistingListeners = true;

        private bool _flipped;

        private void Awake()
        {
            Wire(leftButtons, HandleLeftPressed);
            Wire(rightButtons, HandleRightPressed);
        }

        public void Apply(bool flipped)
        {
            _flipped = flipped;
        }

        private void Wire(Button[] buttons, UnityAction handler)
        {
            if (buttons == null) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                var b = buttons[i];
                if (b == null) continue;

                if (clearExistingListeners)
                    b.onClick.RemoveAllListeners();

                b.onClick.AddListener(handler);
            }
        }

        private void HandleLeftPressed()
        {
            if (!_flipped) onLeftEffect?.Invoke();
            else onRightEffect?.Invoke();
        }

        private void HandleRightPressed()
        {
            if (!_flipped) onRightEffect?.Invoke();
            else onLeftEffect?.Invoke();
        }
    }
}