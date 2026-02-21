using UnityEngine;
using TMPro;

namespace BA.UI
{
    public enum UiStyle
    {
        Minimal,
        Gamified
    }

    [CreateAssetMenu(menuName = "BA/UI/UI Theme", fileName = "UITheme_")]
    public class UIThemeSO : ScriptableObject
    {
        [Header("Identity")]
        public UiStyle style = UiStyle.Gamified;

        [Header("Fonts")]
        public TMP_FontAsset fontHeader;
        public TMP_FontAsset fontBody;
        public TMP_FontAsset fontButton;

        [Header("Colors")]
        public Color textPrimary = Color.white;
        public Color textSecondary = new Color(1f, 1f, 1f, 0.75f);
        public Color accent = new Color(0.2f, 0.9f, 1f, 1f);
        public Color success = new Color(0.2f, 1f, 0.2f, 1f);
        public Color fail = new Color(1f, 0.25f, 0.25f, 1f);

        [Header("Sprites (Panels / Buttons)")]
        public Sprite panelSquare;     // 1:1 frame
        public Sprite panelWide16x9;    // 16:9 frame
        public Sprite buttonSquare;     // generic square button
        public Sprite buttonRound;      // rotate button, etc.
        public Sprite buttonHome;       // home icon/button base (if separate)

        [Header("Icons")]
        public Sprite iconHome;
        public Sprite iconCamera;
        public Sprite iconRotate;
        public Sprite iconFlip;
        public Sprite iconUp;
        public Sprite iconDown;
        public Sprite iconLeft;
        public Sprite iconRight;

        [Header("Audio")]
        public AudioClip sfxClick;
        public AudioClip sfxCorrect;
        public AudioClip sfxWrong;
        public AudioClip sfxWin;
        public AudioClip sfxWhoosh;

        [Header("Motion / Feel")]
        [Range(1f, 1.25f)] public float hoverScale = 1.06f;
        [Range(1f, 1.35f)] public float pressScale = 1.10f;
        [Range(0.05f, 0.25f)] public float animTime = 0.12f;

        [Header("Visibility toggles (Minimal vs Gamified)")]
        public bool showScore = true;
        public bool showProgressBar = true;
        public bool showToasts = true;
        public bool showMedals = true;
        public bool showTimer = true;
    }
}
