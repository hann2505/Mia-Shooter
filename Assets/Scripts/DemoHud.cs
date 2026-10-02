using UnityEngine;

namespace MiaShooter
{
    public sealed class DemoHud : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle scoreStyle;
        private GUIStyle ammoStyle;
        private GUIStyle reticleStyle;
        private WeaponController weapon;
        private GrenadeThrower grenadeThrower;
        private Camera mainCamera;
        private float lastHitMarkerTime = -10f;

        private void OnEnable()
        {
            WeaponController.OnHitTarget += HandleTargetHit;
        }

        private void OnDisable()
        {
            WeaponController.OnHitTarget -= HandleTargetHit;
        }

        private void Update()
        {
            if (weapon == null)
            {
                weapon = FindFirstObjectByType<WeaponController>();
            }

            if (grenadeThrower == null)
            {
                grenadeThrower = FindFirstObjectByType<GrenadeThrower>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void HandleTargetHit()
        {
            lastHitMarkerTime = Time.time;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.5f);

            GUI.color = new Color(0f, 0f, 0f, 0.62f);
            GUI.Box(new Rect(18f, 18f, 500f * scale, 154f * scale), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(34f, 28f, 400f * scale, 32f * scale), "MIA SHOOTER — SOUND & VFX LAB", titleStyle);
            GUI.Label(new Rect(34f, 62f, 470f * scale, 98f * scale),
                "WASD: di chuyển   |   SPACE: nhảy   |   Chuột: nhìn\nChuột trái: bắn / ném   |   Chuột phải: laser   |   R: nạp đạn\nG: cầm lựu đạn nổ   |   H: cầm bom khói   |   ESC: mở con trỏ", bodyStyle);

            DemoGameManager manager = DemoGameManager.Instance;
            string score = manager == null
                ? "SCORE 0000"
                : $"SCORE {manager.Score:0000}   |   TARGETS {manager.TargetsDestroyed:00}";
            GUI.Label(new Rect(Screen.width - 350f * scale, 28f, 325f * scale, 40f * scale), score, scoreStyle);

            if (grenadeThrower != null && grenadeThrower.IsGrenadeEquipped)
            {
                GUI.Label(new Rect(Screen.width - 430f * scale, Screen.height - 72f * scale, 405f * scale, 46f * scale),
                    $"{grenadeThrower.SelectedGrenadeName}  —  CLICK TO THROW", ammoStyle);
            }
            else if (weapon != null)
            {
                string ammo;
                if (weapon.IsFiringLaser)
                {
                    ammo = $"LASER  {weapon.LaserEnergy * 100f:00}%";
                }
                else if (weapon.IsLaserReady)
                {
                    ammo = "LASER READY — HOLD RIGHT MOUSE";
                }
                else
                {
                    ammo = weapon.IsReloading
                        ? "RELOADING..."
                        : $"AMMO  {weapon.CurrentAmmo:00} / {weapon.MagazineSize:00}   |   LASER {weapon.LaserEnergy * 100f:00}%";
                }
                GUI.Label(new Rect(Screen.width - 475f * scale, Screen.height - 72f * scale, 450f * scale, 46f * scale), ammo, ammoStyle);
            }

            DrawCrosshair(scale);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null && bodyStyle != null && scoreStyle != null && ammoStyle != null && reticleStyle != null)
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
            reticleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f, 0.85f) }
            };
        }

        private void DrawCrosshair(float scale)
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;

            bool isAimingAtTarget = CheckTargetAim();
            bool isLaserFiring = weapon != null && weapon.IsFiringLaser;
            bool isLaserReady = weapon != null && weapon.IsLaserReady;
            bool isGrenade = grenadeThrower != null && grenadeThrower.IsGrenadeEquipped;

            Color baseCyan = new Color(0.18f, 0.92f, 1f, 0.95f);
            Color hostileColor = new Color(1f, 0.32f, 0.22f, 0.98f);
            Color primaryColor = isAimingAtTarget ? hostileColor : baseCyan;
            Color shadowColor = new Color(0f, 0f, 0f, 0.75f);

            // 1. Center Precision Dot
            float dotSize = (isLaserFiring ? 4f : 3f) * scale;
            Color dotColor = isLaserFiring ? Color.white : primaryColor;
            DrawRectWithOutline(new Rect(cx - dotSize * 0.5f, cy - dotSize * 0.5f, dotSize, dotSize), dotColor, shadowColor, 1f);

            // 2. Directional Reticle Bars
            float gap = (isLaserFiring ? 12f : (isGrenade ? 10f : (isAimingAtTarget ? 6f : 7f))) * scale;
            float barLen = (isLaserFiring ? 14f : (isGrenade ? 8f : 9f)) * scale;
            float barThick = 2f * scale;

            // Left, Right, Top, Bottom bars with shadow
            DrawRectWithOutline(new Rect(cx - gap - barLen, cy - barThick * 0.5f, barLen, barThick), primaryColor, shadowColor, 1f);
            DrawRectWithOutline(new Rect(cx + gap, cy - barThick * 0.5f, barLen, barThick), primaryColor, shadowColor, 1f);
            DrawRectWithOutline(new Rect(cx - barThick * 0.5f, cy - gap - barLen, barThick, barLen), primaryColor, shadowColor, 1f);
            DrawRectWithOutline(new Rect(cx - barThick * 0.5f, cy + gap, barThick, barLen), primaryColor, shadowColor, 1f);

            // 3. Sci-Fi Corner Aim Brackets
            float bracketDist = (isLaserFiring ? 26f : (isAimingAtTarget ? 17f : 21f)) * scale;
            float bracketLen = 6f * scale;
            float bracketThick = 1.5f * scale;
            Color bracketColor = isAimingAtTarget ? hostileColor : new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.7f);

            // Top-Left
            DrawRect(new Rect(cx - bracketDist, cy - bracketDist, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx - bracketDist, cy - bracketDist, bracketThick, bracketLen), bracketColor);
            // Top-Right
            DrawRect(new Rect(cx + bracketDist - bracketLen, cy - bracketDist, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx + bracketDist - bracketThick, cy - bracketDist, bracketThick, bracketLen), bracketColor);
            // Bottom-Left
            DrawRect(new Rect(cx - bracketDist, cy + bracketDist - bracketThick, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx - bracketDist, cy + bracketDist - bracketLen, bracketThick, bracketLen), bracketColor);
            // Bottom-Right
            DrawRect(new Rect(cx + bracketDist - bracketLen, cy + bracketDist - bracketThick, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx + bracketDist - bracketThick, cy + bracketDist - bracketLen, bracketThick, bracketLen), bracketColor);

            // 4. Laser Aura Reticle & Indicators
            if (isLaserFiring)
            {
                // Dynamic expanding aura ring & vibration
                float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 24f);
                float auraDist = 32f * scale * pulse;
                float auraSize = 8f * scale;
                Color auraColor = new Color(0.35f, 0.98f, 1f, 0.85f);

                // Aura diamond points
                DrawRect(new Rect(cx - auraSize * 0.5f, cy - auraDist - 2f, auraSize, 2f), auraColor);
                DrawRect(new Rect(cx - auraSize * 0.5f, cy + auraDist, auraSize, 2f), auraColor);
                DrawRect(new Rect(cx - auraDist - 2f, cy - auraSize * 0.5f, 2f, auraSize), auraColor);
                DrawRect(new Rect(cx + auraDist, cy - auraSize * 0.5f, 2f, auraSize), auraColor);
            }
            else if (isLaserReady)
            {
                // Pulsing ready aura brackets & text
                float breathe = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
                float auraDist = (27f + 2f * breathe) * scale;
                Color readyAuraColor = new Color(0.2f, 0.95f, 1f, 0.45f + 0.5f * breathe);

                // Subtle diamond pips
                DrawRect(new Rect(cx - 2f, cy - auraDist, 4f, 1.5f), readyAuraColor);
                DrawRect(new Rect(cx - 2f, cy + auraDist - 1.5f, 4f, 1.5f), readyAuraColor);
                DrawRect(new Rect(cx - auraDist, cy - 2f, 1.5f, 4f), readyAuraColor);
                DrawRect(new Rect(cx + auraDist - 1.5f, cy - 2f, 1.5f, 4f), readyAuraColor);

                reticleStyle.normal.textColor = readyAuraColor;
                GUI.Label(new Rect(cx - 50f, cy + auraDist + 4f, 100f, 18f), "⚡ READY", reticleStyle);
            }

            // 5. Reactive Hitmarker (X-ticks on successful bullet/laser impact)
            float hitElapsed = Time.time - lastHitMarkerTime;
            if (hitElapsed >= 0f && hitElapsed < 0.18f)
            {
                float hitAlpha = 1f - (hitElapsed / 0.18f);
                Color hitColor = new Color(1f, 0.25f, 0.25f, hitAlpha);
                float hitDist = 9f * scale;
                float hitLen = 5f * scale;
                float hitThick = 2f * scale;

                // 4 diagonal tick marks
                DrawRect(new Rect(cx - hitDist, cy - hitDist, hitLen, hitThick), hitColor);
                DrawRect(new Rect(cx - hitDist, cy - hitDist, hitThick, hitLen), hitColor);

                DrawRect(new Rect(cx + hitDist - hitLen, cy - hitDist, hitLen, hitThick), hitColor);
                DrawRect(new Rect(cx + hitDist - hitThick, cy - hitDist, hitThick, hitLen), hitColor);

                DrawRect(new Rect(cx - hitDist, cy + hitDist - hitThick, hitLen, hitThick), hitColor);
                DrawRect(new Rect(cx - hitDist, cy + hitDist - hitLen, hitThick, hitLen), hitColor);

                DrawRect(new Rect(cx + hitDist - hitLen, cy + hitDist - hitThick, hitLen, hitThick), hitColor);
                DrawRect(new Rect(cx + hitDist - hitThick, cy + hitDist - hitLen, hitThick, hitLen), hitColor);
            }
        }

        private bool CheckTargetAim()
        {
            Camera cam = mainCamera != null ? mainCamera : Camera.main;
            if (cam == null) return false;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 80f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.GetComponentInParent<ShootableTarget>() != null;
            }
            return false;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prev;
        }

        private static void DrawRectWithOutline(Rect rect, Color fill, Color outline, float border = 1f)
        {
            DrawRect(new Rect(rect.x - border, rect.y - border, rect.width + border * 2f, rect.height + border * 2f), outline);
            DrawRect(rect, fill);
        }
    }
}
