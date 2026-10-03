using System.Collections.Generic;
using UnityEngine;

namespace MiaShooter
{
    public sealed class DemoHud : MonoBehaviour
    {
        private WeaponController weapon;
        private GrenadeThrower grenadeThrower;
        private Camera mainCamera;

        // Hitmarker & Kill tracking
        private float lastHitMarkerTime = -10f;
        private float lastEliminationTime = -10f;
        private int lastEliminatedScore = 100;
        private int killCombo = 0;
        private float lastKillTime = -10f;

        // Cached GUIStyles
        private GUIStyle headerTitleStyle;
        private GUIStyle headerSubStyle;
        private GUIStyle scoreLabelStyle;
        private GUIStyle scoreValueStyle;
        private GUIStyle ammoLargeStyle;
        private GUIStyle ammoSubStyle;
        private GUIStyle weaponNameStyle;
        private GUIStyle weaponModeStyle;
        private GUIStyle reticleStyle;
        private GUIStyle killBannerTitleStyle;
        private GUIStyle killBannerScoreStyle;
        private GUIStyle modalTitleStyle;
        private GUIStyle tabButtonStyle;
        private GUIStyle keyLabelStyle;
        private GUIStyle keyButtonStyle;
        private GUIStyle sliderLabelStyle;
        private GUIStyle sliderValueStyle;
        private GUIStyle settingToggleStyle;
        private GUIStyle actionButtonStyle;

        private struct SoundCue
        {
            public Vector3 worldPosition;
            public float timestamp;
            public float volume;
            public Color color;
        }

        private readonly List<SoundCue> activeSoundCues = new List<SoundCue>();

        private void OnEnable()
        {
            WeaponController.OnHitTarget += HandleTargetHit;
            DemoGameManager.TargetEliminated += HandleTargetEliminated;
            SpatialAudioUtility.OnSpatialSoundPlayed += HandleSpatialSound;
        }

        private void OnDisable()
        {
            WeaponController.OnHitTarget -= HandleTargetHit;
            DemoGameManager.TargetEliminated -= HandleTargetEliminated;
            SpatialAudioUtility.OnSpatialSoundPlayed -= HandleSpatialSound;
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

            activeSoundCues.RemoveAll(c => Time.time - c.timestamp > 0.75f);
        }

        private void HandleSpatialSound(Vector3 position, float volume, Color color)
        {
            activeSoundCues.Add(new SoundCue
            {
                worldPosition = position,
                timestamp = Time.time,
                volume = volume,
                color = color
            });
        }

        private void HandleTargetHit()
        {
            lastHitMarkerTime = Time.time;
        }

        private void HandleTargetEliminated(int score, Vector3 position, Color color)
        {
            lastEliminationTime = Time.time;
            lastEliminatedScore = score;
            if (Time.time - lastKillTime < 3.8f)
            {
                killCombo++;
            }
            else
            {
                killCombo = 1;
            }
            lastKillTime = Time.time;
        }

        private void OnGUI()
        {
            EnsureStyles();
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.4f);

            // Handle Key Rebinding Input Event if currently listening
            HandleRebindingEvents();

            // 1. Sleek Top Status Bar
            DrawTopStatusBar(scale);

            // 2. Bottom-Left Tactical Vitals & Keybind Chips
            DrawBottomLeftVitals(scale);

            // 3. Bottom-Right High-Tech Weapon Module
            DrawBottomRightWeaponModule(scale);

            // 4. Kill Banner Feedback
            DrawKillBanner(scale);

            // 5. Central Reactive Crosshair & Hitmarker
            DrawCrosshair(scale);

            // 5b. 3D Spatial Audio Probe Telemetry HUD
            DrawSpatialAudioProbeHUD(scale);

            // 6. Interactive Settings Modal (when open)
            if (GameSettings.IsSettingsOpen)
            {
                DrawSettingsModal(scale);
            }
        }

        #region HUD Sections

        private void DrawTopStatusBar(float scale)
        {
            // Left Holographic Badge
            float leftW = 340f * scale;
            float leftH = 64f * scale;
            Rect leftRect = new Rect(24f, 20f, leftW, leftH);
            DrawTechCard(leftRect, new Color(0.02f, 0.04f, 0.07f, 0.78f), new Color(0.18f, 0.92f, 1f, 0.85f), scale);

            GUI.Label(new Rect(leftRect.x + 16f * scale, leftRect.y + 8f * scale, leftW - 32f * scale, 24f * scale),
                "✦ MIA // LABS TACTICAL SUITE", headerTitleStyle);
            GUI.Label(new Rect(leftRect.x + 16f * scale, leftRect.y + 32f * scale, leftW - 32f * scale, 22f * scale),
                "FIRING RANGE SIMULATION  •  SYSTEM ACTIVE", headerSubStyle);

            // Right Tactical Score Card & Settings Button
            float rightW = 420f * scale;
            float rightH = 64f * scale;
            Rect rightRect = new Rect(Screen.width - rightW - 24f, 20f, rightW, rightH);
            DrawTechCard(rightRect, new Color(0.02f, 0.04f, 0.07f, 0.78f), new Color(0.18f, 0.92f, 1f, 0.85f), scale);

            DemoGameManager manager = DemoGameManager.Instance;
            int score = manager != null ? manager.Score : 0;
            int targets = manager != null ? manager.TargetsDestroyed : 0;

            float scoreColW = 140f * scale;
            GUI.Label(new Rect(rightRect.x + 14f * scale, rightRect.y + 8f * scale, scoreColW, 20f * scale), "SCORE", scoreLabelStyle);
            GUI.Label(new Rect(rightRect.x + 14f * scale, rightRect.y + 26f * scale, scoreColW, 30f * scale), $"{score:0000}", scoreValueStyle);

            float targetColW = 120f * scale;
            GUI.Label(new Rect(rightRect.x + 160f * scale, rightRect.y + 8f * scale, targetColW, 20f * scale), "TARGETS", scoreLabelStyle);
            scoreValueStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);
            GUI.Label(new Rect(rightRect.x + 160f * scale, rightRect.y + 26f * scale, targetColW, 30f * scale), $"{targets:00} / 06", scoreValueStyle);
            scoreValueStyle.normal.textColor = new Color(1f, 0.82f, 0.18f); // restore gold

            // Settings Button
            float btnW = 110f * scale;
            float btnH = 40f * scale;
            Rect btnRect = new Rect(rightRect.x + rightW - btnW - 12f * scale, rightRect.y + 12f * scale, btnW, btnH);
            string btnText = GameSettings.IsSettingsOpen ? "✕ ĐÓNG" : "⚙ CÀI ĐẶT";
            if (GUI.Button(btnRect, btnText, actionButtonStyle))
            {
                GameSettings.IsSettingsOpen = !GameSettings.IsSettingsOpen;
                Cursor.lockState = GameSettings.IsSettingsOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = GameSettings.IsSettingsOpen;
            }
        }

        private void DrawBottomLeftVitals(float scale)
        {
            float w = 310f * scale;
            float h = (GameSettings.ShowKeyHints ? 154f : 88f) * scale;
            Rect rect = new Rect(24f, Screen.height - h - 24f, w, h);
            DrawTechCard(rect, new Color(0.02f, 0.04f, 0.07f, 0.82f), new Color(0.18f, 0.92f, 1f, 0.75f), scale);

            // Shield Bar
            GUI.Label(new Rect(rect.x + 14f * scale, rect.y + 10f * scale, 70f * scale, 18f * scale), "SHIELD", headerSubStyle);
            DrawSegmentedBar(new Rect(rect.x + 85f * scale, rect.y + 12f * scale, 205f * scale, 12f * scale), 1.0f, new Color(0.2f, 0.95f, 1f), 10, scale);

            // Armor Bar
            GUI.Label(new Rect(rect.x + 14f * scale, rect.y + 30f * scale, 70f * scale, 18f * scale), "ARMOR", headerSubStyle);
            DrawSegmentedBar(new Rect(rect.x + 85f * scale, rect.y + 32f * scale, 205f * scale, 12f * scale), 1.0f, new Color(1f, 0.55f, 0.15f), 10, scale);

            // Stance / Status badge
            GUI.Label(new Rect(rect.x + 14f * scale, rect.y + 54f * scale, w - 28f * scale, 20f * scale),
                "● MOBILITY 100%   |   STANCE: TACTICAL READY", headerSubStyle);

            // Quick Keybind Chips (if enabled)
            if (GameSettings.ShowKeyHints)
            {
                float dividerY = rect.y + 78f * scale;
                DrawRect(new Rect(rect.x + 12f * scale, dividerY, w - 24f * scale, 1f), new Color(0.18f, 0.92f, 1f, 0.25f));

                GUI.Label(new Rect(rect.x + 14f * scale, dividerY + 6f * scale, w - 28f * scale, 60f * scale),
                    $"[WASD] Di chuyển   |   [SPACE] Nhảy\n[{GameSettings.FormatKeyName(GameSettings.PrimaryFireKey)}] Bắn   |   [{GameSettings.FormatKeyName(GameSettings.LaserFireKey)}] Laser   |   [{GameSettings.ReloadKey}] Nạp\n[G] Bom nổ   |   [H] Bom khói   |   [ESC] Cài đặt",
                    headerSubStyle);
            }
        }

        private void DrawBottomRightWeaponModule(float scale)
        {
            float w = 390f * scale;
            float h = 168f * scale;
            Rect rect = new Rect(Screen.width - w - 24f, Screen.height - h - 24f, w, h);
            DrawTechCard(rect, new Color(0.02f, 0.04f, 0.07f, 0.85f), new Color(0.18f, 0.92f, 1f, 0.9f), scale);

            // Weapon Identification & Mode Badge
            bool isGrenadeEquipped = grenadeThrower != null && grenadeThrower.IsGrenadeEquipped;
            string weaponTitle = isGrenadeEquipped ? $"{grenadeThrower.SelectedGrenadeName.ToUpper()} // ORDNANCE" : "ION BLASTER // MK-IV";
            string modeTitle = isGrenadeEquipped ? "READY TO THROW [M1]" : "SEMI-AUTO [PLASMA]";

            GUI.Label(new Rect(rect.x + 16f * scale, rect.y + 10f * scale, 230f * scale, 22f * scale), weaponTitle, weaponNameStyle);

            Color modeCol = isGrenadeEquipped ? new Color(1f, 0.65f, 0.15f) : new Color(0.2f, 0.95f, 1f);
            weaponModeStyle.normal.textColor = modeCol;
            GUI.Label(new Rect(rect.x + 16f * scale, rect.y + 32f * scale, 200f * scale, 18f * scale), modeTitle, weaponModeStyle);

            if (weapon != null && !isGrenadeEquipped)
            {
                // Large Digital Ammo Readout
                if (weapon.IsReloading)
                {
                    ammoLargeStyle.normal.textColor = new Color(1f, 0.65f, 0.15f);
                    GUI.Label(new Rect(rect.x + w - 220f * scale, rect.y + 14f * scale, 200f * scale, 45f * scale), "RELOAD", ammoLargeStyle);
                }
                else
                {
                    ammoLargeStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);
                    GUI.Label(new Rect(rect.x + w - 195f * scale, rect.y + 10f * scale, 100f * scale, 45f * scale), $"{weapon.CurrentAmmo:00}", ammoLargeStyle);
                    GUI.Label(new Rect(rect.x + w - 95f * scale, rect.y + 24f * scale, 80f * scale, 30f * scale), $"/ {weapon.MagazineSize:00}", ammoSubStyle);
                }

                // 12 Tactical Bullet Cartridge Pips
                float pipStartX = rect.x + 16f * scale;
                float pipY = rect.y + 56f * scale;
                float pipW = 18f * scale;
                float pipH = 8f * scale;
                float pipGap = 5f * scale;

                for (int i = 0; i < weapon.MagazineSize; i++)
                {
                    bool isLoaded = i < weapon.CurrentAmmo;
                    Color pipColor = isLoaded ? new Color(0.2f, 0.95f, 1f, 0.95f) : new Color(0.25f, 0.28f, 0.32f, 0.5f);
                    Color pipBorder = isLoaded ? new Color(0.1f, 0.55f, 0.8f, 0.9f) : new Color(0.12f, 0.14f, 0.16f, 0.8f);
                    DrawRectWithOutline(new Rect(pipStartX + i * (pipW + pipGap), pipY, pipW, pipH), pipColor, pipBorder, 1f);
                }

                // Laser Energy Gauge
                float laserY = rect.y + 76f * scale;
                float laserBarW = w - 32f * scale;
                float laserBarH = 14f * scale;

                string laserStatus = weapon.IsFiringLaser ? "⚡ LASER FIRING" : (weapon.IsLaserReady ? "⚡ READY — HOLD M2" : $"LASER CHARGE {weapon.LaserEnergy * 100f:0}%");
                Color laserTextCol = weapon.IsLaserReady ? new Color(1f, 0.65f, 0.15f) : new Color(0.85f, 0.88f, 0.92f);
                GUI.Label(new Rect(rect.x + 16f * scale, laserY, 200f * scale, 18f * scale), laserStatus, headerSubStyle);

                // Laser Gauge Fill (Hot Red -> Orange)
                DrawEnergyGradientBar(new Rect(rect.x + 16f * scale, laserY + 18f * scale, laserBarW, laserBarH), weapon.LaserEnergy, scale);
            }
            else if (isGrenadeEquipped)
            {
                ammoLargeStyle.normal.textColor = new Color(1f, 0.65f, 0.15f);
                GUI.Label(new Rect(rect.x + w - 180f * scale, rect.y + 14f * scale, 160f * scale, 45f * scale), "ACTIVE", ammoLargeStyle);

                GUI.Label(new Rect(rect.x + 16f * scale, rect.y + 64f * scale, w - 32f * scale, 32f * scale),
                    "CLICK CHUỘT TRÁI ĐỂ NÉM LỰU ĐẠN\nNhấn lại phím bom để cất và đổi súng", headerSubStyle);
            }

            // Ordnance Quick Chips at Bottom
            float ordY = rect.y + h - 38f * scale;
            float ordW = 165f * scale;
            float ordH = 26f * scale;

            bool isFragActive = isGrenadeEquipped && grenadeThrower.SelectedGrenadeName.Contains("Frag");
            bool isSmokeActive = isGrenadeEquipped && grenadeThrower.SelectedGrenadeName.Contains("Smoke");

            DrawOrdnanceChip(new Rect(rect.x + 16f * scale, ordY, ordW, ordH), $"[{GameSettings.FragGrenadeKey}] BOM NỔ HE", isFragActive, scale);
            DrawOrdnanceChip(new Rect(rect.x + 200f * scale, ordY, ordW, ordH), $"[{GameSettings.SmokeGrenadeKey}] BOM KHÓI", isSmokeActive, scale);
        }

        private void DrawOrdnanceChip(Rect rect, string label, bool isActive, float scale)
        {
            Color bg = isActive ? new Color(1f, 0.45f, 0.08f, 0.35f) : new Color(0.04f, 0.07f, 0.11f, 0.7f);
            Color border = isActive ? new Color(1f, 0.65f, 0.15f, 0.95f) : new Color(0.18f, 0.92f, 1f, 0.45f);
            DrawRectWithOutline(rect, bg, border, isActive ? 1.5f : 1f);

            headerSubStyle.normal.textColor = isActive ? new Color(1f, 0.92f, 0.65f) : new Color(0.75f, 0.85f, 0.95f);
            GUI.Label(new Rect(rect.x + 6f * scale, rect.y + 4f * scale, rect.width - 12f * scale, rect.height), label, headerSubStyle);
        }

        #endregion

        #region Crosshair & Kill Banner

        private void DrawKillBanner(float scale)
        {
            float elimElapsed = Time.time - lastEliminationTime;
            if (elimElapsed < 0f || elimElapsed > 1.6f)
            {
                return;
            }

            float progress = elimElapsed / 1.6f;
            float alpha = progress < 0.12f ? (progress / 0.12f) : (progress > 0.70f ? (1f - (progress - 0.70f) / 0.30f) : 1f);
            float pop = progress < 0.15f ? Mathf.Lerp(1.2f, 1.0f, progress / 0.15f) : 1.0f;

            float bannerW = 360f * scale * pop;
            float bannerH = 58f * scale * pop;
            float bx = Screen.width * 0.5f - bannerW * 0.5f;
            float by = Screen.height * 0.60f;

            Color bgBox = new Color(0.02f, 0.04f, 0.07f, 0.88f * alpha);
            Color borderCyan = new Color(0.2f, 0.95f, 1f, 0.92f * alpha);
            Color shadow = new Color(0f, 0f, 0f, 0.6f * alpha);

            DrawRect(new Rect(bx + 2f, by + 2f, bannerW, bannerH), shadow);
            DrawRectWithOutline(new Rect(bx, by, bannerW, bannerH), bgBox, borderCyan, 1.5f * scale);

            float tabLen = 10f * scale;
            float tabThick = 2.5f * scale;
            DrawRect(new Rect(bx, by, tabLen, tabThick), borderCyan);
            DrawRect(new Rect(bx, by, tabThick, tabLen), borderCyan);
            DrawRect(new Rect(bx + bannerW - tabLen, by, tabLen, tabThick), borderCyan);
            DrawRect(new Rect(bx + bannerW - tabThick, by, tabThick, tabLen), borderCyan);
            DrawRect(new Rect(bx, by + bannerH - tabThick, tabLen, tabThick), borderCyan);
            DrawRect(new Rect(bx, by + bannerH - tabLen, tabThick, tabLen), borderCyan);
            DrawRect(new Rect(bx + bannerW - tabLen, by + bannerH - tabThick, tabLen, tabThick), borderCyan);
            DrawRect(new Rect(bx + bannerW - tabThick, by + bannerH - tabLen, tabThick, tabLen), borderCyan);

            killBannerTitleStyle.normal.textColor = new Color(1f, 0.96f, 0.9f, alpha);
            killBannerTitleStyle.fontSize = Mathf.RoundToInt(15 * scale * pop);

            Color goldScore = new Color(1f, 0.82f, 0.2f, alpha);
            killBannerScoreStyle.normal.textColor = goldScore;
            killBannerScoreStyle.fontSize = Mathf.RoundToInt(18 * scale * pop);

            string comboText = killCombo > 1 ? $"   ✦   COMBO x{killCombo}!" : "";
            GUI.Label(new Rect(bx, by + 5f * scale, bannerW, 22f * scale), "✦ TARGET ELIMINATED ✦", killBannerTitleStyle);
            GUI.Label(new Rect(bx, by + 27f * scale, bannerW, 26f * scale), $"+{lastEliminatedScore} PTS{comboText}", killBannerScoreStyle);
        }

        private void DrawCrosshair(float scale)
        {
            float crosshairScale = scale * GameSettings.CrosshairScale;
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
            float dotSize = (isLaserFiring ? 4f : 3f) * crosshairScale;
            Color dotColor = isLaserFiring ? Color.white : primaryColor;
            DrawRectWithOutline(new Rect(cx - dotSize * 0.5f, cy - dotSize * 0.5f, dotSize, dotSize), dotColor, shadowColor, 1f);

            // 2. Directional Reticle Bars
            float gap = (isLaserFiring ? 12f : (isGrenade ? 10f : (isAimingAtTarget ? 6f : 7f))) * crosshairScale;
            float barLen = (isLaserFiring ? 14f : (isGrenade ? 8f : 9f)) * crosshairScale;
            float barThick = 2f * crosshairScale;

            DrawRectWithOutline(new Rect(cx - gap - barLen, cy - barThick * 0.5f, barLen, barThick), primaryColor, shadowColor, 1f);
            DrawRectWithOutline(new Rect(cx + gap, cy - barThick * 0.5f, barLen, barThick), primaryColor, shadowColor, 1f);
            DrawRectWithOutline(new Rect(cx - barThick * 0.5f, cy - gap - barLen, barThick, barLen), primaryColor, shadowColor, 1f);
            DrawRectWithOutline(new Rect(cx - barThick * 0.5f, cy + gap, barThick, barLen), primaryColor, shadowColor, 1f);

            // 3. Sci-Fi Corner Aim Brackets
            float bracketDist = (isLaserFiring ? 26f : (isAimingAtTarget ? 17f : 21f)) * crosshairScale;
            float bracketLen = 6f * crosshairScale;
            float bracketThick = 1.5f * crosshairScale;
            Color bracketColor = isAimingAtTarget ? hostileColor : new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.7f);

            DrawRect(new Rect(cx - bracketDist, cy - bracketDist, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx - bracketDist, cy - bracketDist, bracketThick, bracketLen), bracketColor);
            DrawRect(new Rect(cx + bracketDist - bracketLen, cy - bracketDist, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx + bracketDist - bracketThick, cy - bracketDist, bracketThick, bracketLen), bracketColor);
            DrawRect(new Rect(cx - bracketDist, cy + bracketDist - bracketThick, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx - bracketDist, cy + bracketDist - bracketLen, bracketThick, bracketLen), bracketColor);
            DrawRect(new Rect(cx + bracketDist - bracketLen, cy + bracketDist - bracketThick, bracketLen, bracketThick), bracketColor);
            DrawRect(new Rect(cx + bracketDist - bracketThick, cy + bracketDist - bracketLen, bracketThick, bracketLen), bracketColor);

            // 4. Laser Aura Reticle & Indicators
            if (isLaserFiring)
            {
                float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 24f);
                float auraDist = 32f * crosshairScale * pulse;
                float auraSize = 8f * crosshairScale;
                Color auraColor = new Color(1f, 0.38f, 0.05f, 0.90f);

                DrawRect(new Rect(cx - auraSize * 0.5f, cy - auraDist - 2f, auraSize, 2f), auraColor);
                DrawRect(new Rect(cx - auraSize * 0.5f, cy + auraDist, auraSize, 2f), auraColor);
                DrawRect(new Rect(cx - auraDist - 2f, cy - auraSize * 0.5f, 2f, auraSize), auraColor);
                DrawRect(new Rect(cx + auraDist, cy - auraSize * 0.5f, 2f, auraSize), auraColor);
            }
            else if (isLaserReady)
            {
                float breathe = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
                float auraDist = (27f + 2f * breathe) * crosshairScale;
                Color readyAuraColor = new Color(1f, 0.45f, 0.08f, 0.55f + 0.45f * breathe);

                DrawRect(new Rect(cx - 2f, cy - auraDist, 4f, 1.5f), readyAuraColor);
                DrawRect(new Rect(cx - 2f, cy + auraDist - 1.5f, 4f, 1.5f), readyAuraColor);
                DrawRect(new Rect(cx - auraDist, cy - 2f, 1.5f, 4f), readyAuraColor);
                DrawRect(new Rect(cx + auraDist - 1.5f, cy - 2f, 1.5f, 4f), readyAuraColor);

                reticleStyle.normal.textColor = readyAuraColor;
                GUI.Label(new Rect(cx - 50f, cy + auraDist + 4f, 100f, 18f), "⚡ READY", reticleStyle);
            }

            // 5. Reactive Kill Hitmarker & Normal Hitmarker
            float elimElapsed = Time.time - lastEliminationTime;
            if (elimElapsed >= 0f && elimElapsed < 0.35f)
            {
                float killAlpha = 1f - (elimElapsed / 0.35f);
                float expand = Mathf.Lerp(11f, 22f, elimElapsed / 0.35f) * crosshairScale;
                float kLen = 7f * crosshairScale;
                float kThick = 2.5f * crosshairScale;
                Color killColor = new Color(1f, 0.22f, 0.15f, killAlpha);
                Color goldPip = new Color(1f, 0.82f, 0.20f, killAlpha * 0.95f);

                DrawRect(new Rect(cx - expand, cy - expand, kLen, kThick), killColor);
                DrawRect(new Rect(cx - expand, cy - expand, kThick, kLen), killColor);
                DrawRect(new Rect(cx + expand - kLen, cy - expand, kLen, kThick), killColor);
                DrawRect(new Rect(cx + expand - kThick, cy - expand, kThick, kLen), killColor);
                DrawRect(new Rect(cx - expand, cy + expand - kThick, kLen, kThick), killColor);
                DrawRect(new Rect(cx - expand, cy + expand - kLen, kThick, kLen), killColor);
                DrawRect(new Rect(cx + expand - kLen, cy + expand - kThick, kLen, kThick), killColor);
                DrawRect(new Rect(cx + expand - kThick, cy + expand - kLen, kThick, kLen), killColor);

                float cardDist = expand * 0.72f;
                DrawRect(new Rect(cx - 2f * crosshairScale, cy - cardDist - 3f * crosshairScale, 4f * crosshairScale, 3f * crosshairScale), goldPip);
                DrawRect(new Rect(cx - 2f * crosshairScale, cy + cardDist, 4f * crosshairScale, 3f * crosshairScale), goldPip);
                DrawRect(new Rect(cx - cardDist - 3f * crosshairScale, cy - 2f * crosshairScale, 3f * crosshairScale, 4f * crosshairScale), goldPip);
                DrawRect(new Rect(cx + cardDist, cy - 2f * crosshairScale, 3f * crosshairScale, 4f * crosshairScale), goldPip);
            }
            else
            {
                float hitElapsed = Time.time - lastHitMarkerTime;
                if (hitElapsed >= 0f && hitElapsed < 0.18f)
                {
                    float hitAlpha = 1f - (hitElapsed / 0.18f);
                    Color hitColor = new Color(1f, 0.25f, 0.25f, hitAlpha);
                    float hitDist = 9f * crosshairScale;
                    float hitLen = 5f * crosshairScale;
                    float hitThick = 2f * crosshairScale;

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

            // 6. Directional 3D Audio Indicators around Crosshair
            if (GameSettings.DirectionalSoundIndicatorsEnabled && activeSoundCues.Count > 0 && mainCamera != null)
            {
                float cueRadius = (68f + 6f * Mathf.Sin(Time.time * 8f)) * crosshairScale;
                for (int i = 0; i < activeSoundCues.Count; i++)
                {
                    SoundCue cue = activeSoundCues[i];
                    float elapsed = Time.time - cue.timestamp;
                    if (elapsed > 0.75f) continue;
                    float alpha = (1f - (elapsed / 0.75f)) * Mathf.Clamp01(cue.volume);

                    Vector3 toCue = cue.worldPosition - mainCamera.transform.position;
                    Vector3 localDir = mainCamera.transform.InverseTransformDirection(toCue.normalized);
                    float angleRad = Mathf.Atan2(localDir.x, localDir.z); // -pi to +pi

                    float screenX = cx + Mathf.Sin(angleRad) * cueRadius;
                    float screenY = cy - Mathf.Cos(angleRad) * cueRadius;

                    Color cueCol = new Color(cue.color.r, cue.color.g, cue.color.b, alpha * 0.95f);
                    float arcW = 10f * scale;
                    float arcH = 4f * scale;
                    DrawRectWithOutline(new Rect(screenX - arcW * 0.5f, screenY - arcH * 0.5f, arcW, arcH), cueCol, new Color(0f, 0f, 0f, alpha), 1f);
                }
            }
        }

        private void DrawSpatialAudioProbeHUD(float scale)
        {
            if (SpatialAudioProbe.Instance == null || !SpatialAudioProbe.Instance.IsActive)
            {
                return;
            }

            float cardW = 540f * scale;
            float cardH = 76f * scale;
            float cx = (Screen.width - cardW) * 0.5f;
            float cy = 16f;
            Rect cardRect = new Rect(cx, cy, cardW, cardH);

            DrawTechCard(cardRect, new Color(0.02f, 0.05f, 0.09f, 0.92f), new Color(0.18f, 0.92f, 1f, 0.95f), scale);

            float azimuth = SpatialAudioProbe.Instance.CurrentAzimuth;
            string directionLabel;
            if (azimuth > -22.5f && azimuth <= 22.5f) directionLabel = "TRƯỚC MẶT (CENTER)";
            else if (azimuth > 22.5f && azimuth <= 67.5f) directionLabel = "TRƯỚC - PHẢI (FRONT-RIGHT)";
            else if (azimuth > 67.5f && azimuth <= 112.5f) directionLabel = "TAI PHẢI (RIGHT 100%)";
            else if (azimuth > 112.5f && azimuth <= 157.5f) directionLabel = "SAU - PHẢI (REAR-RIGHT)";
            else if (azimuth > 157.5f || azimuth <= -157.5f) directionLabel = "PHÍA SAU (BEHIND)";
            else if (azimuth > -157.5f && azimuth <= -112.5f) directionLabel = "SAU - TRÁI (REAR-LEFT)";
            else if (azimuth > -112.5f && azimuth <= -67.5f) directionLabel = "TAI TRÁI (LEFT 100%)";
            else directionLabel = "TRƯỚC - TRÁI (FRONT-LEFT)";

            GUI.Label(new Rect(cx + 16f * scale, cy + 8f * scale, cardW - 32f * scale, 22f * scale),
                $"🎧 THỬ NGHIỆM ÂM THANH 3D ĐANG BẬT // GÓC QUAY: {azimuth:+000;-000;000}°", headerTitleStyle);

            GUI.Label(new Rect(cx + 16f * scale, cy + 30f * scale, cardW - 32f * scale, 20f * scale),
                $"VỊ TRÍ ĐỊNH VỊ: {directionLabel} • BÁN KÍNH: {SpatialAudioProbe.Instance.OrbitRadius:F1}M", headerSubStyle);

            GUI.Label(new Rect(cx + 16f * scale, cy + 50f * scale, cardW - 32f * scale, 18f * scale),
                "[ Đeo tai nghe cảm nhận vòng xoay 360° • Nhấn phím T để Tắt/Bật ]", scoreLabelStyle);
        }

        #endregion

        #region Settings Modal & Keybind Rebinding

        private void DrawSettingsModal(float scale)
        {
            // Dim Fullscreen Backdrop
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.76f));

            float modalW = 660f * scale;
            float modalH = 540f * scale;
            float mx = (Screen.width - modalW) * 0.5f;
            float my = (Screen.height - modalH) * 0.5f;
            Rect modalRect = new Rect(mx, my, modalW, modalH);

            DrawTechCard(modalRect, new Color(0.04f, 0.06f, 0.09f, 0.97f), new Color(0.2f, 0.95f, 1f, 0.95f), scale);

            // Modal Header
            GUI.Label(new Rect(mx + 24f * scale, my + 16f * scale, modalW - 80f * scale, 32f * scale),
                "⚙ CÀI ĐẶT HỆ THỐNG // SYSTEM SETTINGS", modalTitleStyle);

            // Close Button [X]
            float closeSize = 34f * scale;
            if (GUI.Button(new Rect(mx + modalW - closeSize - 16f * scale, my + 14f * scale, closeSize, closeSize), "✕", actionButtonStyle))
            {
                GameSettings.IsSettingsOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GameSettings.CurrentlyRebindingAction = null;
            }

            // Tabs Bar
            float tabY = my + 56f * scale;
            float tabW = (modalW - 48f * scale) / 3f;
            float tabH = 36f * scale;

            DrawTabButton(new Rect(mx + 24f * scale, tabY, tabW, tabH), "⌨ PHÍM ĐIỀU KHIỂN", 0, scale);
            DrawTabButton(new Rect(mx + 24f * scale + tabW, tabY, tabW, tabH), "♫ ÂM THANH", 1, scale);
            DrawTabButton(new Rect(mx + 24f * scale + tabW * 2f, tabY, tabW, tabH), "🎮 GAMEPLAY", 2, scale);

            // Content Area Box
            float contentY = tabY + tabH + 12f * scale;
            float contentW = modalW - 48f * scale;
            float contentH = modalH - (contentY - my) - 62f * scale;
            Rect contentRect = new Rect(mx + 24f * scale, contentY, contentW, contentH);
            DrawRectWithOutline(contentRect, new Color(0.02f, 0.035f, 0.055f, 0.9f), new Color(0.18f, 0.92f, 1f, 0.35f), 1f);

            // Draw Active Tab Content
            switch (GameSettings.ActiveSettingsTab)
            {
                case 0:
                    DrawKeybindsTab(contentRect, scale);
                    break;
                case 1:
                    DrawAudioTab(contentRect, scale);
                    break;
                case 2:
                    DrawGameplayTab(contentRect, scale);
                    break;
            }

            // Bottom Buttons Bar
            float bottomY = my + modalH - 48f * scale;
            float btnW = 160f * scale;
            float btnH = 34f * scale;

            if (GUI.Button(new Rect(mx + 24f * scale, bottomY, btnW, btnH), "⟲ ĐẶT LẠI MẶC ĐỊNH", actionButtonStyle))
            {
                GameSettings.ResetToDefaults();
            }

            if (GUI.Button(new Rect(mx + modalW - btnW - 24f * scale, bottomY, btnW, btnH), "✓ LƯU & ĐÓNG", actionButtonStyle))
            {
                GameSettings.SaveSettings();
                GameSettings.IsSettingsOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GameSettings.CurrentlyRebindingAction = null;
            }
        }

        private void DrawTabButton(Rect rect, string text, int tabIndex, float scale)
        {
            bool isActive = GameSettings.ActiveSettingsTab == tabIndex;
            Color bg = isActive ? new Color(0.2f, 0.95f, 1f, 0.25f) : new Color(0.03f, 0.05f, 0.08f, 0.8f);
            Color border = isActive ? new Color(0.2f, 0.95f, 1f, 0.95f) : new Color(0.18f, 0.92f, 1f, 0.3f);
            DrawRectWithOutline(rect, bg, border, isActive ? 2f : 1f);

            tabButtonStyle.normal.textColor = isActive ? Color.white : new Color(0.65f, 0.75f, 0.85f);
            if (GUI.Button(rect, text, tabButtonStyle))
            {
                GameSettings.ActiveSettingsTab = tabIndex;
                GameSettings.CurrentlyRebindingAction = null;
            }
        }

        private void DrawKeybindsTab(Rect rect, float scale)
        {
            float rowH = 30f * scale;
            float startY = rect.y + 12f * scale;
            float colW = rect.width - 24f * scale;

            string[,] keybinds = new string[,]
            {
                { "Forward", "Di chuyển tiến (Forward)" },
                { "Backward", "Di chuyển lùi (Backward)" },
                { "Left", "Di chuyển trái (Strafe Left)" },
                { "Right", "Di chuyển phải (Strafe Right)" },
                { "Jump", "Nhảy (Jump)" },
                { "Fire1", "Bắn thường / Ném bom (Primary Fire)" },
                { "Fire2", "Bắn chùm tia Laser (Charged Laser)" },
                { "Reload", "Nạp đạn chiến thuật (Tactical Reload)" },
                { "Frag", "Cầm lựu đạn nổ (Frag Grenade)" },
                { "Smoke", "Cầm bom khói (Smoke Grenade)" }
            };

            for (int i = 0; i < keybinds.GetLength(0); i++)
            {
                string actionKey = keybinds[i, 0];
                string actionLabel = keybinds[i, 1];
                KeyCode currentKey = GameSettings.GetKeyForAction(actionKey);

                float y = startY + i * (rowH + 4f * scale);
                DrawKeybindRow(actionKey, actionLabel, currentKey, rect.x + 12f * scale, y, colW, rowH, scale);
            }
        }

        private void DrawKeybindRow(string actionKey, string label, KeyCode currentKey, float x, float y, float w, float h, float scale)
        {
            bool isRebinding = GameSettings.CurrentlyRebindingAction == actionKey;

            // Row zebra background
            DrawRect(new Rect(x, y, w, h), new Color(0.04f, 0.07f, 0.11f, 0.5f));

            // Action Label
            GUI.Label(new Rect(x + 10f * scale, y + 4f * scale, w - 180f * scale, h), label, keyLabelStyle);

            // Key Bind Button
            float btnW = 150f * scale;
            float btnH = h - 6f * scale;
            Rect btnRect = new Rect(x + w - btnW - 8f * scale, y + 3f * scale, btnW, btnH);

            string keyText = isRebinding ? "▶ BẤM PHÍM... ◀" : GameSettings.FormatKeyName(currentKey);
            Color btnBg = isRebinding ? new Color(1f, 0.45f, 0.05f, 0.4f) : new Color(0.1f, 0.16f, 0.24f, 0.85f);
            Color btnBorder = isRebinding ? new Color(1f, 0.65f, 0.15f, 0.95f) : new Color(0.2f, 0.95f, 1f, 0.65f);
            DrawRectWithOutline(btnRect, btnBg, btnBorder, isRebinding ? 2f : 1f);

            keyButtonStyle.normal.textColor = isRebinding ? new Color(1f, 0.9f, 0.3f) : new Color(0.9f, 0.95f, 1f);
            if (GUI.Button(btnRect, keyText, keyButtonStyle))
            {
                GameSettings.CurrentlyRebindingAction = isRebinding ? null : actionKey;
            }
        }

        private void HandleRebindingEvents()
        {
            if (string.IsNullOrEmpty(GameSettings.CurrentlyRebindingAction))
            {
                return;
            }

            Event e = Event.current;
            if (e == null)
            {
                return;
            }

            if (e.isKey && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    // Cancel rebinding
                    GameSettings.CurrentlyRebindingAction = null;
                    e.Use();
                }
                else if (e.keyCode != KeyCode.None)
                {
                    GameSettings.SetKeyForAction(GameSettings.CurrentlyRebindingAction, e.keyCode);
                    GameSettings.CurrentlyRebindingAction = null;
                    e.Use();
                }
            }
            else if (e.isMouse && e.type == EventType.MouseDown)
            {
                KeyCode mouseKey = e.button == 0 ? KeyCode.Mouse0 : (e.button == 1 ? KeyCode.Mouse1 : KeyCode.Mouse2);
                GameSettings.SetKeyForAction(GameSettings.CurrentlyRebindingAction, mouseKey);
                GameSettings.CurrentlyRebindingAction = null;
                e.Use();
            }
        }

        private void DrawAudioTab(Rect rect, float scale)
        {
            float y = rect.y + 24f * scale;
            float rowH = 46f * scale;
            float w = rect.width - 48f * scale;
            float x = rect.x + 24f * scale;

            float master = GameSettings.MasterVolume;
            if (DrawSliderRow("ÂM LƯỢNG TỔNG (MASTER)", ref master, 0f, 1f, "{0:P0}", x, y, w, scale))
            {
                GameSettings.MasterVolume = master;
                GameSettings.SaveSettings();
            }
            y += rowH;

            float sfx = GameSettings.SfxVolume;
            if (DrawSliderRow("HIỆU ỨNG ÂM THANH (SFX)", ref sfx, 0f, 1f, "{0:P0}", x, y, w, scale))
            {
                GameSettings.SfxVolume = sfx;
                GameSettings.SaveSettings();
            }
            y += rowH;

            float music = GameSettings.MusicVolume;
            if (DrawSliderRow("ÂM THANH MÔI TRƯỜNG (AMBIENCE)", ref music, 0f, 1f, "{0:P0}", x, y, w, scale))
            {
                GameSettings.MusicVolume = music;
                GameSettings.SaveSettings();
            }
            y += rowH + 6f * scale;

            bool dirAudio = GameSettings.DirectionalSoundIndicatorsEnabled;
            if (DrawToggleRow("CHỈ BÁO HƯỚNG ÂM THANH 3D TRÊN HUD", ref dirAudio, x, y, w, scale))
            {
                GameSettings.DirectionalSoundIndicatorsEnabled = dirAudio;
                GameSettings.SaveSettings();
            }
            y += rowH + 12f * scale;

            // 3D Audio Orbiting Probe Button
            bool isProbeActive = SpatialAudioProbe.Instance != null && SpatialAudioProbe.Instance.IsActive;
            float probeBtnW = 380f * scale;
            float probeBtnH = 38f * scale;
            string probeText = isProbeActive
                ? "🎧 ĐANG BẬT THỬ ÂM THANH 3D (BẤM ĐỂ TẮT / PHÍM T)"
                : "🎧 BẬT CHẾ ĐỘ THỬ ÂM THANH 3D XOAY 360° (PHÍM T)";

            Color probeBg = isProbeActive ? new Color(0.12f, 0.45f, 0.65f, 0.95f) : new Color(0.08f, 0.18f, 0.28f, 0.85f);
            Color probeBorder = isProbeActive ? new Color(0.2f, 0.95f, 1f, 1f) : new Color(0.18f, 0.92f, 1f, 0.65f);
            Rect probeRect = new Rect(x + (w - probeBtnW) * 0.5f, y, probeBtnW, probeBtnH);
            DrawRectWithOutline(probeRect, probeBg, probeBorder, 1.5f * scale);

            if (GUI.Button(probeRect, probeText, actionButtonStyle))
            {
                SpatialAudioProbe.Instance?.Toggle();
            }
            y += probeBtnH + 6f * scale;

            GUI.Label(new Rect(x, y, w, 28f * scale),
                "★ Đeo tai nghe: đầu dò 3D bay quanh đầu bạn ở bán kính 3.8m, phát âm thanh định vị rõ 4 hướng: Trước ➔ Phải ➔ Sau ➔ Trái ➔ Trước!",
                scoreLabelStyle);
            y += 34f * scale;

            // Test Sound Chime Button
            float testBtnW = 260f * scale;
            float testBtnH = 34f * scale;
            if (GUI.Button(new Rect(x + (w - testBtnW) * 0.5f, y, testBtnW, testBtnH), "♫ PHÁT THỬ CHUÔNG TIÊU DIỆT", actionButtonStyle))
            {
                AudioClip chime = VfxUtility.GetKillChimeAudio();
                if (chime != null)
                {
                    SpatialAudioUtility.PlayClipAtPoint3D(chime, Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 4f : Vector3.zero, GameSettings.SfxVolume);
                }
            }
        }

        private void DrawGameplayTab(Rect rect, float scale)
        {
            float y = rect.y + 24f * scale;
            float rowH = 46f * scale;
            float w = rect.width - 48f * scale;
            float x = rect.x + 24f * scale;

            float sens = GameSettings.MouseSensitivity;
            if (DrawSliderRow("ĐỘ NHẠY CHUỘT (MOUSE SENSITIVITY)", ref sens, 0.5f, 5.0f, "{0:F1}x", x, y, w, scale))
            {
                GameSettings.MouseSensitivity = sens;
                GameSettings.SaveSettings();
            }
            y += rowH;

            float crossScale = GameSettings.CrosshairScale;
            if (DrawSliderRow("KÍCH THƯỚC TÂM NGẮM (CROSSHAIR SCALE)", ref crossScale, 0.8f, 1.4f, "{0:P0}", x, y, w, scale))
            {
                GameSettings.CrosshairScale = crossScale;
                GameSettings.SaveSettings();
            }
            y += rowH + 6f * scale;

            bool shake = GameSettings.ScreenShakeEnabled;
            if (DrawToggleRow("RUNG MÀN HÌNH (SCREEN SHAKE)", ref shake, x, y, w, scale))
            {
                GameSettings.ScreenShakeEnabled = shake;
                GameSettings.SaveSettings();
            }
            y += rowH;

            bool hints = GameSettings.ShowKeyHints;
            if (DrawToggleRow("HIỂN THỊ GỢI Ý PHÍM TẮT TRÊN HUD (KEY HINTS)", ref hints, x, y, w, scale))
            {
                GameSettings.ShowKeyHints = hints;
                GameSettings.SaveSettings();
            }
        }

        private bool DrawSliderRow(string label, ref float value, float min, float max, string format, float x, float y, float w, float scale)
        {
            GUI.Label(new Rect(x, y, w - 80f * scale, 20f * scale), label, sliderLabelStyle);
            GUI.Label(new Rect(x + w - 70f * scale, y, 70f * scale, 20f * scale), string.Format(format, value), sliderValueStyle);

            float sliderW = w;
            float sliderH = 20f * scale;
            float newVal = GUI.HorizontalSlider(new Rect(x, y + 22f * scale, sliderW, sliderH), value, min, max);

            if (!Mathf.Approximately(newVal, value))
            {
                value = newVal;
                return true;
            }
            return false;
        }

        private bool DrawToggleRow(string label, ref bool value, float x, float y, float w, float scale)
        {
            GUI.Label(new Rect(x, y + 4f * scale, w - 120f * scale, 26f * scale), label, sliderLabelStyle);

            float btnW = 100f * scale;
            float btnH = 28f * scale;
            Rect btnRect = new Rect(x + w - btnW, y, btnW, btnH);

            string text = value ? "[ BẬT (ON) ]" : "[ TẮT (OFF) ]";
            Color bg = value ? new Color(0.2f, 0.95f, 1f, 0.35f) : new Color(0.1f, 0.12f, 0.15f, 0.65f);
            Color border = value ? new Color(0.2f, 0.95f, 1f, 0.95f) : new Color(0.4f, 0.45f, 0.5f, 0.5f);
            DrawRectWithOutline(btnRect, bg, border, 1f);

            settingToggleStyle.normal.textColor = value ? Color.white : new Color(0.6f, 0.65f, 0.7f);
            if (GUI.Button(btnRect, text, settingToggleStyle))
            {
                value = !value;
                return true;
            }
            return false;
        }

        #endregion

        #region Helper Drawing Primitives & Styles

        private static void DrawTechCard(Rect rect, Color fill, Color border, float scale)
        {
            // Backing Shadow
            DrawRect(new Rect(rect.x + 2f * scale, rect.y + 2f * scale, rect.width, rect.height), new Color(0f, 0f, 0f, 0.55f));
            // Card Body and Border
            DrawRectWithOutline(rect, fill, border, 1.5f * scale);

            // Corner tech notches
            float notch = 8f * scale;
            float notchT = 2f * scale;
            DrawRect(new Rect(rect.x, rect.y, notch, notchT), border);
            DrawRect(new Rect(rect.x, rect.y, notchT, notch), border);
            DrawRect(new Rect(rect.x + rect.width - notch, rect.y, notch, notchT), border);
            DrawRect(new Rect(rect.x + rect.width - notchT, rect.y, notchT, notch), border);
            DrawRect(new Rect(rect.x, rect.y + rect.height - notchT, notch, notchT), border);
            DrawRect(new Rect(rect.x, rect.y + rect.height - notch, notchT, notch), border);
            DrawRect(new Rect(rect.x + rect.width - notch, rect.y + rect.height - notchT, notch, notchT), border);
            DrawRect(new Rect(rect.x + rect.width - notchT, rect.y + rect.height - notch, notchT, notch), border);
        }

        private static void DrawSegmentedBar(Rect rect, float fillRatio, Color fillColor, int segments, float scale)
        {
            DrawRectWithOutline(rect, new Color(0.04f, 0.08f, 0.12f, 0.8f), new Color(0.18f, 0.92f, 1f, 0.4f), 1f);
            float segGap = 2f * scale;
            float totalGap = segGap * (segments - 1);
            float segW = (rect.width - 4f * scale - totalGap) / segments;
            float segH = rect.height - 4f * scale;
            int filledCount = Mathf.RoundToInt(fillRatio * segments);

            for (int i = 0; i < segments; i++)
            {
                float sx = rect.x + 2f * scale + i * (segW + segGap);
                float sy = rect.y + 2f * scale;
                Color col = i < filledCount ? fillColor : new Color(fillColor.r * 0.2f, fillColor.g * 0.2f, fillColor.b * 0.2f, 0.25f);
                DrawRect(new Rect(sx, sy, segW, segH), col);
            }
        }

        private static void DrawEnergyGradientBar(Rect rect, float progress, float scale)
        {
            DrawRectWithOutline(rect, new Color(0.05f, 0.07f, 0.09f, 0.9f), new Color(0.4f, 0.2f, 0.1f, 0.7f), 1f);

            int segments = 16;
            float gap = 2f * scale;
            float totalGap = gap * (segments - 1);
            float segW = (rect.width - 4f * scale - totalGap) / segments;
            float segH = rect.height - 4f * scale;
            int activeSegs = Mathf.RoundToInt(Mathf.Clamp01(progress) * segments);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Color col = Color.Lerp(new Color(1f, 0.12f, 0.03f), new Color(1f, 0.65f, 0.05f), t);
                if (i >= activeSegs)
                {
                    col = new Color(0.2f, 0.08f, 0.04f, 0.35f);
                }

                float sx = rect.x + 2f * scale + i * (segW + gap);
                float sy = rect.y + 2f * scale;
                DrawRect(new Rect(sx, sy, segW, segH), col);
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

        private void EnsureStyles()
        {
            if (headerTitleStyle != null)
            {
                return;
            }

            headerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };

            headerSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.85f, 0.9f, 0.98f) }
            };

            scoreLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.6f, 0.8f, 0.9f) }
            };

            scoreValueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.18f) }
            };

            ammoLargeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };

            ammoSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.5f, 0.6f, 0.7f) }
            };

            weaponNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            weaponModeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };

            reticleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f, 0.85f) }
            };

            killBannerTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            killBannerScoreStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.2f) }
            };

            modalTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };

            tabButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            keyLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.9f, 0.94f, 1f) }
            };

            keyButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            sliderLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.85f, 0.92f, 1f) }
            };

            sliderValueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };

            settingToggleStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            actionButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        #endregion
    }
}
