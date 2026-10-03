using System;
using UnityEngine;

namespace MiaShooter
{
    public static class GameSettings
    {
        // Action Constants
        public const string ACTION_FORWARD = "Forward";
        public const string ACTION_BACKWARD = "Backward";
        public const string ACTION_LEFT = "Left";
        public const string ACTION_RIGHT = "Right";
        public const string ACTION_JUMP = "Jump";
        public const string ACTION_FIRE1 = "Fire1";
        public const string ACTION_FIRE2 = "Fire2";
        public const string ACTION_RELOAD = "Reload";
        public const string ACTION_FRAG = "Frag";
        public const string ACTION_SMOKE = "Smoke";

        // Keybinds
        public static KeyCode MoveForwardKey { get; set; } = KeyCode.W;
        public static KeyCode MoveBackwardKey { get; set; } = KeyCode.S;
        public static KeyCode MoveLeftKey { get; set; } = KeyCode.A;
        public static KeyCode MoveRightKey { get; set; } = KeyCode.D;
        public static KeyCode JumpKey { get; set; } = KeyCode.Space;
        public static KeyCode ReloadKey { get; set; } = KeyCode.R;
        public static KeyCode FragGrenadeKey { get; set; } = KeyCode.G;
        public static KeyCode SmokeGrenadeKey { get; set; } = KeyCode.H;
        public static KeyCode PrimaryFireKey { get; set; } = KeyCode.Mouse0;
        public static KeyCode LaserFireKey { get; set; } = KeyCode.Mouse1;
        public static KeyCode ToggleSettingsKey { get; set; } = KeyCode.Escape;

        public static KeyCode GetKeybind(string action) => GetKeyForAction(action);
        public static void SetKeybind(string action, KeyCode key) => SetKeyForAction(action, key);

        // Audio & Gameplay
        public static float MasterVolume { get; set; } = 1.0f;
        public static float SfxVolume { get; set; } = 1.0f;
        public static float MusicVolume { get; set; } = 0.75f;
        public static bool SpatialAudioDemoActive { get; set; } = false;
        public static bool DirectionalSoundIndicatorsEnabled { get; set; } = true;
        public static float MouseSensitivity { get; set; } = 2.0f;
        public static bool ScreenShakeEnabled { get; set; } = true;
        public static float CrosshairScale { get; set; } = 1.0f;
        public static bool ShowKeyHints { get; set; } = true;

        // Runtime Modal State
        public static bool IsSettingsOpen { get; set; } = false;
        public static string CurrentlyRebindingAction { get; set; } = null;
        public static int ActiveSettingsTab { get; set; } = 0; // 0: Keybinds, 1: Audio, 2: Gameplay

        // Events
        public static event Action OnSettingsChanged;

        static GameSettings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            MoveForwardKey = (KeyCode)PlayerPrefs.GetInt("Key_Forward", (int)KeyCode.W);
            MoveBackwardKey = (KeyCode)PlayerPrefs.GetInt("Key_Backward", (int)KeyCode.S);
            MoveLeftKey = (KeyCode)PlayerPrefs.GetInt("Key_Left", (int)KeyCode.A);
            MoveRightKey = (KeyCode)PlayerPrefs.GetInt("Key_Right", (int)KeyCode.D);
            JumpKey = (KeyCode)PlayerPrefs.GetInt("Key_Jump", (int)KeyCode.Space);
            ReloadKey = (KeyCode)PlayerPrefs.GetInt("Key_Reload", (int)KeyCode.R);
            FragGrenadeKey = (KeyCode)PlayerPrefs.GetInt("Key_Frag", (int)KeyCode.G);
            SmokeGrenadeKey = (KeyCode)PlayerPrefs.GetInt("Key_Smoke", (int)KeyCode.H);
            PrimaryFireKey = (KeyCode)PlayerPrefs.GetInt("Key_Fire1", (int)KeyCode.Mouse0);
            LaserFireKey = (KeyCode)PlayerPrefs.GetInt("Key_Fire2", (int)KeyCode.Mouse1);

            MasterVolume = PlayerPrefs.GetFloat("Audio_Master", 1.0f);
            SfxVolume = PlayerPrefs.GetFloat("Audio_SFX", 1.0f);
            MusicVolume = PlayerPrefs.GetFloat("Audio_Music", 0.75f);
            MouseSensitivity = PlayerPrefs.GetFloat("Gameplay_MouseSens", 2.0f);
            ScreenShakeEnabled = PlayerPrefs.GetInt("Gameplay_ScreenShake", 1) == 1;
            CrosshairScale = PlayerPrefs.GetFloat("Gameplay_CrosshairScale", 1.0f);
            ShowKeyHints = PlayerPrefs.GetInt("Gameplay_ShowKeyHints", 1) == 1;

            AudioListener.volume = MasterVolume;
        }

        public static void SaveSettings()
        {
            PlayerPrefs.SetInt("Key_Forward", (int)MoveForwardKey);
            PlayerPrefs.SetInt("Key_Backward", (int)MoveBackwardKey);
            PlayerPrefs.SetInt("Key_Left", (int)MoveLeftKey);
            PlayerPrefs.SetInt("Key_Right", (int)MoveRightKey);
            PlayerPrefs.SetInt("Key_Jump", (int)JumpKey);
            PlayerPrefs.SetInt("Key_Reload", (int)ReloadKey);
            PlayerPrefs.SetInt("Key_Frag", (int)FragGrenadeKey);
            PlayerPrefs.SetInt("Key_Smoke", (int)SmokeGrenadeKey);
            PlayerPrefs.SetInt("Key_Fire1", (int)PrimaryFireKey);
            PlayerPrefs.SetInt("Key_Fire2", (int)LaserFireKey);

            PlayerPrefs.SetFloat("Audio_Master", MasterVolume);
            PlayerPrefs.SetFloat("Audio_SFX", SfxVolume);
            PlayerPrefs.SetFloat("Audio_Music", MusicVolume);
            PlayerPrefs.SetFloat("Gameplay_MouseSens", MouseSensitivity);
            PlayerPrefs.SetInt("Gameplay_ScreenShake", ScreenShakeEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("Gameplay_CrosshairScale", CrosshairScale);
            PlayerPrefs.SetInt("Gameplay_ShowKeyHints", ShowKeyHints ? 1 : 0);
            PlayerPrefs.Save();

            AudioListener.volume = MasterVolume;
            OnSettingsChanged?.Invoke();
        }

        public static void ResetToDefaults()
        {
            MoveForwardKey = KeyCode.W;
            MoveBackwardKey = KeyCode.S;
            MoveLeftKey = KeyCode.A;
            MoveRightKey = KeyCode.D;
            JumpKey = KeyCode.Space;
            ReloadKey = KeyCode.R;
            FragGrenadeKey = KeyCode.G;
            SmokeGrenadeKey = KeyCode.H;
            PrimaryFireKey = KeyCode.Mouse0;
            LaserFireKey = KeyCode.Mouse1;

            MasterVolume = 1.0f;
            SfxVolume = 1.0f;
            MusicVolume = 0.75f;
            SpatialAudioDemoActive = false;
            DirectionalSoundIndicatorsEnabled = true;
            MouseSensitivity = 2.0f;
            ScreenShakeEnabled = true;
            CrosshairScale = 1.0f;
            ShowKeyHints = true;

            SaveSettings();
        }

        public static bool IsFire1Held()
        {
            if (IsSettingsOpen) return false;
            return PrimaryFireKey == KeyCode.Mouse0 ? Input.GetMouseButton(0) : Input.GetKey(PrimaryFireKey);
        }

        public static bool IsFire1Down()
        {
            if (IsSettingsOpen) return false;
            return PrimaryFireKey == KeyCode.Mouse0 ? Input.GetMouseButtonDown(0) : Input.GetKeyDown(PrimaryFireKey);
        }

        public static bool IsFire2Held()
        {
            if (IsSettingsOpen) return false;
            return LaserFireKey == KeyCode.Mouse1 ? Input.GetMouseButton(1) : Input.GetKey(LaserFireKey);
        }

        public static bool IsFire2Down()
        {
            if (IsSettingsOpen) return false;
            return LaserFireKey == KeyCode.Mouse1 ? Input.GetMouseButtonDown(1) : Input.GetKeyDown(LaserFireKey);
        }

        public static KeyCode GetKeyForAction(string action)
        {
            switch (action)
            {
                case "Forward": return MoveForwardKey;
                case "Backward": return MoveBackwardKey;
                case "Left": return MoveLeftKey;
                case "Right": return MoveRightKey;
                case "Jump": return JumpKey;
                case "Fire1": return PrimaryFireKey;
                case "Fire2": return LaserFireKey;
                case "Reload": return ReloadKey;
                case "Frag": return FragGrenadeKey;
                case "Smoke": return SmokeGrenadeKey;
                default: return KeyCode.None;
            }
        }

        public static void SetKeyForAction(string action, KeyCode key)
        {
            switch (action)
            {
                case "Forward": MoveForwardKey = key; break;
                case "Backward": MoveBackwardKey = key; break;
                case "Left": MoveLeftKey = key; break;
                case "Right": MoveRightKey = key; break;
                case "Jump": JumpKey = key; break;
                case "Fire1": PrimaryFireKey = key; break;
                case "Fire2": LaserFireKey = key; break;
                case "Reload": ReloadKey = key; break;
                case "Frag": FragGrenadeKey = key; break;
                case "Smoke": SmokeGrenadeKey = key; break;
            }
            SaveSettings();
        }

        public static string FormatKeyName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Mouse0: return "Chuột trái (M1)";
                case KeyCode.Mouse1: return "Chuột phải (M2)";
                case KeyCode.Mouse2: return "Chuột giữa (M3)";
                case KeyCode.Space: return "Phím Cách (Space)";
                case KeyCode.LeftShift: return "Shift trái";
                case KeyCode.RightShift: return "Shift phải";
                case KeyCode.LeftControl: return "Ctrl trái";
                case KeyCode.RightControl: return "Ctrl phải";
                case KeyCode.LeftAlt: return "Alt trái";
                case KeyCode.Tab: return "Tab";
                case KeyCode.Escape: return "Esc";
                default: return key.ToString();
            }
        }
    }
}
