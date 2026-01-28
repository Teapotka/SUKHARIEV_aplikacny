using System;
using UnityEngine;

namespace BA.Core.Settings
{
    public enum UiVariant
    {
        Minimal,
        Gamified
    }

    [Serializable]
    public class AppSettings
    {
        public UiVariant uiVariant = UiVariant.Minimal;
        public string profileId = "anon";
    }
}
