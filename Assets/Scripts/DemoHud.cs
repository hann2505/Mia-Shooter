using UnityEngine;

namespace MiaShooter
{
    public sealed class DemoHud : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle scoreStyle;
        private GUIStyle ammoStyle;
        private WeaponController weapon;
        private GrenadeThrower grenadeThrower;

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.5f);

            GUI.color = new Color(0f, 0f, 0f, 0.62f);
            GUI.Box(new Rect(18f, 18f, 500f * scale, 154f * scale), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(34f, 28f, 400f * scale, 32f * scale), "MIA SHOOTER — SOUND & VFX LAB", titleStyle);
            GUI.Label(new Rect(34f, 62f, 470f * scale, 98f * scale),
                "WASD: di chuyển   |   SPACE: nhảy   |   Chuột: nhìn\nChuột trái: bắn / ném   |   R: nạp đạn\nG: cầm lựu đạn nổ   |   H: cầm bom khói   |   ESC: mở con trỏ", bodyStyle);

            DemoGameManager manager = DemoGameManager.Instance;
            string score = manager == null
                ? "SCORE 0000"
                : $"SCORE {manager.Score:0000}   |   TARGETS {manager.TargetsDestroyed:00}";
            GUI.Label(new Rect(Screen.width - 350f * scale, 28f, 325f * scale, 40f * scale), score, scoreStyle);

            if (weapon == null)
            {
                weapon = FindFirstObjectByType<WeaponController>();
            }

            if (grenadeThrower == null)
            {
                grenadeThrower = FindFirstObjectByType<GrenadeThrower>();
            }

            if (grenadeThrower != null && grenadeThrower.IsGrenadeEquipped)
            {
                GUI.Label(new Rect(Screen.width - 430f * scale, Screen.height - 72f * scale, 405f * scale, 46f * scale),
                    $"{grenadeThrower.SelectedGrenadeName}  —  CLICK TO THROW", ammoStyle);
            }
            else if (weapon != null)
            {
                string ammo = weapon.IsReloading
                    ? "RELOADING..."
                    : $"AMMO  {weapon.CurrentAmmo:00} / {weapon.MagazineSize:00}";
                GUI.Label(new Rect(Screen.width - 350f * scale, Screen.height - 72f * scale, 325f * scale, 46f * scale), ammo, ammoStyle);
            }

            DrawCrosshair();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null && bodyStyle != null && scoreStyle != null && ammoStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.9f, 0.94f, 1f) }
            };
            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.78f, 0.18f) }
            };
            ammoStyle = new GUIStyle(scoreStyle)
            {
                fontSize = 26,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };
        }

        private static void DrawCrosshair()
        {
            float x = Screen.width * 0.5f;
            float y = Screen.height * 0.5f;
            Color previous = GUI.color;
            GUI.color = new Color(0.2f, 0.95f, 1f, 0.95f);
            GUI.DrawTexture(new Rect(x - 10f, y - 1f, 20f, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x - 1f, y - 10f, 2f, 20f), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
